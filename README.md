# CarPartStoreAppCORE

A comprehensive car parts inventory management system with both WPF desktop and React web applications.

## 🚀 Project Overview

CarPartStoreAppCORE is a modern inventory management solution that allows users to:
- Store and categorize car parts with stock and price information
- Track inventory quantities and locations
- Search and filter parts by various criteria
- Synchronize data between desktop and web applications (future)

## 📁 Architecture

```
CarPartStoreAppCORE/
├── CarPartStoreApp/          # WPF Desktop Application (C#)
│   ├── Models/               # Data models (CarPart, Category)
│   ├── ViewModels/           # MVVM ViewModels
│   ├── Views/                # XAML UI
│   ├── Services/             # Data access and business logic
│   ├── Controllers/          # REST API endpoints
│   ├── DTOs/                 # Data Transfer Objects
│   └── Data/                 # Database initialization
├── CarPartStoreWebApp/       # React Web Application
│   ├── src/
│   │   ├── components/       # React components
│   │   ├── services/         # API client
│   │   ├── hooks/            # React Query hooks
│   │   ├── pages/            # Page components
│   │   └── types/            # TypeScript interfaces
│   └── public/               # Static assets
├── Docs/                     # Documentation
│   ├── Deployment.md         # Cloud migration research
│   └── TursoResearch.md      # Turso database research
└── package.json              # Root scripts for both apps
```

## 🛠️ Technology Stack

### Desktop Application (WPF)
- **Framework**: WPF with MVVM pattern
- **Runtime**: .NET 10.0 Windows
- **Database**: SQLite (current) → Turso (planned)
- **API**: ASP.NET Core embedded server
- **Documentation**: Swagger/OpenAPI

### Web Application (React)
- **Framework**: React 19.2.4
- **Build Tool**: Vite 8.0.0
- **Language**: TypeScript
- **State Management**: TanStack Query
- **Routing**: React Router DOM
- **HTTP Client**: Axios

### Database
- **Current**: Local SQLite (single-file, no server)
- **Planned**: Turso Cloud Database (libSQL-based, SQLite-compatible)

## ✨ Features

### Desktop Application
- 📊 Inventory management with categories and parts
- 🔍 Search and filter functionality
- 📈 Stock tracking and pricing
- 🌐 Embedded REST API for web sync
- 💾 SQLite database with WAL mode

### Web Application
- 🎨 Modern, responsive UI
- 🔄 Real-time data synchronization
- 📱 Mobile-friendly interface
- 🔐 API integration with desktop app

## 🚀 Getting Started

### Prerequisites

- **Desktop**: .NET 10.0 SDK or Visual Studio 2022 with .NET workload
- **Web**: Node.js 18+ and npm

### Installation

```bash
# Clone the repository
git clone https://github.com/your-username/CarPartStoreAppCORE.git
cd CarPartStoreAppCORE

# Install web dependencies
npm run install:all

# Restore NuGet packages (if needed)
cd CarPartStoreApp
dotnet restore
```

### Running the Applications

#### Desktop Application (WPF)

```bash
# Using npm script
npm run dev:wpf

# Or using dotnet CLI
cd CarPartStoreApp
dotnet run
```

The WPF app will start with an embedded API server at `http://localhost:5000`.

#### Web Application (React)

```bash
# Using npm script
npm run dev:web

# Or using npm directly
cd CarPartStoreWebApp
npm run dev
```

The React app will start at `http://localhost:5173`.

#### Running Both Applications

Open two terminals and run:
```bash
# Terminal 1 - Desktop app
npm run dev:wpf

# Terminal 2 - Web app
npm run dev:web
```

## 📦 Build Commands

```bash
# Build desktop application
npm run build:wpf
# or
cd CarPartStoreApp
dotnet build

# Build web application
npm run build:web
# or
cd CarPartStoreWebApp
npm run build

# Build both applications
npm run build:all
```

## 🔧 Configuration

### Database Location

The SQLite database is stored at:
```
%LOCALAPPDATA%\CarPartStoreApp\CarPartStore.db
```

The database is automatically initialized on first run with:
- 10 pre-configured categories
- 7 sample car parts

### API Configuration

- **Base URL**: `http://localhost:5000`
- **Swagger Documentation**: `http://localhost:5000/swagger`
- **CORS**: Enabled for `http://localhost:5173`

## 🌐 API Endpoints

### Parts
- `GET /api/parts` - Get all parts
- `GET /api/parts/{id}` - Get part by ID
- `POST /api/parts` - Create new part
- `PUT /api/parts/{id}` - Update part
- `DELETE /api/parts/{id}` - Delete part

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create new category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

## 🚧 Current Status

### ✅ Completed
- WPF application structure with MVVM pattern
- SQLite database with full CRUD operations
- Embedded ASP.NET Core API server
- React web application with API integration
- Swagger API documentation
- Basic UI components and navigation

### 🔄 In Progress
- Enhanced desktop UI with data binding
- Complete CRUD operations in UI
- Search and filtering functionality
- Real-time synchronization between apps

### 📋 Planned
- Migration to Turso cloud database
- User authentication and authorization
- Advanced inventory reporting
- Multi-location support
- Image upload for parts
- Mobile app (React Native)

## 🗄️ Database Migration

The project is planned to migrate from local SQLite to **Turso Cloud Database**:

### Why Turso?
- ✅ SQLite-compatible (minimal code changes)
- ✅ Built-in REST API
- ✅ Edge deployment (works with Vercel)
- ✅ Free tier (1GB storage)
- ✅ Real-time synchronization
- ✅ Multi-platform SDK support

### Documentation
See [Docs/Deployment.md](Docs/Deployment.md) for detailed migration plan and [Docs/TursoResearch.md](Docs/TursoResearch.md) for Turso research.

## 📚 Documentation

- **Deployment Guide**: [Docs/Deployment.md](Docs/Deployment.md) - Cloud database migration strategies
- **Turso Research**: [Docs/TursoResearch.md](Docs/TursoResearch.md) - Comprehensive Turso documentation
- **Project Instructions**: [CLAUDE.md](CLAUDE.md) - Development guidelines and conventions

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

## 👥 Authors

- **Spyda** - Initial development

## 🙏 Acknowledgments

- Inspired by modern inventory management systems
- Built with .NET, WPF, React, and Turso
- Special thanks to the open-source community

---

**Note**: This project is currently under active development. Features and functionality may change as development progresses.
