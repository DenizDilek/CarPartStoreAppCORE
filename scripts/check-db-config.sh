#!/bin/bash

# Database Configuration Diagnostic Script (WSL/bash version)

echo "🔍 Database Configuration Diagnostic"
echo "======================================"
echo ""

# Check .env file
ENV_FILE=".env"
if [ ! -f "$ENV_FILE" ]; then
    echo "❌ ERROR: .env file not found"
    exit 1
fi

echo "✅ OK: .env file found"
echo ""

# Read and parse .env file
echo "📋 Environment Variables:"
HAS_URL=false
HAS_TOKEN=false
DATABASE_URL=""

while IFS='=' read -r key value; do
    # Skip comments and empty lines
    [[ $key =~ ^#.*$ ]] && continue
    [[ -z $key ]] && continue

    key=$(echo "$key" | xargs)
    value=$(echo "$value" | xargs)

    if [ "$key" = "TURSO_DATABASE_URL" ]; then
        if [ -z "$value" ]; then
            echo "⚠️  WARNING: TURSO_DATABASE_URL is EMPTY"
        else
            HAS_URL=true
            DATABASE_URL="$value"
            echo "✅ OK: TURSO_DATABASE_URL is set"
            echo "   Value: $value"
        fi
    fi

    if [ "$key" = "TURSO_AUTH_TOKEN" ]; then
        if [ -z "$value" ]; then
            echo "⚠️  WARNING: TURSO_AUTH_TOKEN is EMPTY"
        else
            HAS_TOKEN=true
            echo "✅ OK: TURSO_AUTH_TOKEN is set"
        fi
    fi
done < "$ENV_FILE"

echo ""

# Check configuration status
echo "🔧 Configuration Status:"
if [ "$HAS_URL" = true ] && [ "$HAS_TOKEN" = true ]; then
    echo "✅ OK: Turso is properly configured"
    echo "💡 INFO: WPF app will use Turso cloud database"
else
    echo "❌ ERROR: Turso is NOT properly configured"

    if [ "$HAS_URL" = false ]; then
        echo "⚠️  WARNING: Missing TURSO_DATABASE_URL"
    fi
    if [ "$HAS_TOKEN" = false ]; then
        echo "⚠️  WARNING: Missing TURSO_AUTH_TOKEN"
    fi

    echo ""
    echo "💡 INFO: WPF app will fall back to local SQLite"
    echo "📝 ACTION: Run './scripts/fix-env.sh' to fix this"
fi

echo ""
echo "======================================"
echo "💡 Quick Fix Options:"
echo "1. Run: ./scripts/fix-env.sh"
echo "2. Or manually add your Turso URL to .env file"
echo "======================================"
