# Database Configuration Guide

## Overview

CarPartStoreApp supports **two database backends**:

1. **Turso Cloud Database** (Recommended) - SQLite-compatible cloud database
2. **Local SQLite Database** - Traditional file-based database

The app automatically chooses the appropriate database based on your environment configuration.

## Quick Start

### Option 1: Use Turso Cloud Database (Recommended)

1. **Setup Turso:**
   ```bash
   npm run setup:turso
   ```

2. **Run the app:**
   ```bash
   npm run dev:wpf    # WPF App
   npm run dev:web    # React Web App
   ```

The app will automatically detect Turso configuration and use the cloud database.

### Option 2: Use Local SQLite Database

1. **No configuration needed** - Just run the app:
   ```bash
   npm run dev:wpf
   ```

The app will fall back to local SQLite if Turso is not configured.

## Environment Variables

### Root `.env` file (WPF App)

```env
# Turso Database Configuration
TURSO_DATABASE_URL=libsql://your-database-url-here
TURSO_AUTH_TOKEN=your-auth-token-here

# API Configuration
API_URL=http://localhost:5000
```

### React App `.env` file (`CarPartStoreWebApp/.env`)

```env
# Turso Database Configuration
VITE_TURSO_DATABASE_URL=libsql://your-database-url-here
VITE_TURSO_AUTH_TOKEN=your-auth-token-here

# API Configuration
VITE_API_URL=http://localhost:5000
```

## How It Works

### Database Selection Logic

The app uses the following logic to choose a database:

```
IF TURSO_DATABASE_URL AND TURSO_AUTH_TOKEN are set:
    Use Turso Cloud Database
ELSE:
    Use Local SQLite Database
```

### Component Architecture

1. **`DatabaseConfig`** - Reads environment variables and determines database type
2. **`DataServiceFactory`** - Creates the appropriate data service instance
3. **`TursoDataService`** - Handles cloud database operations
4. **`SqliteDataService`** - Handles local database operations

### Initialization Flow

```
App Startup
    ↓
DataServiceFactory.GetDataService()
    ↓
Check Turso Configuration
    ↓
IF Turso Configured:
    └─→ TursoDataService
ELSE:
    └─→ SqliteDataService (with local initialization)
```

## Turso Setup

### Initial Setup

Run the automated setup script:

```powershell
# PowerShell (Windows)
powershell -ExecutionPolicy Bypass -File scripts/setup-turso.ps1
```

Or manually:

```bash
# Create database
turso db create carpartstore

# Get database URL
turso db show carpartstore

# Create auth token
turso db tokens create carpartstore

# Update .env files with the URL and token
```

### Managing Your Database

```bash
# View database contents
turso db shell carpartstore

# Run SQL queries
turso db execute carpartstore "SELECT * FROM Parts"

# Inspect schema
turso db shell carpartstore ".schema"

# List databases
turso db list
```

## Local SQLite Database

### Database Location

- **Windows:** `%LOCALAPPDATA%\CarPartStoreApp\CarPartStore.db`
- **Mac/Linux:** `~/.local/share/CarPartStoreApp/CarPartStore.db`

### Initialization

The local database is automatically initialized on first run with:
- Required tables (`Parts`, `Categories`)
- Sample data (10 categories, 7 parts)
- Database indexes for performance

### Features

- **Single-file database** - No server required
- **WAL mode** - Better concurrency for API access
- **Foreign key constraints** - Data integrity
- **Connection pooling** - Performance optimization

## Development Scripts

### Available Scripts

```bash
# Development
npm run dev:wpf           # Run WPF app
npm run dev:web           # Run React web app
npm run dev               # Show usage instructions

# Build
npm run build:wpf         # Build WPF app
npm run build:web         # Build React web app
npm run build:all         # Build both apps

# Setup
npm run setup:turso       # Setup Turso database

# Utilities
npm run check:env         # Check environment variables
npm run load:env          # Load environment variables
```

## Troubleshooting

### "TURSO_DATABASE_URL is not configured"

**Solution:** Run the Turso setup script or update your `.env` file:
```bash
npm run setup:turso
```

### "Failed to initialize Turso database"

**Cause:** Invalid credentials or network issues

**Solution:**
1. Check your internet connection
2. Verify the database URL and token are correct
3. Generate a new auth token:
   ```bash
   turso db tokens create carpartstore
   ```

### App falls back to local SQLite unexpectedly

**Cause:** Environment variables not loaded

**Solution:**
1. Check that `.env` file exists in the correct location
2. Verify variable names are correct
3. Use `npm run check:env` to verify loaded variables

### WPF app can't connect to Turso

**Cause:** Network connectivity or authentication issues

**Solution:**
1. Verify internet connection
2. Check if the Turso service is running: https://status.turso.tech
3. Generate a new auth token and update `.env`

### React app can't connect to API

**Cause:** WPF app not running or wrong API URL

**Solution:**
1. Make sure WPF app is running (`npm run dev:wpf`)
2. Check `VITE_API_URL` in `.env` file
3. Verify WPF app is running on correct port (default: 5000)

## Switching Between Databases

### From Local to Turso

1. Setup Turso: `npm run setup:turso`
2. Update `.env` files with Turso credentials
3. Restart the app

The app will automatically use Turso.

### From Turso to Local

1. Comment out or remove Turso variables in `.env`:
   ```env
   # TURSO_DATABASE_URL=
   # TURSO_AUTH_TOKEN=
   ```

2. Restart the app

The app will automatically use local SQLite.

## Database Schema

### Categories Table

```sql
CREATE TABLE Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    ParentCategoryId INTEGER,
    DisplayOrder INTEGER DEFAULT 0,
    CreatedDate TEXT NOT NULL,
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id)
);
```

### Parts Table

```sql
CREATE TABLE Parts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartNumber TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Description TEXT,
    CategoryId INTEGER,
    CostPrice REAL NOT NULL DEFAULT 0,
    RetailPrice REAL NOT NULL DEFAULT 0,
    StockQuantity INTEGER NOT NULL DEFAULT 0,
    Location TEXT,
    Supplier TEXT,
    ImagePath TEXT,
    CreatedDate TEXT NOT NULL,
    LastUpdated TEXT,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

## Advanced Configuration

### Custom Database Names

To use a custom database name with Turso:

1. Create a custom database:
   ```bash
   turso db create my-custom-db
   ```

2. Update `.env` with the custom database URL

### Multiple Environments

Create environment-specific `.env` files:

- `.env.development` - Development environment
- `.env.production` - Production environment

The app will automatically load the appropriate file based on the build mode.

## Performance Tips

### Turso Optimization

1. **Use indexes** on frequently queried columns
2. **Batch operations** when possible
3. **Use connection pooling** (automatic)

### SQLite Optimization

1. **WAL mode** is enabled by default
2. **Indexes** are created on common queries
3. **Connection pooling** is enabled

## Security Notes

- **Never commit** `.env` files to version control
- **Use strong auth tokens** for Turso
- **Rotate tokens regularly** for production
- **Use environment variables** in production deployments

## Additional Resources

- [Turso Documentation](https://turso.tech/docs)
- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [.NET SQLite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/)
- [Vite Environment Variables](https://vitejs.dev/guide/env-and-mode.html)
