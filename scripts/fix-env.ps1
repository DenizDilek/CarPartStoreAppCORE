# Script to fix .env file with Turso database URL

Write-Host "Fixing .env file for Turso database..." -ForegroundColor Cyan

# Check if turso CLI is available
$TursoPath = Get-Command turso -ErrorAction SilentlyContinue
if (-not $TursoPath) {
    Write-Host "ERROR: Turso CLI not found. Please install Turso or manually add TURSO_DATABASE_URL to .env file." -ForegroundColor Red
    exit 1
}

# Check if logged in
try {
    turso auth whoami | Out-Null
} catch {
    Write-Host "ERROR: Not logged into Turso. Please run: turso auth login" -ForegroundColor Red
    exit 1
}

# Get database URL
Write-Host "Getting Turso database URL..." -ForegroundColor Cyan
try {
    $DB_JSON = turso db show carpartstore --json
    $DB_URL = ($DB_JSON | ConvertFrom-Json).url

    if ([string]::IsNullOrWhiteSpace($DB_URL)) {
        Write-Host "ERROR: Could not find database 'carpartstore'" -ForegroundColor Red
        Write-Host "Available databases:" -ForegroundColor Yellow
        turso db list
        exit 1
    }

    Write-Host "OK: Found database URL: $DB_URL" -ForegroundColor Green

    # Update .env file
    $EnvFile = ".env"
    if (-not (Test-Path $EnvFile)) {
        Write-Host "ERROR: .env file not found" -ForegroundColor Red
        exit 1
    }

    # Read current .env content
    $EnvContent = Get-Content $EnvFile -Raw

    # Replace or add TURSO_DATABASE_URL
    if ($EnvContent -match "TURSO_DATABASE_URL\s*=") {
        $EnvContent = $EnvContent -replace "TURSO_DATABASE_URL\s*=.*", "TURSO_DATABASE_URL=$DB_URL"
        Write-Host "OK: Updated TURSO_DATABASE_URL in .env file" -ForegroundColor Green
    } else {
        $EnvContent += "`nTURSO_DATABASE_URL=$DB_URL"
        Write-Host "OK: Added TURSO_DATABASE_URL to .env file" -ForegroundColor Green
    }

    # Write back to .env file
    Set-Content $EnvFile $EnvContent -NoNewline

    Write-Host ""
    Write-Host "SUCCESS: .env file updated successfully!" -ForegroundColor Green
    Write-Host "ACTION: Please restart your WPF app to use Turso database" -ForegroundColor Yellow

} catch {
    Write-Host "ERROR: Could not get database URL: $_" -ForegroundColor Red
    Write-Host "ACTION: You can manually add the URL to .env file:" -ForegroundColor Yellow
    Write-Host "TURSO_DATABASE_URL=libsql://your-database-url-here" -ForegroundColor Cyan
    exit 1
}
