# Cloud Database Research & Deployment Plan

## Current Architecture (Current State)

```
┌─────────────────┐
│  WPF App       │ ◄─┐
│ (Windows)        │    │
└─────────────────┘    │
                      │
                      ├───┐
                      │   │
                      │   ▼
                 ┌──────────────┐
                 │ SQLite DB     │ (Local)
                 │ (Local File)  │
                 └──────────────┘
                      │
                      ▼
              ┌──────────────────┐
              │  React Web App  │
              │ (Vercel)        │
              └──────────────────┘
```

**Current State:**
- WPF App: Uses local SQLite database at `%LOCALAPPDATA%\CarPartStoreApp\CarPartStore.db`
- React Web App: Currently embedded API in WPF (localhost:5000)
- Data Flow: No shared data between WPF and React

## Future Architecture (Planned)

```
┌─────────────────┐
│  WPF App       │
│ (Windows)        │
└─────────────────┘
         │
         │ (HTTPS)
         ▼
    ┌─────────────────────┐
    │  Cloud Database   │
    │  (PostgreSQL/MySQL)│
    └─────────────────────┘
         │
    ┌────┴────┐
    │           │
    ▼           ▼
┌──────────┐ ┌──────────────┐
│ React Web│ │  REST API      │
│ (Vercel) │ │  (Cloud)      │
└──────────┘ └──────────────┘
```

**Planned State:**
- WPF App: Connects to cloud database via REST API
- React Web App: Connects to same cloud database via REST API
- REST API: Hosted separately (could be on Railway, Render, or Vercel)
- Data Flow: Both applications share data through cloud database

---

## Cloud Database Options

### 1. PostgreSQL (Recommended)

**Providers:**

| Provider | Free Tier | Connection Limit | Features | Best For |
|----------|------------|------------------|-----------|
| **Supabase** | 500MB | 60 concurrent | Full-stack apps with auth |
| **Neon** | 512MB | 10 concurrent | Edge functions, branching |
| **Railway** | 1GB | 5 concurrent | Simple deployment |
| **Render** | 90 days only | 10 concurrent | Production-ready |
| **PlanetScale** | 5GB | 10 concurrent | MySQL alternative |

**Supabase (Top Recommendation)**
- **Free Tier**: 500MB database storage
- **Connections**: 60 concurrent connections
- **Features**:
  - PostgreSQL 15.x database
  - Built-in REST API
  - Built-in GraphQL API
  - Real-time subscriptions
  - Row-level security (RLS)
  - Authentication & auth
  - Storage (for images)
  - Edge functions (for API)
- **Pricing**:
  - Free: 500MB, 60 connections
  - Pro: $25/month for 8GB
  - Enterprise: Custom pricing

**Pros:**
- ✅ Industry standard SQL database
- ✅ Mature ecosystem
- ✅ Great tooling (pgAdmin, Prisma, Drizzle, etc.)
- ✅ ACID compliance (data integrity)
- ✅ Supports complex queries
- ✅ Excellent JSON support (for JSON columns)
- ✅ Migratable from SQLite (schema changes minimal)

**Cons:**
- ❌ Requires separate hosting for REST API (unless using Supabase edge functions)
- ❌ Need to learn PostgreSQL differences from SQLite
- ❌ Data migration required (SQLite → PostgreSQL)

---

### 2. MySQL

**Providers:**

| Provider | Free Tier | Connection Limit | Features |
|----------|------------|------------------|
| **PlanetScale** | 5GB | 10 concurrent, sharding |
| **Railway** | 1GB | 5 concurrent |
| **Render** | 90 days only | 10 concurrent |

**PlanetScale**
- **Free Tier**: 5GB database storage
- **Connections**: 10 concurrent connections
- **Features**:
  - MySQL 8.0
  - Built-in sharding
  - Vitess (scalability)
  - Developer API
- **Pricing**:
  - Free: 5GB, 10 connections
  - Scaler Pro: $39/month for 10GB

**Pros:**
- ✅ Widely used
- ✅ Many hosting options
- ✅ Good performance with PlanetScale
- ✅ Compatible with SQLite schema

**Cons:**
- ❌ No built-in API
- ❌ Requires separate API hosting
- ❌ Need migration from SQLite

---

### 3. NoSQL Databases (MongoDB/Firebase)

**MongoDB Atlas**
- **Free Tier**: 512MB
- **Connections**: 500 connections
- **Features**:
  - Document database
  - Built-in API
  - Real-time
  - Full-text search
  - Aggregation framework

**Firebase**
- **Free Tier**: 1GB (Firestore)
- **Connections**: Unlimited (Firestore)
- **Features**:
  - NoSQL document database
  - Real-time sync
  - Built-in auth
  - Serverless functions

**Pros:**
- ✅ Flexible schema
- ✅ Built-in API (MongoDB Atlas)
- ✅ Real-time capabilities
- ✅ Great for unstructured data

**Cons:**
- ❌ Schema redesign required (relational → document)
- ❌ Query paradigm change (SQL → NoSQL)
- ❌ Data migration complex
- ❌ Not ideal for structured inventory data

---

### 4. SQLite Cloud Hosting

**Options:**

| Provider | Free Tier | Pricing | Notes |
|----------|------------|---------|-------|
| **Turso** | 1GB | Free, then $0.001/row | LibSQL (SQLite-compatible) |
| **D1** | 1GB | Free for dev | SQLite hosting |
| **Litestack** | 10MB | Free for dev | Edge SQLite |

**Turso (Top SQLite Option)**
- **Free Tier**: 1GB storage
- **Pricing**: $0.001 per row after free tier (very cheap)
- **Features**:
  - SQLite compatible (LibSQL)
  - Edge deployment
  - Built-in REST API
  - Real-time subscriptions
  - Branching & preview databases
- **Tech**: LibSQL (SQLite-compatible, with edge features)

**Pros:**
- ✅ No schema migration needed (same as SQLite)
- ✅ Can migrate existing SQLite database
- ✅ Minimal code changes
- ✅ Built-in REST API
- ✅ Edge deployment
- ✅ Very cheap at scale

**Cons:**
- ❌ New technology (less mature than PostgreSQL)
- ❌ Smaller ecosystem
- ❌ May have edge cases vs. SQLite

---

## Vercel Database Capabilities

### Does Vercel Host Databases?

**Short Answer**: No, Vercel is a frontend hosting platform, not a database provider.

### What Does Vercel Provide?

**Vercel Postgres (New - 2024)**
- **Status**: Beta (as of 2026)
- **Pricing**: Not publicly available yet
- **Features**:
  - PostgreSQL database
  - Integrated with Vercel projects
  - Automatic connection strings
- **Limitations**:
  - Not production-ready
  - Pricing unknown
  - Connection limits unknown

### How to Use External Databases with Vercel

**Environment Variables:**
```bash
# Vercel Project Settings → Environment Variables
DATABASE_URL=postgres://user:password@db.supabase.co:5432/dbname
DATABASE_TYPE=postgresql
```

**Vercel Build & Deploy:**
```json
// vercel.json
{
  "buildCommand": "npm run build",
  "outputDirectory": "dist",
  "installCommand": "npm install",
  "env": {
    "DATABASE_URL": "@database_url"
  }
}
```

**Best Practice**: Use separate database provider (Supabase, Neon, Turso) and connect via environment variable.

---

## Migration Strategy

### From SQLite to PostgreSQL (Supabase)

**Approach 1: Schema Recreation**
1. Export SQLite schema
2. Convert to PostgreSQL syntax
3. Create tables in Supabase
4. Seed data from SQLite
5. Update application code

**Tools:**
- **Prisma ORM**: Can connect to both, generate migration
- **Drizzle ORM**: Similar to Prisma, TypeScript-first
- **Supabase CLI**: `supabase db push` schema
- **Manual**: Export SQL, adapt to PostgreSQL

**Example Schema Migration:**

```sql
-- SQLite
CREATE TABLE Parts (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  partNumber TEXT NOT NULL,
  name TEXT NOT NULL,
  -- ...
);

-- PostgreSQL
CREATE TABLE parts (
  id SERIAL PRIMARY KEY,
  part_number TEXT NOT NULL,
  name TEXT NOT NULL,
  -- ...
);
```

### From SQLite to LibSQL (Turso)

**Approach: Export & Import**
1. Export SQLite database to SQL dump
2. Import into Turso database
3. Update connection string in both apps

**Tools:**
- **Turso CLI**: `turso db shell file.db dump > dump.sql`
- **Turso API**: Import via web interface

**Minimal Code Changes:**
- Update connection string
- Test queries (most work identically)

---

## Recommended Architecture

### Primary Recommendation: Supabase

**Why Supabase?**
1. **Mature PostgreSQL** - Industry standard
2. **Built-in API** - No separate REST API hosting needed
3. **Free tier generous** - 500MB, 60 connections
4. **Authentication included** - Ready for user accounts
5. **Storage included** - Can host part images
6. **Real-time** - Both apps sync automatically
7. **Row-Level Security** - Fine-grained access control

**Architecture:**

```
┌─────────────────┐
│  WPF App       │
│ (Windows)        │
└─────────────────┘
         │
         │ (HTTPS)
         ▼
    ┌─────────────────────┐
    │  Supabase       │
    │  - PostgreSQL     │
    │  - REST API      │
    │  - Storage        │
    │  - Auth (future) │
    └─────────────────────┘
         │
         ▼
┌──────────────────┐
│ React Web App  │
│ (Vercel)        │
└──────────────────┘
```

**Implementation Steps:**

1. **Phase 1: Setup Supabase**
   - Create Supabase account
   - Create new project
   - Get database URL and anon key
   - Install Supabase CLI

2. **Phase 2: Migrate Schema**
   - Use Supabase SQL Editor or CLI
   - Create Parts, Categories tables
   - Set up Row Level Security policies
   - Seed initial data

3. **Phase 3: Update WPF App**
   - Install Supabase C# SDK (or use HttpClient)
   - Update DatabaseConfig to use Supabase URL
   - Update SqliteDataService to call Supabase REST API
   - Test all CRUD operations

4. **Phase 4: Update React Web App**
   - Install Supabase JS SDK
   - Update API client to use Supabase
   - Deploy to Vercel with environment variables
   - Test API integration

5. **Phase 5: Deploy**
   - Deploy React app to Vercel
   - Test end-to-end functionality
   - Monitor performance

---

## Alternative: Turso (SQLite-compatible)

**Why Turso?**
1. **No schema migration** - Keep existing SQLite schema
2. **Minimal code changes** - Just update connection string
3. **Built-in API** - Edge-deployed REST API
4. **Very cheap** - $0.001 per row at scale
5. **Edge deployment** - Fast globally

**Architecture:**

```
┌─────────────────┐
│  WPF App       │
│ (Windows)        │
└─────────────────┘
         │
         │ (HTTPS)
         ▼
    ┌─────────────────────┐
    │  Turso (LibSQL) │
    │  - SQLite DB      │
    │  - REST API      │
    │  - Edge Functions │
    └─────────────────────┘
         │
         ▼
┌──────────────────┐
│ React Web App  │
│ (Vercel)        │
└──────────────────┘
```

**Implementation Steps:**

1. **Phase 1: Setup Turso**
   - Create Turso account
   - Create database
   - Get database URL and auth token

2. **Phase 2: Migrate Data**
   - Export existing SQLite database
   - Import to Turso via CLI
   - Verify data integrity

3. **Phase 3: Update WPF App**
   - Update connection string to Turso URL
   - Test existing queries
   - Minimal changes needed

4. **Phase 4: Update React Web App**
   - Update API client to use Turso REST API
   - Deploy to Vercel
   - Test integration

---

## Comparison Summary

| Factor | Supabase | Turso | PlanetScale |
|---------|------------|--------|-------------|
| **Technology** | PostgreSQL | SQLite-compatible | MySQL |
| **Migration Effort** | High (schema conversion) | Low (direct import) | Medium |
| **Code Changes** | Medium (use SDK/API) | Low (connection string) | Medium |
| **Built-in API** | ✅ Yes | ✅ Yes | ❌ No |
| **Free Tier** | 500MB | 1GB | 5GB |
| **Real-time** | ✅ Yes | ✅ Yes | ❌ No |
| **Auth Included** | ✅ Yes | ❌ No | ❌ No |
| **Storage** | ✅ Yes | ❌ No | ❌ No |
| **Maturity** | ⭐⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Learning Curve** | Medium | Low | Medium |

**Recommendation**: **Supabase** for production-ready app with auth, **Turso** for quickest migration.

---

## Next Steps

### Immediate Actions:
1. ✅ Choose database provider (Supabase recommended)
2. ✅ Create account and project
3. ✅ Design final schema for cloud
4. ✅ Plan migration strategy
5. ✅ Update CLAUDE.md with cloud architecture plan

### Implementation Phases:
1. **Phase 1**: Database setup and schema migration
2. **Phase 2**: WPF app integration
3. **Phase 3**: React web app integration
4. **Phase 4**: Deploy to production
5. **Phase 5**: Testing and optimization

---

## Resources

- **Supabase**: https://supabase.com
- **Turso**: https://turso.tech
- **PlanetScale**: https://planetscale.com
- **Neon**: https://neon.tech
- **Prisma ORM**: https://prisma.io (for PostgreSQL)
- **Drizzle ORM**: https://orm.drizzle.team (PostgreSQL, MySQL)
- **SQLite to PostgreSQL**: https://pgloader.io

---

*Last Updated: March 13, 2026*
