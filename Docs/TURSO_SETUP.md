# Turso Database Setup Guide

This guide helps you set up and configure Turso as the cloud database for CarPartStoreApp.

## Prerequisites

1. **Turso CLI installed** - Follow the installation guide at https://turso.tech/docs/cli/installation
2. **Turso account** - Sign up at https://turso.tech (free tier available)
3. **Git Bash or WSL** - For running the setup script

## Quick Setup (Recommended)

### Option 1: Use the Setup Script

The setup script automates database creation, environment configuration, and schema initialization.

```bash
# Make the script executable (Git Bash/Linux)
chmod +x scripts/setup-turso.sh

# Run the setup script
./scripts/setup-turso.sh
```

The script will:
- Create a new Turso database
- Generate an auth token
- Update your `.env` files with the database URL and token
- Initialize the database schema

### Option 2: Manual Setup

If you prefer manual setup:

#### Step 1: Create Database

```bash
# Create a new database
turso db create carpartstore

# Verify it was created
turso db list
```

#### Step 2: Get Database URL and Token

```bash
# Get database URL
turso db show carpartstore

# Create auth token
turso db tokens create carpartstore
```

Save both values - you'll need them for the next step.

#### Step 3: Update Environment Files

Update your `.env` files with the database URL and auth token:

**Root directory `.env`:**
```env
TURSO_DATABASE_URL=libsql://your-database-url-here.turso.io
TURSO_AUTH_TOKEN=your-auth-token-here
API_URL=http://localhost:5000
```

**React app `.env` (`CarPartStoreWebApp/.env`):**
```env
VITE_TURSO_DATABASE_URL=libsql://your-database-url-here.turso.io
VITE_TURSO_AUTH_TOKEN=your-auth-token-here
VITE_API_URL=http://localhost:5000
```

#### Step 4: Initialize Database Schema

```bash
# Create the database schema
turso db shell carpartstore
```

Then run these SQL commands:

```sql
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

-- Add sample categories
INSERT INTO Categories (Name, Description, CreatedAt) VALUES
    ('Engine', 'Engine components and parts', strftime('%s', 'now')),
    ('Brakes', 'Brake system components', strftime('%s', 'now')),
    ('Electrical', 'Electrical system parts', strftime('%s', 'now')),
    ('Suspension', 'Suspension and steering components', strftime('%s', 'now')),
    ('Exhaust', 'Exhaust system parts', strftime('%s', 'now'));

-- Add sample parts
INSERT INTO Parts (Name, CategoryId, PartNumber, Description, Quantity, Price, Location, CreatedAt) VALUES
    ('Oil Filter', 1, 'OF-001', 'High-flow oil filter', 25, 12.99, 'Shelf A1', strftime('%s', 'now')),
    ('Brake Pads', 2, 'BP-002', 'Ceramic brake pads', 15, 45.99, 'Shelf B2', strftime('%s', 'now')),
    ('Battery', 3, 'BT-003', '12V car battery', 8, 89.99, 'Shelf C1', strftime('%s', 'now'));
```

## Using the Database

### WPF Application

The WPF app uses the `DatabaseConfig` class to read environment variables:

```csharp
using CarPartStoreApp.Configuration;

var config = new DatabaseConfig();
var dbUrl = config.DatabaseUrl;
var authToken = config.AuthToken;
```

### React Web Application

The React app uses the `turso` configuration module:

```typescript
import { tursoConfig } from './config/turso';

const { databaseUrl, authToken, apiUrl } = tursoConfig;
```

## Managing Your Database

### View Database Contents

```bash
turso db shell carpartstore
```

### Run SQL Queries

```bash
turso db execute carpartstore "SELECT * FROM Parts"
```

### Inspect Database Schema

```bash
turso db shell carpartstore ".schema"
```

### Drop and Recreate Database

```bash
turso db destroy carpartstore
turso db create carpartstore
```

## Environment Variables

| Variable | WPF (.env) | React (.env) | Description |
|----------|------------|--------------|-------------|
| Database URL | `TURSO_DATABASE_URL` | `VITE_TURSO_DATABASE_URL` | Turso database URL |
| Auth Token | `TURSO_AUTH_TOKEN` | `VITE_TURSO_AUTH_TOKEN` | Database authentication token |
| API URL | `API_URL` | `VITE_API_URL` | Embedded API server URL |

## Troubleshooting

### "TURSO_DATABASE_URL is not configured"

Make sure your `.env` file exists in the correct location and contains the database URL:
- WPF app: Root directory `.env` file
- React app: `CarPartStoreWebApp/.env` file

### Turso CLI not found

Install the Turso CLI: https://turso.tech/docs/cli/installation

### Auth token expired

Generate a new auth token:
```bash
turso db tokens create carpartstore
```

Update your `.env` file with the new token.

### Connection issues

- Check your internet connection
- Verify the database URL is correct
- Ensure the auth token is valid
- Check Turso status: https://status.turso.tech

## Next Steps

1. Update the WPF app's `SqliteDataService` to connect to Turso instead of local SQLite
2. Update the React app's API client to use Turso directly or through the WPF API
3. Test the connection with both applications
4. Deploy the React app to Vercel with environment variables

## Additional Resources

- [Turso Documentation](https://turso.tech/docs)
- [Turso CLI Reference](https://turso.tech/docs/cli/introduction)
- [LibSQL Documentation](https://turso.tech/docs/libsql/introduction)
