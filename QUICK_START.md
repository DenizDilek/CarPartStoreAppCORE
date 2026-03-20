# Quick Start Guide

## 🚀 Getting Started with CarPartStoreApp

### Prerequisites
- .NET 10.0 Windows SDK
- Node.js (for React web app)
- Turso account (optional, for cloud database)

### 1. Install Dependencies

```bash
npm run install:all
```

### 2. Choose Database Type

**Option A: Use Local SQLite (Default - No Setup)**
- Works offline, no configuration needed
- Database stored locally at `%LOCALAPPDATA%\CarPartStoreApp\CarPartStore.db`
- Just run the app!

**Option B: Use Turso Cloud Database (Recommended)**
- Cloud database accessible from anywhere
- Real-time sync between WPF and React apps
- Run the setup script:

```bash
npm run setup:turso
```

**Troubleshooting Database Configuration:**

If you encounter issues with Turso setup, run the diagnostic:

**For PowerShell/Windows:**
```bash
npm run check:db        # Check configuration
npm run fix:env         # Fix empty TURSO_DATABASE_URL
```

**For WSL:**
```bash
npm run check:db:wsl    # Check configuration
npm run fix:env:wsl     # Fix empty TURSO_DATABASE_URL
```

This will:
- Create a Turso database
- Generate an auth token
- Update your `.env` files automatically
- Initialize the database schema

### 3. Run the Apps

**Option 1: Run Both Apps (Recommended)**
- Open two terminals in VSCode (use Ctrl+Shift+5 to split)
- Terminal 1: `npm run dev:wpf`
- Terminal 2: `npm run dev:web`

**Option 2: Run Individually**
```bash
# WPF App
npm run dev:wpf

# React Web App
npm run dev:web
```

### 4. Access the Apps

- **WPF App**: Opens as a desktop application
- **React Web App**: Opens at http://localhost:5173
- **API Documentation**: http://localhost:5000/swagger

## 📁 Environment Files

### Root `.env` (WPF App)
```env
# Turso Database Configuration (only needed for cloud database)
TURSO_DATABASE_URL=libsql://your-database-url-here
TURSO_AUTH_TOKEN=your-auth-token-here

# API Configuration
API_URL=http://localhost:5000
```

### React App `.env` (`CarPartStoreWebApp/.env`)
```env
# API Configuration
VITE_API_URL=http://localhost:5000

# Turso Configuration (if needed for direct access)
VITE_TURSO_DATABASE_URL=libsql://your-database-url-here
VITE_TURSO_AUTH_TOKEN=your-auth-token-here
```

## 🔧 How Environment Variables Work

### WPF App
The app automatically loads `.env` files from:
- Current directory `.env`
- Parent directory `.env`
- App base directory `.env`

No additional setup needed - just create/update the `.env` file!

### React App
Vite automatically loads `.env` files from the `CarPartStoreWebApp` directory:
- `.env` - Default environment
- `.env.local` - Local overrides
- `.env.development` - Development only
- `.env.production` - Production only

**Important:** React environment variables must start with `VITE_` to be available in the browser.

## 🎯 Database Selection

The app automatically chooses the database based on configuration:

```javascript
IF TURSO_DATABASE_URL AND TURSO_AUTH_TOKEN are set:
    → Use Turso Cloud Database
ELSE:
    → Use Local SQLite Database
```

### Switch Between Databases

**To use Turso Cloud:**
1. Run `npm run setup:turso`
2. Make sure `.env` files have TURSO_DATABASE_URL and TURSO_AUTH_TOKEN
3. Restart the app

**To use Local SQLite:**
1. Comment out or remove Turso variables from `.env`
2. Restart the app

## 🛠️ Available Scripts

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

# Installation
npm run install:all       # Install all dependencies
```

## 📝 Database Schema

### Categories Table
- Id, Name, Description, ParentCategoryId, DisplayOrder, CreatedDate

### Parts Table
- Id, PartNumber, Name, Description, CategoryId, CostPrice, RetailPrice, StockQuantity, Location, Supplier, ImagePath, CreatedDate, LastUpdated

## 🐛 Troubleshooting

### "TURSO_DATABASE_URL is not configured"
- **Solution**: Run `npm run setup:turso` or remove Turso variables from `.env`

### React app can't connect to API
- **Solution**: Make sure WPF app is running (`npm run dev:wpf`)
- **Solution**: Check `VITE_API_URL` in `CarPartStoreWebApp/.env`

### Build errors
- **Solution**: Close any running instances of the app
- **Solution**: Run `npm run build:all` to rebuild everything

### Database not working
- **Solution**: Check console output to see which database is being used
- **Solution**: Verify `.env` files are in the correct locations

## 📚 Additional Resources

- [Database Configuration Guide](Docs/DATABASE_CONFIGURATION.md)
- [Turso Setup Guide](Docs/TURSO_SETUP.md)
- [React Documentation](https://react.dev)
- [WPF Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)

## 💡 Tips

1. **Use Turso** for production - provides cloud hosting and real-time sync
2. **Use SQLite** for development - works offline and is faster
3. **Check console output** - shows which database is being used
4. **Restart apps** after changing `.env` files
5. **Never commit** `.env` files to version control

## 🎉 You're Ready!

That's it! You now have a fully functional car parts inventory management system with both WPF desktop and React web interfaces.

Start by running `npm run dev` to see usage instructions, or run the apps directly in separate terminals.
