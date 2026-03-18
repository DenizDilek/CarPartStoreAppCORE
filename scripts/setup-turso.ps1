# Turso Database Setup Script (PowerShell)
# This script helps you set up the Turso database and configure environment files

Write-Host "🚀 Turso Database Setup Script" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""

# Check if turso CLI is installed
$TursoPath = Get-Command turso -ErrorAction SilentlyContinue
if (-not $TursoPath) {
    Write-Host "❌ Turso CLI is not installed" -ForegroundColor Red
    Write-Host "Please install it first: https://turso.tech/docs/cli/installation"
    exit 1
}

# Check if user is logged in
$WhoamiResult = & turso auth whoami 2>&1
if ($LASTEXITCODE -ne 0 -or $WhoamiResult -like "*Not logged in*") {
    Write-Host "🔐 Please login to Turso" -ForegroundColor Yellow
    & turso auth login
}

Write-Host ""
Write-Host "✅ Turso CLI is ready" -ForegroundColor Green
Write-Host ""

# Ask for database name
$DB_NAME = Read-Host "Enter database name (default: carpartstore)"
if ([string]::IsNullOrWhiteSpace($DB_NAME)) {
    $DB_NAME = "carpartstore"
}

# Create database
Write-Host ""
Write-Host "📦 Creating database: $DB_NAME" -ForegroundColor Cyan
& turso db create $DB_NAME

# Get database URL
Write-Host ""
Write-Host "🔗 Getting database URL..." -ForegroundColor Cyan
$DB_JSON = & turso db show $DB_NAME --json
$DB_URL = ($DB_JSON | ConvertFrom-Json).url
Write-Host "Database URL: $DB_URL" -ForegroundColor Green

# Create auth token
Write-Host ""
Write-Host "🔑 Creating auth token..." -ForegroundColor Cyan
$AUTH_TOKEN = & turso db tokens create $DB_NAME --no-stdout
Write-Host "Auth token created" -ForegroundColor Green

# Update .env files
Write-Host ""
Write-Host "📝 Updating environment files..." -ForegroundColor Cyan

# Root .env
$EnvFile = ".env"
if (Test-Path $EnvFile) {
    $EnvContent = Get-Content $EnvFile -Raw
    $EnvContent = $EnvContent -replace "TURSO_DATABASE_URL=.*", "TURSO_DATABASE_URL=$DB_URL"
    $EnvContent = $EnvContent -replace "TURSO_AUTH_TOKEN=.*", "TURSO_AUTH_TOKEN=$AUTH_TOKEN"
    Set-Content $EnvFile $EnvContent -NoNewline
    Write-Host "✅ Updated .env" -ForegroundColor Green
} else {
    Write-Host "❌ .env file not found" -ForegroundColor Red
}

# React .env
$ReactEnvFile = "CarPartStoreWebApp\.env"
if (Test-Path $ReactEnvFile) {
    $ReactEnvContent = Get-Content $ReactEnvFile -Raw
    $ReactEnvContent = $ReactEnvContent -replace "VITE_TURSO_DATABASE_URL=.*", "VITE_TURSO_DATABASE_URL=$DB_URL"
    $ReactEnvContent = $ReactEnvContent -replace "VITE_TURSO_AUTH_TOKEN=.*", "VITE_TURSO_AUTH_TOKEN=$AUTH_TOKEN"
    Set-Content $ReactEnvFile $ReactEnvContent -NoNewline
    Write-Host "✅ Updated CarPartStoreWebApp/.env" -ForegroundColor Green
} else {
    Write-Host "❌ CarPartStoreWebApp/.env file not found" -ForegroundColor Red
}

# Initialize database schema
Write-Host ""
Write-Host "🏗️  Initializing database schema..." -ForegroundColor Cyan

$SQL = @"
CREATE TABLE IF NOT EXISTS Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    CreatedAt INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Parts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    CategoryId INTEGER,
    PartNumber TEXT,
    Description TEXT,
    Quantity INTEGER NOT NULL,
    Price REAL NOT NULL,
    Location TEXT,
    CreatedAt INTEGER NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

INSERT INTO Categories (Name, Description, CreatedAt) VALUES
    ('Engine', 'Engine components and parts', strftime('%s', 'now')),
    ('Brakes', 'Brake system components', strftime('%s', 'now')),
    ('Electrical', 'Electrical system parts', strftime('%s', 'now')),
    ('Suspension', 'Suspension and steering components', strftime('%s', 'now')),
    ('Exhaust', 'Exhaust system parts', strftime('%s', 'now'));

INSERT INTO Parts (Name, CategoryId, PartNumber, Description, Quantity, Price, Location, CreatedAt) VALUES
    ('Oil Filter', 1, 'OF-001', 'High-flow oil filter', 25, 12.99, 'Shelf A1', strftime('%s', 'now')),
    ('Brake Pads', 2, 'BP-002', 'Ceramic brake pads', 15, 45.99, 'Shelf B2', strftime('%s', 'now')),
    ('Battery', 3, 'BT-003', '12V car battery', 8, 89.99, 'Shelf C1', strftime('%s', 'now'));
"@

# Save SQL to temp file
$TempSqlFile = [System.IO.Path]::GetTempFileName()
$SQL | Out-File -FilePath $TempSqlFile -Encoding UTF8

# Execute SQL
& turso db shell $DB_NAME --file $TempSqlFile

# Clean up temp file
Remove-Item $TempSqlFile -Force

Write-Host "✅ Database schema created" -ForegroundColor Green

Write-Host ""
Write-Host "🎉 Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Your database is ready:" -ForegroundColor Cyan
Write-Host "  - Database: $DB_NAME"
Write-Host "  - URL: $DB_URL"
Write-Host "  - Token configured"
Write-Host ""
Write-Host "You can now run your apps:" -ForegroundColor Cyan
Write-Host "  - WPF App: npm run dev:wpf"
Write-Host "  - React Web: npm run dev:web"
