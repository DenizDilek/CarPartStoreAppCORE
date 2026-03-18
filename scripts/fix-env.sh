#!/bin/bash

# Script to fix .env file with Turso database URL (WSL/bash version)

echo "🔧 Fixing .env file for Turso database..."

# Check if turso CLI is available
if ! command -v turso &> /dev/null; then
    echo "❌ ERROR: Turso CLI not found. Please install Turso or manually add TURSO_DATABASE_URL to .env file."
    exit 1
fi

# Check if logged in
if ! turso auth whoami &> /dev/null; then
    echo "❌ ERROR: Not logged into Turso. Please run: turso auth login"
    exit 1
fi

# Get database URL
echo "📦 Getting Turso database URL..."
DB_JSON=$(turso db show carpartstore --json)
DB_URL=$(echo "$DB_JSON" | grep -o '"url":"[^"]*"' | cut -d'"' -f4)

if [ -z "$DB_URL" ]; then
    echo "❌ ERROR: Could not find database 'carpartstore'"
    echo "📝 Available databases:"
    turso db list
    exit 1
fi

echo "✅ OK: Found database URL: $DB_URL"

# Update .env file
ENV_FILE=".env"
if [ ! -f "$ENV_FILE" ]; then
    echo "❌ ERROR: .env file not found"
    exit 1
fi

# Read current .env content
ENV_CONTENT=$(cat "$ENV_FILE")

# Check if TURSO_DATABASE_URL exists
if echo "$ENV_CONTENT" | grep -q "TURSO_DATABASE_URL="; then
    # Update existing TURSO_DATABASE_URL
    ENV_CONTENT=$(echo "$ENV_CONTENT" | sed "s|TURSO_DATABASE_URL=.*|TURSO_DATABASE_URL=$DB_URL|")
    echo "✅ OK: Updated TURSO_DATABASE_URL in .env file"
else
    # Add new TURSO_DATABASE_URL
    echo "" >> "$ENV_FILE"
    echo "TURSO_DATABASE_URL=$DB_URL" >> "$ENV_FILE"
    echo "✅ OK: Added TURSO_DATABASE_URL to .env file"
fi

# Write back to .env file
echo "$ENV_CONTENT" > "$ENV_FILE"

echo ""
echo "✅ SUCCESS: .env file updated successfully!"
echo "🔄 ACTION: Please restart your WPF app to use Turso database"
