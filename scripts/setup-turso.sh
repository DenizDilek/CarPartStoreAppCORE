#!/bin/bash

# Turso Database Setup Script
# This script helps you set up the Turso database and configure environment files

set -e

echo "🚀 Turso Database Setup Script"
echo "================================="
echo ""

# Check if turso CLI is installed
if ! command -v turso &> /dev/null; then
    echo "❌ Turso CLI is not installed"
    echo "Please install it first: https://turso.tech/docs/cli/installation"
    exit 1
fi

# Check if user is logged in
if ! turso auth whoami &> /dev/null; then
    echo "🔐 Please login to Turso"
    turso auth login
fi

echo ""
echo "✅ Turso CLI is ready"
echo ""

# Ask for database name
read -p "Enter database name (default: carpartstore): " DB_NAME
DB_NAME=${DB_NAME:-carpartstore}

# Create database
echo ""
echo "📦 Creating database: $DB_NAME"
turso db create "$DB_NAME"

# Get database URL
echo ""
echo "🔗 Getting database URL..."
DB_URL=$(turso db show "$DB_NAME" --json | grep -o '"url":"[^"]*"' | cut -d'"' -f4)
echo "Database URL: $DB_URL"

# Create auth token
echo ""
echo "🔑 Creating auth token..."
AUTH_TOKEN=$(turso db tokens create "$DB_NAME")
echo "Auth token created"

# Update .env files
echo ""
echo "📝 Updating environment files..."

# Root .env
if [ -f .env ]; then
    sed -i "s|TURSO_DATABASE_URL=.*|TURSO_DATABASE_URL=$DB_URL|" .env
    sed -i "s|TURSO_AUTH_TOKEN=.*|TURSO_AUTH_TOKEN=$AUTH_TOKEN|" .env
    echo "✅ Updated .env"
else
    echo "❌ .env file not found"
fi

# React .env
if [ -f CarPartStoreWebApp/.env ]; then
    sed -i "s|VITE_TURSO_DATABASE_URL=.*|VITE_TURSO_DATABASE_URL=$DB_URL|" CarPartStoreWebApp/.env
    sed -i "s|VITE_TURSO_AUTH_TOKEN=.*|VITE_TURSO_AUTH_TOKEN=$AUTH_TOKEN|" CarPartStoreWebApp/.env
    echo "✅ Updated CarPartStoreWebApp/.env"
else
    echo "❌ CarPartStoreWebApp/.env file not found"
fi

# Initialize database schema
echo ""
echo "🏗️  Initializing database schema..."
turso db shell "$DB_NAME" <<'SQL'
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
SQL

echo "✅ Database schema created"

echo ""
echo "🎉 Setup complete!"
echo ""
echo "Your database is ready:"
echo "  - Database: $DB_NAME"
echo "  - URL: $DB_URL"
echo "  - Token configured"
echo ""
echo "You can now run your apps:"
echo "  - WPF App: npm run dev:wpf"
echo "  - React Web: npm run dev:web"
