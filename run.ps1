#Requires -Version 7.0
<#
.SYNOPSIS
    Universal entry point for toolup-forge-io. One command starts the
    Tailwind build + the dotnet server, serving the site on
    http://localhost:13940/.

.DESCRIPTION
    Stage-1 shape per the workspace `run.ps1` mandate
    (../CLAUDE.md "Every new sibling app ships a run.ps1 at its repo root").
    Wraps the standard build + run sequence so contributors can drop into
    the repo and `pwsh ./run.ps1` without remembering tool-restore /
    Tailwind-compile / dotnet-run incantations.

.PARAMETER SkipFormat
    Skip the Fantomas check before build. Use when iterating on a known-good
    formatted state to shave a few seconds off the inner loop.

.PARAMETER SkipBuild
    Skip the dotnet build pass. Implies SkipFormat. Use when launching
    against an already-built bin/ to skip MSBuild evaluation entirely.

.PARAMETER SkipTailwind
    Skip the Tailwind CSS compile. Use when the site CSS is current and the
    edit is server-side-only.
#>

[CmdletBinding()]
param(
    [switch] $SkipFormat,
    [switch] $SkipBuild,
    [switch] $SkipTailwind
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

# Sibling launcher conventions (../CLAUDE.md "Sibling launcher conventions
# (mandate)"). Node 22.x npm.ps1 / npx.ps1 mangle args when invoked from
# inside another .ps1 via `& npm`; Invoke-Npm / Invoke-Npx skip the shim
# by resolving npm.cmd / npx.cmd directly.
function Invoke-Npm {
    [CmdletBinding()]
    param([Parameter(ValueFromRemainingArguments = $true)] $Arguments)
    $cmd = Get-Command npm.cmd -CommandType Application -ErrorAction Stop
    & $cmd.Source @Arguments
}

function Invoke-Npx {
    [CmdletBinding()]
    param([Parameter(ValueFromRemainingArguments = $true)] $Arguments)
    $cmd = Get-Command npx.cmd -CommandType Application -ErrorAction Stop
    & $cmd.Source @Arguments
}

Write-Host ""
Write-Host "==> toolup-forge-io ==> bootstrapping..." -ForegroundColor Cyan
Write-Host ""

# 1. dotnet tool restore (Fantomas + FAKE)
Write-Host "==> dotnet tool restore" -ForegroundColor Cyan
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed" }

# 2. npm install (Tailwind CLI + autoprefixer). Skipped under -SkipTailwind.
if (-not $SkipTailwind) {
    if (-not (Test-Path "./node_modules")) {
        Write-Host ""
        Write-Host "==> npm install (Tailwind + postcss)" -ForegroundColor Cyan
        Invoke-Npm install --no-fund --no-audit
        if ($LASTEXITCODE -ne 0) { throw "npm install failed" }
    }
}

# 3. Fantomas format check on .fs files. Skipped under -SkipFormat.
if (-not $SkipFormat -and -not $SkipBuild) {
    Write-Host ""
    Write-Host "==> fantomas --check src/" -ForegroundColor Cyan
    dotnet fantomas --check src/ Build.fs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Fantomas check failed. Run 'dotnet fantomas src/ Build.fs' to format." -ForegroundColor Red
        throw "Fantomas check failed"
    }
}

# 4. Tailwind compile. Skipped under -SkipTailwind.
if (-not $SkipTailwind) {
    Write-Host ""
    Write-Host "==> tailwind build" -ForegroundColor Cyan
    Invoke-Npm run build:css
    if ($LASTEXITCODE -ne 0) { throw "Tailwind build failed" }
}

# 5. dotnet build the server project. Skipped under -SkipBuild.
if (-not $SkipBuild) {
    Write-Host ""
    Write-Host "==> dotnet build" -ForegroundColor Cyan
    dotnet build src/Server/ToolUpForge.Site.fsproj --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }
}

# 6. Run the server. Ctrl+C terminates.
Write-Host ""
Write-Host "==> dotnet run — serving on http://localhost:13940/" -ForegroundColor Green
Write-Host ""
dotnet run --project src/Server/ToolUpForge.Site.fsproj --no-build
