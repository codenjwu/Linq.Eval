#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publish Linq.Eval package to NuGet.org

.DESCRIPTION
    This script publishes the Linq.Eval NuGet package to NuGet.org.
    Requires a valid NuGet API key.

.PARAMETER ApiKey
    NuGet API key for authentication. If not provided, will prompt for it.

.PARAMETER PackageVersion
    Specific version to publish. If not provided, will use the latest package found.

.PARAMETER SkipDuplicate
    Skip if the package version already exists on NuGet.org

.PARAMETER DryRun
    Simulate the publish without actually uploading

.EXAMPLE
    .\publish-nuget.ps1
    
.EXAMPLE
    .\publish-nuget.ps1 -ApiKey "YOUR_API_KEY"
    
.EXAMPLE
    .\publish-nuget.ps1 -PackageVersion "1.1.0"
    
.EXAMPLE
    .\publish-nuget.ps1 -DryRun
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ApiKey,
    
    [Parameter(Mandatory=$false)]
    [string]$PackageVersion,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipDuplicate,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

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

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

# Get script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$packageOutputPath = Join-Path $scriptPath "Linq.Eval\bin\Release"

Write-Host "=" * 60 -ForegroundColor Yellow
Write-Host "  Linq.Eval NuGet Publisher" -ForegroundColor Yellow
Write-Host "=" * 60 -ForegroundColor Yellow

if ($DryRun) {
    Write-Warning-Custom "DRY RUN MODE - No actual upload will occur"
}

# Step 1: Find package
Write-Step "Locating package..."
$packagePattern = if ($PackageVersion) { "Linq.Eval.$PackageVersion.nupkg" } else { "Linq.Eval.*.nupkg" }
$packageFiles = Get-ChildItem -Path $packageOutputPath -Filter $packagePattern -ErrorAction SilentlyContinue

if (-not $packageFiles) {
    Write-Error-Custom "No package found matching '$packagePattern' in $packageOutputPath"
    Write-Host "`nRun .\build-package.ps1 first to create the package" -ForegroundColor Yellow
    exit 1
}

# Use the most recent package if multiple found
$package = $packageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$packagePath = $package.FullName
$size = [math]::Round($package.Length / 1KB, 2)

Write-Success "Found package:"
Write-Host "  Name: $($package.Name)" -ForegroundColor White
Write-Host "  Size: $size KB" -ForegroundColor White
Write-Host "  Path: $packagePath" -ForegroundColor Gray

# Step 2: Get API Key
if (-not $ApiKey) {
    Write-Host "`nNuGet API Key required for publishing." -ForegroundColor Cyan
    Write-Host "Get your API key from: https://www.nuget.org/account/apikeys" -ForegroundColor Gray
    
    $secureApiKey = Read-Host "Enter your NuGet API key" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureApiKey)
    $ApiKey = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    
    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        Write-Error-Custom "API key is required!"
        exit 1
    }
}

# Step 3: Confirmation
if (-not $DryRun) {
    Write-Host "`n" + ("=" * 60) -ForegroundColor Yellow
    Write-Host "  Ready to Publish" -ForegroundColor Yellow
    Write-Host ("=" * 60) -ForegroundColor Yellow
    Write-Host "  Package: $($package.Name)" -ForegroundColor White
    Write-Host "  Target:  NuGet.org" -ForegroundColor White
    Write-Host ("=" * 60) -ForegroundColor Yellow
    
    $confirmation = Read-Host "`nDo you want to proceed with publishing? (yes/no)"
    if ($confirmation -ne "yes") {
        Write-Warning-Custom "Publishing cancelled by user"
        exit 0
    }
}

# Step 4: Publish
Write-Step "Publishing to NuGet.org..."

if ($DryRun) {
    Write-Warning-Custom "DRY RUN: Would execute:"
    Write-Host "  dotnet nuget push `"$packagePath`" --api-key ****** --source https://api.nuget.org/v3/index.json" -ForegroundColor Gray
    Write-Success "Dry run completed - no actual upload performed"
} else {
    try {
        $pushArgs = @(
            "nuget", "push",
            $packagePath,
            "--api-key", $ApiKey,
            "--source", "https://api.nuget.org/v3/index.json"
        )
        
        if ($SkipDuplicate) {
            $pushArgs += "--skip-duplicate"
        }
        
        $output = & dotnet $pushArgs 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Package published successfully!"
        } else {
            Write-Error-Custom "Publish failed!"
            Write-Host $output -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Error-Custom "Publish failed: $_"
        exit 1
    }
}

# Summary
Write-Host "`n" + ("=" * 60) -ForegroundColor Yellow
Write-Host "  Publish Summary" -ForegroundColor Yellow
Write-Host ("=" * 60) -ForegroundColor Yellow

if ($DryRun) {
    Write-Host "  Status: " -NoNewline
    Write-Host "DRY RUN" -ForegroundColor Yellow
} else {
    Write-Host "  Status: " -NoNewline
    Write-Host "SUCCESS" -ForegroundColor Green
    Write-Host "  Package: $($package.Name)" -ForegroundColor White
    Write-Host "  URL: https://www.nuget.org/packages/Linq.Eval/" -ForegroundColor Cyan
}

Write-Host ("=" * 60) -ForegroundColor Yellow

if (-not $DryRun) {
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "  1. Verify package at: https://www.nuget.org/packages/Linq.Eval/"
    Write-Host "  2. Create Git tag: git tag -a v$($package.Name -replace 'Linq.Eval.(\d+\.\d+\.\d+)\.nupkg','$1') -m 'Release v...'"
    Write-Host "  3. Push tag: git push origin --tags"
    Write-Host "  4. Create GitHub Release"
    Write-Host ""
}
