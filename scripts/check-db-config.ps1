# Database Configuration Diagnostic Script

Write-Host "Database Configuration Diagnostic" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check .env file
$EnvFile = ".env"
if (-not (Test-Path $EnvFile)) {
    Write-Host "ERROR: .env file not found" -ForegroundColor Red
    exit 1
}

Write-Host "OK: .env file found" -ForegroundColor Green
Write-Host ""

# Read .env file
$EnvContent = Get-Content $EnvFile
$HasUrl = $false
$HasToken = $false
$DatabaseUrl = ""

Write-Host "Environment Variables:" -ForegroundColor Yellow

foreach ($Line in $EnvContent) {
    $Line = $Line.Trim()
    if ($Line -eq "" -or $Line.StartsWith("#")) {
        continue
    }

    $Parts = $Line -split '=', 2
    if ($Parts.Length -eq 2) {
        $Key = $Parts[0].Trim()
        $Value = $Parts[1].Trim()

        if ($Key -eq "TURSO_DATABASE_URL") {
            if ([string]::IsNullOrWhiteSpace($Value)) {
                Write-Host "WARNING: TURSO_DATABASE_URL is EMPTY" -ForegroundColor Yellow
            } else {
                $HasUrl = $true
                $DatabaseUrl = $Value
                Write-Host "OK: TURSO_DATABASE_URL is set" -ForegroundColor Green
                Write-Host "Value: $Value" -ForegroundColor Gray
            }
        }

        if ($Key -eq "TURSO_AUTH_TOKEN") {
            $HasToken = $true
            if ([string]::IsNullOrWhiteSpace($Value)) {
                Write-Host "WARNING: TURSO_AUTH_TOKEN is EMPTY" -ForegroundColor Yellow
            } else {
                Write-Host "OK: TURSO_AUTH_TOKEN is set" -ForegroundColor Green
            }
        }
    }
}

Write-Host ""

# Check configuration status
Write-Host "Configuration Status:" -ForegroundColor Yellow
if ($HasUrl -and $HasToken) {
    Write-Host "OK: Turso is properly configured" -ForegroundColor Green
    Write-Host "INFO: WPF app will use Turso cloud database" -ForegroundColor Cyan
} else {
    Write-Host "ERROR: Turso is NOT properly configured" -ForegroundColor Red

    if (-not $HasUrl) {
        Write-Host "WARNING: Missing TURSO_DATABASE_URL" -ForegroundColor Yellow
    }
    if (-not $HasToken) {
        Write-Host "WARNING: Missing TURSO_AUTH_TOKEN" -ForegroundColor Yellow
    }

    Write-Host "INFO: WPF app will fall back to local SQLite" -ForegroundColor Yellow
    Write-Host "ACTION: Run 'npm run fix:env' to fix this" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Quick Fix Options:" -ForegroundColor Cyan
Write-Host "1. Run: npm run fix:env" -ForegroundColor Gray
Write-Host "2. Or manually add your Turso URL to .env file" -ForegroundColor Gray
Write-Host "======================================" -ForegroundColor Cyan
