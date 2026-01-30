#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build NuGet package for Linq.Eval

.DESCRIPTION
    This script cleans, builds, and tests the Linq.Eval project in Release mode,
    then generates the NuGet package.

.EXAMPLE
    .\build-package.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Colors for output
function Write-Step {
    param([string]$Message)
    Write-Host "`n[$(Get-Date -Format 'HH:mm:ss')] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

# Get script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $scriptPath "Linq.Eval.sln"
$projectPath = Join-Path $scriptPath "Linq.Eval\Linq.Eval.csproj"
$packageOutputPath = Join-Path $scriptPath "Linq.Eval\bin\Release"

Write-Host "=" * 60 -ForegroundColor Yellow
Write-Host "  Linq.Eval NuGet Package Builder" -ForegroundColor Yellow
Write-Host "=" * 60 -ForegroundColor Yellow

# Step 1: Clean
Write-Step "Cleaning previous builds..."
try {
    dotnet clean $solutionPath -c Release -v q
    Write-Success "Clean completed"
} catch {
    Write-Error-Custom "Clean failed: $_"
    exit 1
}

# Step 2: Restore
Write-Step "Restoring NuGet packages..."
try {
    dotnet restore $solutionPath -v q
    Write-Success "Restore completed"
} catch {
    Write-Error-Custom "Restore failed: $_"
    exit 1
}

# Step 3: Build
Write-Step "Building solution (Release mode)..."
try {
    dotnet build $solutionPath -c Release --no-restore -v q
    Write-Success "Build completed"
} catch {
    Write-Error-Custom "Build failed: $_"
    exit 1
}

# Step 4: Run tests
Write-Step "Running tests..."
try {
    $testResult = dotnet test $solutionPath -c Release --no-build --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Success "All tests passed"
    } else {
        Write-Error-Custom "Tests failed"
        exit 1
    }
} catch {
    Write-Error-Custom "Test execution failed: $_"
    exit 1
}

# Step 5: Verify package
Write-Step "Verifying package..."
$packageFiles = Get-ChildItem -Path $packageOutputPath -Filter "*.nupkg" -ErrorAction SilentlyContinue

if ($packageFiles) {
    Write-Success "Package(s) found:"
    foreach ($pkg in $packageFiles) {
        $size = [math]::Round($pkg.Length / 1KB, 2)
        Write-Host "  - $($pkg.Name) ($size KB)" -ForegroundColor White
    }
} else {
    Write-Error-Custom "No package files found!"
    exit 1
}

# Summary
Write-Host "`n" + ("=" * 60) -ForegroundColor Yellow
Write-Host "  Build Summary" -ForegroundColor Yellow
Write-Host ("=" * 60) -ForegroundColor Yellow
Write-Host "  Status: " -NoNewline
Write-Host "SUCCESS" -ForegroundColor Green
Write-Host "  Output: $packageOutputPath" -ForegroundColor White
Write-Host ("=" * 60) -ForegroundColor Yellow

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "  1. Review the package contents"
Write-Host "  2. Run .\publish-nuget.ps1 to publish to NuGet.org"
Write-Host ""
