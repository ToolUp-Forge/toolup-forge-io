#Requires -Version 7.0
<#
.SYNOPSIS
  Start the toolup-forge-io server (Kestrel on 13940) — wait for it to
  respond — print the URL — return to the prompt with the server
  running in the background.

.NOTES
  Requires PowerShell 7+ (`pwsh`). Adapted from Constellation's
  `dev-scripts/launch-constellation.ps1`. Website-class deployment:
  SSR-only F# via `ToolUp.PublicRendering`, no Fable client, no Vite
  dev server — so this is a single-process launcher, lighter than
  Constellation's. The pre-flight stale-process sweep + the
  background-process logging shape are kept verbatim.

.DESCRIPTION
  Steps:
    0. Pre-flight: sweep stale dotnet processes from earlier runs
       (durable PID file + port-owner check + cmdline match).
    1. (Optional) `dotnet build src/Server/`.
    2. (Optional) Tailwind compile + SyncDocs via the FAKE driver.
    3. Start the Server (Kestrel, port 13940) as a background process.
    4. Wait for the server to respond on `/health`.
    5. Print the browser URL.

  Process is left running. Stop with `Stop-Process -Id <pid>` (the
  script prints the PID at the end) or by re-running with `-StopOnly`.

.PARAMETER SkipBuild
  Skip `dotnet build`. Use when you've just built and want a faster
  restart.

.PARAMETER SkipTailwind
  Skip the Tailwind CSS compile + SyncDocs sync. Use when the site
  CSS + synced docs are current.

.PARAMETER SkipFormat
  Skip Fantomas in the build step.

.PARAMETER TimeoutSeconds
  How long to wait for the server's first `/health` 200. Defaults to
  60 (cold starts on a fresh checkout pay the NuGet restore tax; warm
  restarts are usually under 5s).

.PARAMETER StopOnly
  Run the stale-process cleanup pass and exit without launching.
#>
[CmdletBinding()]
param(
    [switch] $SkipBuild,
    [switch] $SkipTailwind,
    [switch] $SkipFormat,
    [int]    $TimeoutSeconds = 60,
    [switch] $StopOnly
)

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
$repoRoot = Split-Path -Parent $scriptDir

Push-Location $repoRoot

# Sibling launcher conventions — see workspace CLAUDE.md "Sibling launcher
# conventions (mandate)". Copy-pasted from the canonical body there; do not
# diverge without updating the workspace doc.
function Invoke-Npm {
    [CmdletBinding()]
    param([Parameter(ValueFromRemainingArguments = $true)] $Arguments)
    $cmd = Get-Command npm.cmd -CommandType Application -ErrorAction Stop |
           Select-Object -First 1
    & $cmd.Source @Arguments
}

function Invoke-Npx {
    [CmdletBinding()]
    param([Parameter(ValueFromRemainingArguments = $true)] $Arguments)
    $cmd = Get-Command npx.cmd -CommandType Application -ErrorAction Stop |
           Select-Object -First 1
    & $cmd.Source @Arguments
}

$allProcesses = New-Object System.Collections.Generic.List[object]

function Register-Proc {
    param($Proc, [string] $Label)
    $allProcesses.Add(@{ Process = $Proc; Label = $Label }) | Out-Null
}

function Start-Background {
    # Named-only. `-Arguments $a $b` does NOT put $b in the array — it binds $b POSITIONALLY to
    # $WorkingDirectory, which is a [string] and so accepts it SILENTLY: the process would launch
    # in the wrong directory with an argument missing, and nothing would say so. (Phase 925:
    # `-Compare <a> <b>` bound <b> onto -Render, took the render branch and exited 0 having
    # compared nothing.) Every call site below already passes by name and wraps its array in @()
    # or parentheses, so this changes no behaviour today — it forecloses the next edit that would
    # not.
    [CmdletBinding(PositionalBinding = $false)]
    param(
        [string]   $FilePath,
        [string[]] $Arguments,
        [string]   $WorkingDirectory,
        [string]   $LogFile,
        [string]   $Label
    )
    # Remove-Item can fail with a sharing violation if a server from a
    # prior run is still holding the log open. Pre-flight should have
    # killed it but defend in depth — rename a locked file out of the
    # way so the new process gets a clean handle.
    foreach ($f in @($LogFile, "$LogFile.err")) {
        if (Test-Path $f) {
            try {
                Remove-Item $f -Force -ErrorAction Stop
            }
            catch {
                $bak = "$f.locked-$(Get-Date -Format 'HHmmss')"
                try { Rename-Item $f $bak -Force -ErrorAction Stop } catch { }
            }
        }
    }
    $proc = Start-Process `
        -FilePath $FilePath `
        -ArgumentList $Arguments `
        -WorkingDirectory $WorkingDirectory `
        -RedirectStandardOutput $LogFile `
        -RedirectStandardError "$LogFile.err" `
        -PassThru -WindowStyle Hidden
    Add-Content -Path (Join-Path $repoRoot ".toolup-forge-io-pids") -Value $proc.Id -ErrorAction SilentlyContinue
    Register-Proc $proc $Label
    return $proc
}

function Wait-Server {
    # Kestrel binds to `0.0.0.0` (IPv4-only); `127.0.0.1` reaches it
    # without ambiguity (vs `localhost`, which can resolve to `::1`).
    param([int] $Port, [string] $Path, [int] $Timeout, [string] $Probe = "127.0.0.1")
    $deadline = [DateTime]::UtcNow.AddSeconds($Timeout)
    while ([DateTime]::UtcNow -lt $deadline) {
        try {
            $r = Invoke-WebRequest -Uri "http://${Probe}:$Port$Path" `
                -TimeoutSec 2 -SkipHttpErrorCheck -ErrorAction Stop
            if ($r -and $r.StatusCode -eq 200) {
                Write-Host "  Server is up on port $Port (status $($r.StatusCode))."
                return $true
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }
    return $false
}

function Stop-StaleProcesses {
    # Sweep dotnet processes left behind by prior runs. Three signals:
    #   1. .toolup-forge-io-pids — durable PID file written by
    #      Start-Background; the highest-confidence signal.
    #   2. Port owners (13940 server only — no Vite in v0).
    #   3. Command-line substring match — catches anything whose command
    #      line references the workspace path or the ToolUpForge.Site
    #      project identifier.
    param(
        [string] $WorkspaceRoot,
        [int[]]  $Ports = @(13940)
    )

    $rootBack = $WorkspaceRoot.Replace('/', '\').TrimEnd('\')
    $rootFwd = $rootBack.Replace('\', '/')
    $pidFile = Join-Path $WorkspaceRoot ".toolup-forge-io-pids"

    $filePids = @()
    if (Test-Path $pidFile) {
        $filePids =
            Get-Content $pidFile -ErrorAction SilentlyContinue |
                Where-Object { $_ -match '^\d+$' } |
                ForEach-Object { [int] $_ }
    }

    # Filter port owners to dotnet.exe only — toolup-forge-io only ever
    # spawns the one Kestrel process.
    $portPids =
        Get-NetTCPConnection -LocalPort $Ports -State Listen -ErrorAction SilentlyContinue |
            Where-Object {
                $p = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
                $p -and $p.ProcessName -eq 'dotnet'
            } |
            Select-Object -ExpandProperty OwningProcess -Unique

    $cmdPids =
        Get-CimInstance Win32_Process `
            -Filter "Name = 'dotnet.exe'" `
            -ErrorAction SilentlyContinue |
            Where-Object {
                $_.CommandLine -and (
                    $_.CommandLine -like "*$rootBack*" -or
                    $_.CommandLine -like "*$rootFwd*" -or
                    $_.CommandLine -like "*ToolUpForge.Site*"
                )
            } | Select-Object -ExpandProperty ProcessId

    $allPids =
        @($filePids) + @($portPids) + @($cmdPids) |
            Where-Object { $_ } |
            Sort-Object -Unique

    if (@($allPids).Count -eq 0) {
        Write-Host "Pre-flight: no stale toolup-forge-io processes found."
    }
    else {
        Write-Host "Pre-flight: stopping $(@($allPids).Count) stale process(es):"
        foreach ($procId in $allPids) {
            $proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
            $label = if ($proc) { "$($proc.ProcessName) (PID $procId)" } else { "PID $procId" }
            Write-Host "  $label"
            Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
        }
        Start-Sleep -Milliseconds 800
    }

    if (Test-Path $pidFile) { Remove-Item $pidFile -Force -ErrorAction SilentlyContinue }
}

try {
    # ─── 0. Pre-flight cleanup ───────────────────────────────────────
    Stop-StaleProcesses -WorkspaceRoot $repoRoot

    if ($StopOnly) {
        Write-Host ""
        Write-Host "Done (-StopOnly)."
        return
    }

    # ─── 1. dotnet tool restore (Fantomas) ───────────────────────────
    Write-Host ""
    Write-Host "Restoring local dotnet tools (Fantomas)..."
    & dotnet tool restore | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet tool restore failed. Is .config/dotnet-tools.json present?"
        exit 3
    }

    # ─── 2. Fantomas format check (optional) ─────────────────────────
    if (-not $SkipFormat -and -not $SkipBuild) {
        Write-Host ""
        Write-Host "Running fantomas --check..."
        & dotnet fantomas --check src/ Build.fs
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Fantomas check failed — run 'dotnet fantomas src/ Build.fs' to format in place."
            exit 4
        }
    }

    # ─── 3. npm install if needed ────────────────────────────────────
    if (-not $SkipTailwind) {
        if (-not (Test-Path (Join-Path $repoRoot "node_modules"))) {
            $hasLock = Test-Path (Join-Path $repoRoot "package-lock.json")
            $npmSub = if ($hasLock) { "ci" } else { "install" }
            Write-Host ""
            Write-Host "Running npm $npmSub (one-time)..."
            Invoke-Npm $npmSub --no-fund --no-audit
            if ($LASTEXITCODE -ne 0) {
                Write-Host "npm $npmSub failed (exit $LASTEXITCODE); retrying once..." -ForegroundColor Yellow
                Invoke-Npm $npmSub --no-fund --no-audit
            }
            if ($LASTEXITCODE -ne 0) {
                Write-Error "npm $npmSub failed (see output above)."
                exit 5
            }
        }
    }

    # ─── 4. SyncDocs + Tailwind via FAKE driver ──────────────────────
    # SyncDocs first (populates content/docs/ from ../toolup-forge/docs/);
    # the FAKE chain "SyncDocs ==> Tailwind" runs both in order.
    # Tailwind compiles src/Server/styles/index.css to wwwroot/css/site.css
    # and is what makes the site render with brand colours.
    if (-not $SkipTailwind) {
        Write-Host ""
        Write-Host "Running SyncDocs + Tailwind via Build.fs..."
        & dotnet run --project Build.fsproj -- Tailwind
        if ($LASTEXITCODE -ne 0) {
            Write-Error "SyncDocs + Tailwind step failed."
            exit 6
        }
    }

    # ─── 5. Build the server project ─────────────────────────────────
    if (-not $SkipBuild) {
        Write-Host ""
        Write-Host "Building src/Server/ToolUpForge.Site.fsproj..."
        & dotnet build src/Server/ToolUpForge.Site.fsproj --nologo --verbosity quiet
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed."
            exit 7
        }
    }
    else {
        Write-Host ""
        Write-Host "Skipping build."
    }

    # ─── 6. Start the server ─────────────────────────────────────────
    Write-Host ""
    Write-Host "Starting toolup-forge-io server (Kestrel, port 13940)..."
    $serverLog = Join-Path $repoRoot "toolup-forge-io-server.log"
    $serverProc = Start-Background `
        -FilePath "dotnet" `
        -Arguments @("run", "--no-build", "--project", "src/Server/ToolUpForge.Site.fsproj") `
        -WorkingDirectory $repoRoot `
        -LogFile $serverLog `
        -Label "Server"
    Write-Host "  Server PID: $($serverProc.Id)  (log: $serverLog)"

    # ─── 7. Wait for server ──────────────────────────────────────────
    # /health is the SDK's fast liveness probe — bypasses auth, static
    # files, and rate limiting — so it returns 200 as soon as Kestrel
    # is bound and the app pipeline is ready. Same path that App
    # Service's warmup probe uses on the production deploy.
    Write-Host ""
    Write-Host "Waiting for server (/health, timeout ${TimeoutSeconds}s)..."
    if (-not (Wait-Server -Port 13940 -Path "/health" -Timeout $TimeoutSeconds)) {
        Write-Host ""
        Write-Host "Server failed to respond on /health within ${TimeoutSeconds}s." -ForegroundColor Red
        Write-Host "  Log: $serverLog (+ .err)"
        Write-Host ""
        Write-Host "Leaving the process running so you can inspect it; stop with:"
        Write-Host "  pwsh ./run.ps1 -StopOnly"
        exit 8
    }

    Write-Host ""
    Write-Host "toolup-forge-io is live. Open in your browser:" -ForegroundColor Green
    Write-Host "  http://localhost:13940"
}
finally {
    if ($allProcesses.Count -gt 0 -and -not $StopOnly) {
        Write-Host ""
        Write-Host "Processes left running:"
        foreach ($entry in $allProcesses) {
            Write-Host ("  {0,-8} PID {1}" -f $entry.Label, $entry.Process.Id)
        }
        Write-Host ""
        $pids = @($allProcesses | ForEach-Object { $_.Process.Id })
        Write-Host "Stop with:"
        Write-Host "  pwsh ./run.ps1 -StopOnly"
        Write-Host "  # or:  Stop-Process -Id $($pids -join ',')"
    }

    Pop-Location
}
