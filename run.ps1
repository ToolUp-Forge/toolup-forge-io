#Requires -Version 7.0
<#
.SYNOPSIS
  toolup-forge-io entry point: bring up the server (SSR-only Kestrel
  on port 13940) and print the browser URL. The server runs in the
  background; the prompt returns immediately.

.DESCRIPTION
  Stage-1+ shape per the workspace `CLAUDE.md` "Every new sibling app
  ships a run.ps1" mandate. Thin pass-through to
  `dev-scripts/launch-toolup-forge-io.ps1`, which orchestrates the
  pre-flight stale-process sweep, the SyncDocs + Tailwind build, the
  dotnet build, and the background server start.

  Three modes:

    pwsh ./run.ps1                  # default — launch the server and
                                    #           leave it running in the
                                    #           background
    pwsh ./run.ps1 -Check           # verify-only — tool restore +
                                    #           fantomas check + build,
                                    #           no server start
    pwsh ./run.ps1 -StopOnly        # sweep stale dotnet processes from
                                    #           prior runs and exit

  The website is SSR-only (no Fable, no Vite), so launch is a single
  process and the URL is the Kestrel server directly:

    http://localhost:13940

.EXAMPLE
  pwsh ./run.ps1

  Full happy path: pre-flight cleanup, SyncDocs + Tailwind, build,
  server start, /health probe, browser URL. Returns to the prompt with
  the server running.

.EXAMPLE
  pwsh ./run.ps1 -Check

  Verify: fantomas check + dotnet build. No server start. Same
  invocation as in CI.

.EXAMPLE
  pwsh ./run.ps1 -StopOnly

  Kill any toolup-forge-io processes from a prior launch.

.EXAMPLE
  pwsh ./run.ps1 -SkipBuild -SkipTailwind

  Fastest possible restart: skips everything except the pre-flight
  sweep, the server start, and the /health probe. Use when you're
  iterating purely on content/*.md files (which are re-read on each
  request) and the server binary is already built.
#>
[CmdletBinding()]
param(
    # Verify-only mode (no server start).
    [switch] $Check,
    # Skip the Fantomas format check.
    [switch] $SkipFormat,
    # Skip the dotnet build pass.
    [switch] $SkipBuild,
    # Skip SyncDocs + Tailwind. Implies the existing wwwroot/css/site.css
    # and content/docs/ are current.
    [switch] $SkipTailwind,
    # How long to wait for the server's first /health 200.
    [int] $TimeoutSeconds,
    # Sweep stale processes and exit.
    [switch] $StopOnly
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

# ─── -Check: verify-only mode (fantomas + build) ─────────────────────
if ($Check) {
    function Write-Step {
        param([string] $message)
        Write-Host ""
        Write-Host "── $message ──────────────────────────────────────────────" -ForegroundColor Cyan
    }

    Write-Step "dotnet tool restore"
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { Write-Error "dotnet tool restore failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }

    if (-not $SkipFormat) {
        Write-Step "fantomas --check"
        dotnet fantomas --check src/ Build.fs
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Fantomas check failed — run 'dotnet fantomas src/ Build.fs' to format in place."
            exit $LASTEXITCODE
        }
    }

    if (-not $SkipBuild) {
        Write-Step "dotnet build toolup-forge-io.sln"
        dotnet build toolup-forge-io.sln --nologo
        if ($LASTEXITCODE -ne 0) { Write-Error "Build failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }
    }

    Write-Host ""
    Write-Host "✓ Verify passed." -ForegroundColor Green
    exit 0
}

# ─── Default: delegate to the launcher ───────────────────────────────
$inner = Join-Path $PSScriptRoot "dev-scripts/launch-toolup-forge-io.ps1"

if (-not (Test-Path $inner)) {
    Write-Error "Expected launcher not found at $inner"
    exit 1
}

# Forward only the switches the caller actually passed (so the inner
# script's own defaults apply otherwise). Hashtable splatting binds by
# NAME — array splatting is positional and would mis-bind.
$splat = @{}
foreach ($name in $PSBoundParameters.Keys) {
    if ($name -in @('SkipBuild', 'SkipTailwind', 'SkipFormat', 'TimeoutSeconds', 'StopOnly')) {
        $splat[$name] = $PSBoundParameters[$name]
    }
}

Write-Host "toolup-forge-io: launching server -> dev-scripts/launch-toolup-forge-io.ps1 $(($splat.Keys | ForEach-Object { "-$_" }) -join ' ')"
Write-Host ""

& $inner @splat
exit $LASTEXITCODE
