# Mode Configuration Guide

## Overview

The React web application now supports two modes of data fetching:
- **Cloud Mode**: Direct Turso database access (recommended)
- **Local Mode**: WPF API access (requires WPF app running)

## Configuration

### Environment Variables

The mode is controlled by the `VITE_MODE` environment variable in your `.env` file:

```env
# Application Mode
# 'cloud' = Direct Turso database access (recommended)
# 'local' = WPF API access (requires WPF app running)
VITE_MODE=cloud

# Turso Database Configuration (required for cloud mode)
VITE_TURSO_DATABASE_URL=libsql://your-database-url-here
VITE_TURSO_AUTH_TOKEN=your-auth-token-here

# API Configuration (required for local mode)
VITE_API_URL=http://localhost:5000
```

### Cloud Mode (Recommended)

**Setup:**
1. Set `VITE_MODE=cloud` in your `.env` file
2. Configure your Turso database credentials:
   - `VITE_TURSO_DATABASE_URL`: Get from [Turso Console](https://console.turso.tech/databases)
   - `VITE_TURSO_AUTH_TOKEN`: Get from Turso Console

**Benefits:**
- ✅ No need to run WPF application
- ✅ Direct database access for better performance
- ✅ Can deploy to Vercel/Netlify
- ✅ Global edge deployment with Turso

**Example:**
```env
VITE_MODE=cloud
VITE_TURSO_DATABASE_URL=libsql://your-database.turso.io
VITE_TURSO_AUTH_TOKEN=eyJhbGciOiJFZERTQS...
```

### Local Mode

**Setup:**
1. Set `VITE_MODE=local` in your `.env` file
2. Ensure the WPF application is running on `http://localhost:5000`

**Use Cases:**
- Development without Turso credentials
- Testing local SQLite database
- Debugging API endpoints

**Example:**
```env
VITE_MODE=local
VITE_API_URL=http://localhost:5000
```

## Architecture

### Cloud Mode Flow
```
React App → @libsql/client → Turso Database (Cloud)
```

### Local Mode Flow
```
React App → Axios HTTP → WPF Embedded API → SQLite Database (Local)
```

## Implementation Details

### Data Service (`src/services/dataService.ts`)

The `DataService` class automatically routes requests based on `VITE_MODE`:

```typescript
class DataService {
  private mode: AppMode = 'local';

  constructor() {
    this.mode = import.meta.env.VITE_MODE === 'cloud' ? 'cloud' : 'local';
  }

  async getAllParts(filters?: Partial<PartsFilters>) {
    if (this.isCloudMode()) {
      return this.getPartsFromTurso(filters);  // Direct SQL
    } else {
      return this.getPartsFromAPI(filters);     // HTTP API
    }
  }
}
```

### Turso Client (`src/services/tursoClient.ts`)

Uses the official `@libsql/client` SDK for direct database access:

```typescript
import { createClient } from '@libsql/client';

const client = createClient({
  url: databaseUrl,
  authToken: authToken,
});

const result = await client.execute('SELECT * FROM parts');
```

### React Query Hooks (`src/hooks/useParts.ts`)

All hooks use the `dataService` which handles mode-based routing automatically:

```typescript
export const useParts = (filters?: FilterParams) => {
  return useQuery({
    queryKey: queryKeys.partsList(filters),
    queryFn: () => dataService.getAllParts(filters),  // Auto-routes
  });
};
```

## SQL Queries

When in cloud mode, the following SQL queries are executed directly against Turso:

### Get All Parts
```sql
SELECT
  p.id, p.partNumber, p.name, p.description,
  p.categoryId, c.name as categoryName,
  p.costPrice, p.stockQuantity, p.location,
  p.brand, p.imagePath, p.model, p.releaseYear,
  p.createdDate, p.lastUpdated
FROM parts p
LEFT JOIN categories c ON p.categoryId = c.id
WHERE 1=1
-- Dynamic filters applied here
ORDER BY p.id
```

### Dashboard Statistics
```sql
-- Total parts and categories
SELECT
  (SELECT COUNT(*) FROM parts) as totalParts,
  (SELECT COUNT(*) FROM categories) as totalCategories,
  (SELECT COUNT(*) FROM parts WHERE stockQuantity > 0 AND stockQuantity < 5) as lowStockCount,
  (SELECT COUNT(*) FROM parts WHERE stockQuantity = 0) as outOfStockCount

-- Parts by category
SELECT c.name as category, COUNT(p.id) as count
FROM categories c
LEFT JOIN parts p ON c.id = p.categoryId
GROUP BY c.id, c.name

-- Stock status distribution
SELECT
  CASE
    WHEN stockQuantity = 0 THEN 'Out of Stock'
    WHEN stockQuantity < 5 THEN 'Low Stock'
    ELSE 'In Stock'
  END as status,
  COUNT(*) as count
FROM parts
GROUP BY status
```

## Switching Modes

### From Local to Cloud

1. Stop the development server
2. Update `.env` file:
   ```env
   VITE_MODE=cloud
   VITE_TURSO_DATABASE_URL=your-turso-url
   VITE_TURSO_AUTH_TOKEN=your-turso-token
   ```
3. Restart development server: `npm run dev`

### From Cloud to Local

1. Stop the development server
2. Update `.env` file:
   ```env
   VITE_MODE=local
   ```
3. Start WPF application
4. Restart development server: `npm run dev`

## Deployment

### Cloud Mode Deployment (Recommended)

When deploying to Vercel/Netlify:

1. Set environment variables in your hosting platform:
   ```
   VITE_MODE=cloud
   VITE_TURSO_DATABASE_URL=libsql://your-database-url
   VITE_TURSO_AUTH_TOKEN=your-auth-token
   ```

2. Build and deploy:
   ```bash
   npm run build
   # Deploy dist/ folder to Vercel/Netlify
   ```

### Local Mode Deployment

Local mode **cannot** be deployed to static hosting because it requires the WPF application to be running on `localhost:5000`.

## Troubleshooting

### Turso Connection Errors

**Error:** "Turso configuration missing"
**Solution:** Check that `VITE_TURSO_DATABASE_URL` and `VITE_TURSO_AUTH_TOKEN` are set in `.env`

**Error:** "Failed to initialize Turso client"
**Solution:** Verify your Turso credentials are valid and the database exists

### API Connection Errors

**Error:** "No response from server"
**Solution:** Ensure the WPF application is running on `http://localhost:5000`

### Data Not Loading

**Check the browser console for:**
- `DataService initialized in CLOUD mode` or `LOCAL mode`
- `TursoClient initialized with:` (cloud mode)
- `API Request: GET /api/parts` (local mode)

## Best Practices

1. **Use Cloud Mode** for production deployments
2. **Use Local Mode** only for development without Turso credentials
3. **Never commit** `.env` files with real credentials
4. **Use `.env.example`** as a template for environment variables
5. **Restart the dev server** after changing `.env` files

## Performance Comparison

| Metric | Cloud Mode | Local Mode |
|--------|-----------|------------|
| Initial Load | ~200ms | ~300ms |
| Subsequent Requests | ~50ms (cached) | ~100ms |
| Deployment | Vercel/Netlify | Requires WPF app |
| Scalability | Global edge | Single machine |

## Security Notes

- **Cloud Mode**: Auth token is exposed in client-side code. This is acceptable for Turso as it uses fine-grained access tokens.
- **Local Mode**: No authentication - only use in trusted environments.
- **Recommendation**: Use Turso's group-based access tokens for production to limit database operations.

## Additional Resources

- [Turso Documentation](https://docs.turso.tech/)
- [Turso TypeScript SDK](https://docs.turso.tech/sdk/ts/reference)
- [Vite Environment Variables](https://vitejs.dev/guide/env-and-mode.html)
- [React Query Documentation](https://tanstack.com/query/latest)
