# Turso React Integration Plan
## Direct Database Access for React Frontend

**Document Version:** 1.0
**Date:** 2026-03-19
**Status:** Planning

---

## Overview

This plan outlines how to integrate Turso's official TypeScript SDK (`@libsql/client`) into the React frontend, enabling direct database access in **cloud mode** while maintaining **local mode** with the WPF API.

---

## Architecture

### Current Architecture
```
React Frontend
    ↓ HTTP
WPF API (localhost:5000)
    ↓
Turso/SQLite Database
```

### New Architecture (Mode-Based)

**Cloud Mode:**
```
React Frontend
    ↓ @libsql/client
Turso Database (Direct)
```

**Local Mode:**
```
React Frontend
    ↓ HTTP
WPF API (localhost:5000)
    ↓
SQLite Database
```

---

## Phase 1: Install Dependencies

### Package Installation

```bash
cd CarPartStoreWebApp
npm install @libsql/client
```

**Package Details:**
- **@libsql/client**: Official Turso SDK for TypeScript/JavaScript
- **Version**: Latest (check npm for current version)
- **Size**: ~50KB
- **Support**: Node.js, Vite, Edge Functions

---

## Phase 2: Environment Configuration

### Environment Variables

**For Cloud Mode (`.env`):**
```env
MODE=cloud
TURSO_DATABASE_URL=libsql://your-database-name.turso.io
TURSO_AUTH_TOKEN=your-auth-token-here
```

**For Local Mode (`.env`):**
```env
MODE=local
API_URL=http://localhost:5000
```

**Development (`.env.development`):**
```env
MODE=cloud  # or local
TURSO_DATABASE_URL=libsql://dev-database.turso.io
TURSO_AUTH_TOKEN=dev-token
```

**Production (`.env.production`):**
```env
MODE=cloud
TURSO_DATABASE_URL=libsql://prod-database.turso.io
TURSO_AUTH_TOKEN=prod-token
```

---

## Phase 3: Create Turso Client Service

### File: `src/services/tursoClient.ts`

```typescript
import { createClient } from '@libsql/client';

interface TursoConfig {
  url: string;
  authToken: string;
  concurrency?: number; // Default: 20
}

/**
 * Turso Database Client
 * Direct connection to Turso cloud database using @libsql/client
 */
class TursoClient {
  private client: ReturnType<typeof createClient>;
  private initialized = false;

  constructor(config: TursoConfig) {
    this.client = createClient({
      url: config.url,
      authToken: config.authToken,
      concurrency: config.concurrency || 20, // Default 20 concurrent requests
    });
    this.initialized = true;
  }

  /**
   * Execute a SQL query
   * @param sql - SQL statement with placeholders
   * @param args - Arguments for placeholders
   * @returns Query result set
   */
  async execute(sql: string, args: any[] = []) {
    try {
      const result = await this.client.execute(sql, args);
      return result;
    } catch (error) {
      console.error('Turso query error:', error);
      throw error;
    }
  }

  /**
   * Execute multiple queries in a batch (implicit transaction)
   * @param queries - Array of SQL queries with arguments
   * @param mode - Transaction mode ('read' | 'write' | 'deferred')
   * @returns Batch result
   */
  async batch(
    queries: Array<{ sql: string; args: any[] }>,
    mode: 'read' | 'write' | 'deferred' = 'read'
  ) {
    try {
      const result = await this.client.batch(queries, mode);
      return result;
    } catch (error) {
      console.error('Turso batch error:', error);
      throw error;
    }
  }

  /**
   * Execute queries in an interactive transaction
   * @param callback - Transaction callback function
   * @returns Transaction result
   */
  async transaction<T>(
    callback: (txn: ReturnType<typeof this.client.transaction>) => Promise<T>
  ) {
    const txn = this.client.transaction('write');
    try {
      const result = await callback(txn);
      await txn.commit();
      return result;
    } catch (error) {
      await txn.rollback();
      throw error;
    } finally {
      await txn.close();
    }
  }

  isInitialized() {
    return this.initialized;
  }
}

/**
 * Create and export Turso client instance
 */
export const createTursoClient = (config: TursoConfig) => {
  return new TursoClient(config);
};

/**
 * Singleton instance (created on first import)
 */
let tursoClientInstance: TursoClient | null = null;

export const getTursoClient = () => {
  if (!tursoClientInstance) {
    const url = import.meta.env.VITE_TURSO_DATABASE_URL;
    const authToken = import.meta.env.VITE_TURSO_AUTH_TOKEN;

    if (!url || !authToken) {
      throw new Error('TURSO_DATABASE_URL and TURSO_AUTH_TOKEN must be set in .env');
    }

    tursoClientInstance = new TursoClient({ url, authToken });
  }

  return tursoClientInstance;
};

export default TursoClient;
```

---

## Phase 4: Create Data Access Layer

### File: `src/services/dataService.ts`

```typescript
import { getTursoClient } from './tursoClient';
import type { CarPartDto, CategoryDto, DashboardStats } from '@/types';

/**
 * Data Service
 * Abstracts database access (Turso or API)
 * Returns data in the same format regardless of underlying source
 */
class DataService {
  private mode: 'cloud' | 'local';

  constructor() {
    this.mode = (import.meta.env.VITE_MODE as string) || 'local';
    console.log(`Data Service initialized in ${this.mode} mode`);
  }

  getMode() {
    return this.mode;
  }

  isCloudMode() {
    return this.mode === 'cloud';
  }

  isLocalMode() {
    return this.mode === 'local';
  }

  // ==================== PARTS ====================

  /**
   * Get all parts
   */
  async getAllParts(params?: {
    search?: string;
    categoryId?: number;
  }): Promise<CarPartDto[]> {
    if (this.isCloudMode()) {
      return this.getPartsFromTurso(params);
    } else {
      return this.getPartsFromAPI(params);
    }
  }

  /**
   * Get parts from Turso (cloud mode)
   */
  private async getPartsFromTurso(params?: {
    search?: string;
    categoryId?: number;
  }): Promise<CarPartDto[]> {
    const client = getTursoClient();

    let sql = `
      SELECT
        p.id,
        p.partNumber,
        p.name,
        p.description,
        p.categoryId,
        c.name as categoryName,
        p.costPrice,
        p.stockQuantity,
        p.location,
        p.brand,
        p.imagePath,
        p.model,
        p.releaseYear,
        p.createdDate,
        p.lastUpdated
      FROM parts p
      LEFT JOIN categories c ON p.categoryId = c.id
    `;

    const conditions: string[] = [];
    const args: any[] = [];

    if (params?.search) {
      conditions.push('(p.name LIKE ? OR p.partNumber LIKE ? OR p.description LIKE ? OR p.brand LIKE ?)');
      const searchTerm = `%${params.search}%`;
      args.push(searchTerm, searchTerm, searchTerm, searchTerm);
    }

    if (params?.categoryId) {
      conditions.push('p.categoryId = ?');
      args.push(params.categoryId);
    }

    if (conditions.length > 0) {
      sql += ' WHERE ' + conditions.join(' AND ');
    }

    sql += ' ORDER BY p.createdDate DESC';

    const result = await client.execute(sql, args);
    return this.mapRowsToCarParts(result.rows);
  }

  /**
   * Get part by ID
   */
  async getPartById(id: number): Promise<CarPartDto | null> {
    if (this.isCloudMode()) {
      const client = getTursoClient();
      const sql = `
        SELECT
          p.id,
          p.partNumber,
          p.name,
          p.description,
          p.categoryId,
          c.name as categoryName,
          p.costPrice,
          p.stockQuantity,
          p.location,
          p.brand,
          p.imagePath,
          p.model,
          p.releaseYear,
          p.createdDate,
          p.lastUpdated
        FROM parts p
        LEFT JOIN categories c ON p.categoryId = c.id
        WHERE p.id = ?
      `;
      const result = await client.execute(sql, [id]);
      const parts = this.mapRowsToCarParts(result.rows);
      return parts[0] || null;
    } else {
      // Use existing API
      const { partsApi } = await import('./api');
      return await partsApi.getById(id);
    }
  }

  /**
   * Get parts by category
   */
  async getPartsByCategory(categoryId: number): Promise<CarPartDto[]> {
    return this.getAllParts({ categoryId });
  }

  /**
   * Search parts
   */
  async searchParts(term: string): Promise<CarPartDto[]> {
    return this.getAllParts({ search: term });
  }

  /**
   * Get dashboard statistics
   */
  async getDashboardStats(): Promise<DashboardStats> {
    if (this.isCloudMode()) {
      const client = getTursoClient();

      // Get total parts
      const totalPartsResult = await client.execute('SELECT COUNT(*) as count FROM parts');
      const totalParts = totalPartsResult.rows[0].count as number;

      // Get total categories
      const totalCategoriesResult = await client.execute('SELECT COUNT(*) as count FROM categories');
      const totalCategories = totalCategoriesResult.rows[0].count as number;

      // Get low stock count (< 5)
      const lowStockResult = await client.execute('SELECT COUNT(*) as count FROM parts WHERE stockQuantity < 5');
      const lowStockCount = lowStockResult.rows[0].count as number;

      // Get out of stock count
      const outOfStockResult = await client.execute('SELECT COUNT(*) as count FROM parts WHERE stockQuantity = 0');
      const outOfStockCount = outOfStockResult.rows[0].count as number;

      // Get parts by category
      const byCategoryResult = await client.execute(`
        SELECT c.name as category, COUNT(p.id) as count
        FROM categories c
        LEFT JOIN parts p ON c.id = p.categoryId
        GROUP BY c.id, c.name
        ORDER BY count DESC
      `);
      const partsByCategory = byCategoryResult.rows.map(row => ({
        category: row.category as string,
        count: row.count as number,
      }));

      // Get stock status distribution
      const stockStatusResult = await client.execute(`
        SELECT
          CASE
            WHEN stockQuantity = 0 THEN 'Out of Stock'
            WHEN stockQuantity < 5 THEN 'Low Stock'
            ELSE 'In Stock'
          END as status,
          COUNT(*) as count
        FROM parts
        GROUP BY status
      `);
      const stockStatusDistribution = stockStatusResult.rows.map(row => ({
        status: row.status as string,
        count: row.count as number,
      }));

      return {
        totalParts,
        totalCategories,
        lowStockCount,
        outOfStockCount,
        partsByCategory,
        stockStatusDistribution,
        monthlyAdditions: [], // Could be implemented if needed
      };
    } else {
      // Use existing API
      const { partsApi } = await import('./api');
      return await partsApi.getDashboardStats();
    }
  }

  /**
   * Get low stock parts
   */
  async getLowStockParts(): Promise<CarPartDto[]> {
    if (this.isCloudMode()) {
      const client = getTursoClient();
      const sql = `
        SELECT
          p.id,
          p.partNumber,
          p.name,
          p.description,
          p.categoryId,
          c.name as categoryName,
          p.costPrice,
          p.stockQuantity,
          p.location,
          p.brand,
          p.imagePath,
          p.model,
          p.releaseYear,
          p.createdDate,
          p.lastUpdated
        FROM parts p
        LEFT JOIN categories c ON p.categoryId = c.id
        WHERE p.stockQuantity < 5
        ORDER BY p.stockQuantity ASC
      `;
      const result = await client.execute(sql);
      return this.mapRowsToCarParts(result.rows);
    } else {
      // Use existing API
      const { partsApi } = await import('./api');
      return await partsApi.getLowStock();
    }
  }

  /**
   * Get recent parts
   */
  async getRecentParts(limit = 10): Promise<CarPartDto[]> {
    if (this.isCloudMode()) {
      const client = getTursoClient();
      const sql = `
        SELECT
          p.id,
          p.partNumber,
          p.name,
          p.description,
          p.categoryId,
          c.name as categoryName,
          p.costPrice,
          p.stockQuantity,
          p.location,
          p.brand,
          p.imagePath,
          p.model,
          p.releaseYear,
          p.createdDate,
          p.lastUpdated
        FROM parts p
        LEFT JOIN categories c ON p.categoryId = c.id
        ORDER BY p.createdDate DESC
        LIMIT ?
      `;
      const result = await client.execute(sql, [limit]);
      return this.mapRowsToCarParts(result.rows);
    } else {
      // Use existing API
      const { partsApi } = await import('./api');
      return await partsApi.getRecent();
    }
  }

  // ==================== CATEGORIES ====================

  /**
   * Get all categories
   */
  async getAllCategories(): Promise<CategoryDto[]> {
    if (this.isCloudMode()) {
      const client = getTursoClient();
      const sql = 'SELECT * FROM categories ORDER BY displayOrder, name';
      const result = await client.execute(sql);
      return this.mapRowsToCategories(result.rows);
    } else {
      // Use existing API
      const { categoriesApi } = await import('./api');
      return await categoriesApi.getAll();
    }
  }

  /**
   * Get category by ID
   */
  async getCategoryById(id: number): Promise<CategoryDto | null> {
    if (this.isCloudMode()) {
      const client = getTursoClient();
      const sql = 'SELECT * FROM categories WHERE id = ?';
      const result = await client.execute(sql, [id]);
      const categories = this.mapRowsToCategories(result.rows);
      return categories[0] || null;
    } else {
      // Use existing API
      const { categoriesApi } = await import('./api');
      return await categoriesApi.getById(id);
    }
  }

  // ==================== HELPER METHODS ====================

  /**
   * Map Turso result rows to CarPartDto array
   */
  private mapRowsToCarParts(rows: any[]): CarPartDto[] {
    return rows.map(row => ({
      id: row.id as number,
      partNumber: row.partNumber as string | undefined,
      name: row.name as string,
      description: row.description as string | undefined,
      categoryId: row.categoryId as number,
      categoryName: row.categoryName as string | undefined,
      costPrice: row.costPrice as number,
      stockQuantity: row.stockQuantity as number,
      location: row.location as string | undefined,
      brand: row.brand as string | undefined,
      imagePath: row.imagePath as string | undefined,
      model: row.model as string | undefined,
      releaseYear: row.releaseYear as number | null | undefined,
      createdDate: row.createdDate as string,
      lastUpdated: row.lastUpdated as string | undefined,
    }));
  }

  /**
   * Map Turso result rows to CategoryDto array
   */
  private mapRowsToCategories(rows: any[]): CategoryDto[] {
    return rows.map(row => ({
      id: row.id as number,
      name: row.name as string,
      description: row.description as string | undefined,
      parentCategoryId: row.parentCategoryId as number | undefined,
      displayOrder: row.displayOrder as number,
      createdDate: row.createdDate as string,
    }));
  }

  /**
   * Get parts from API (local mode fallback)
   */
  private async getPartsFromAPI(params?: {
    search?: string;
    categoryId?: number;
  }): Promise<CarPartDto[]> {
    const { partsApi } = await import('./api');
    return await partsApi.getAll(params);
  }
}

/**
 * Export singleton instance
 */
export const dataService = new DataService();
export default DataService;
```

---

## Phase 5: Update React Query Hooks

### File: `src/hooks/useParts.ts`

```typescript
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { dataService } from '@/services/dataService';
import type { CarPartDto, CategoryDto, DashboardStats } from '@/types';

// ==================== PARTS ====================

/**
 * Get all parts with optional filters
 */
export const useParts = (params?: {
  search?: string;
  categoryId?: number;
}) => {
  return useQuery({
    queryKey: ['parts', params],
    queryFn: () => dataService.getAllParts(params),
    staleTime: 5 * 1000, // 5 seconds
  });
};

/**
 * Get part by ID
 */
export const usePart = (id: number) => {
  return useQuery({
    queryKey: ['part', id],
    queryFn: () => dataService.getPartById(id),
    enabled: !!id,
    staleTime: 10 * 1000, // 10 seconds
  });
};

/**
 * Get parts by category
 */
export const usePartsByCategory = (categoryId: number) => {
  return useQuery({
    queryKey: ['parts', 'category', categoryId],
    queryFn: () => dataService.getPartsByCategory(categoryId),
    enabled: !!categoryId,
    staleTime: 5 * 1000,
  });
};

/**
 * Search parts
 */
export const useSearchParts = (term: string) => {
  return useQuery({
    queryKey: ['parts', 'search', term],
    queryFn: () => dataService.searchParts(term),
    enabled: term.length > 2,
    staleTime: 5 * 1000,
  });
};

// ==================== DASHBOARD ====================

/**
 * Get dashboard statistics
 */
export const useDashboardStats = () => {
  return useQuery({
    queryKey: ['dashboard', 'stats'],
    queryFn: () => dataService.getDashboardStats(),
    staleTime: 30 * 1000, // 30 seconds
  });
};

/**
 * Get low stock parts
 */
export const useLowStockParts = () => {
  return useQuery({
    queryKey: ['parts', 'low-stock'],
    queryFn: () => dataService.getLowStockParts(),
    staleTime: 15 * 1000, // 15 seconds
  });
};

/**
 * Get recent parts
 */
export const useRecentParts = (limit = 10) => {
  return useQuery({
    queryKey: ['parts', 'recent', limit],
    queryFn: () => dataService.getRecentParts(limit),
    staleTime: 10 * 1000, // 10 seconds
  });
};

// ==================== CATEGORIES ====================

/**
 * Get all categories
 */
export const useCategories = () => {
  return useQuery({
    queryKey: ['categories'],
    queryFn: () => dataService.getAllCategories(),
    staleTime: 60 * 1000, // 60 seconds
  });
};

/**
 * Get category by ID
 */
export const useCategory = (id: number) => {
  return useQuery({
    queryKey: ['category', id],
    queryFn: () => dataService.getCategoryById(id),
    enabled: !!id,
    staleTime: 60 * 1000,
  });
};

// ==================== UTILITY ====================

/**
 * Get data service mode
 */
export const useDataServiceMode = () => {
  return {
    mode: dataService.getMode(),
    isCloud: dataService.isCloudMode(),
    isLocal: dataService.isLocalMode(),
  };
};
```

---

## Phase 6: Update Vite Configuration

### File: `vite.config.js`

```javascript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  define: {
    // Expose MODE to client code
    'import.meta.env.VITE_MODE': JSON.stringify(process.env.MODE || 'local'),
  },
  // ... rest of config
});
```

---

## Phase 7: Update TypeScript Types

### File: `src/types/index.ts`

Add Turso-specific types if needed:

```typescript
/**
 * Turso result row type
 * Based on @libsql/client ResultSet
 */
export interface TursoRow {
  [key: string]: string | number | null | undefined;
}

/**
 * Turso query result
 */
export interface TursoResult {
  rows: TursoRow[];
  columns: string[];
  rowsAffected: number;
  lastInsertRowid?: bigint;
}
```

---

## Phase 8: Environment-Specific Configurations

### Development (`.env.development`)

```env
# Mode: cloud or local
MODE=cloud

# Turso Configuration (for cloud mode)
TURSO_DATABASE_URL=libsql://your-dev-database.turso.io
TURSO_AUTH_TOKEN=your-dev-auth-token

# OR WPF API Configuration (for local mode)
# API_URL=http://localhost:5000
```

### Production (`.env.production`)

```env
# Mode: cloud (recommended for production)
MODE=cloud

# Turso Configuration
TURSO_DATABASE_URL=libsql://your-prod-database.turso.io
TURSO_AUTH_TOKEN=your-prod-auth-token
```

---

## Phase 9: Implementation Steps

### Step 1: Install Dependencies
```bash
npm install @libsql/client
```

### Step 2: Create Turso Client
- Create `src/services/tursoClient.ts`
- Implement client initialization and query methods
- Add error handling

### Step 3: Create Data Service
- Create `src/services/dataService.ts`
- Implement mode-based routing (cloud vs local)
- Add SQL queries for Turso
- Keep API integration for local mode

### Step 4: Update Hooks
- Modify `src/hooks/useParts.ts`
- Replace API calls with dataService calls
- Add mode detection hook

### Step 5: Update Configuration
- Add MODE variable to `.env` files
- Update `vite.config.js` if needed
- Update TypeScript types

### Step 6: Testing
- Test in cloud mode (with Turso credentials)
- Test in local mode (with WPF API running)
- Verify data consistency between modes
- Test error handling

### Step 7: Documentation
- Update README with mode configuration
- Document environment variables
- Add deployment instructions

---

## SQL Queries Reference

### Parts Table Queries

```sql
-- Get all parts with category
SELECT
  p.id, p.partNumber, p.name, p.description,
  p.categoryId, c.name as categoryName,
  p.costPrice, p.stockQuantity, p.location,
  p.brand, p.imagePath, p.model, p.releaseYear,
  p.createdDate, p.lastUpdated
FROM parts p
LEFT JOIN categories c ON p.categoryId = c.id
ORDER BY p.createdDate DESC;

-- Get part by ID
SELECT * FROM parts WHERE id = ?;

-- Search parts
SELECT * FROM parts
WHERE name LIKE ? OR partNumber LIKE ? OR description LIKE ? OR brand LIKE ?;

-- Get low stock parts
SELECT * FROM parts WHERE stockQuantity < 5 ORDER BY stockQuantity ASC;

-- Get recent parts
SELECT * FROM parts ORDER BY createdDate DESC LIMIT 10;

-- Dashboard stats
SELECT COUNT(*) FROM parts;
SELECT COUNT(*) FROM categories;
SELECT COUNT(*) FROM parts WHERE stockQuantity < 5;
SELECT COUNT(*) FROM parts WHERE stockQuantity = 0;

-- Parts by category
SELECT c.name, COUNT(p.id) as count
FROM categories c
LEFT JOIN parts p ON c.id = p.categoryId
GROUP BY c.id, c.name;
```

### Categories Table Queries

```sql
-- Get all categories
SELECT * FROM categories ORDER BY displayOrder, name;

-- Get category by ID
SELECT * FROM categories WHERE id = ?;
```

---

## Error Handling

### Turso Client Errors

```typescript
// Handle connection errors
try {
  await client.execute(sql, args);
} catch (error) {
  if (error.message.includes('authentication')) {
    console.error('Turso authentication failed. Check your auth token.');
  } else if (error.message.includes('database')) {
    console.error('Turso database not found. Check your database URL.');
  } else {
    console.error('Turso query error:', error);
  }
  throw error;
}
```

### Mode Detection

```typescript
// Display mode in UI
const { isCloud, isLocal } = useDataServiceMode();

return (
  <div>
    {isCloud && <Badge variant="secondary">Cloud Mode</Badge>}
    {isLocal && <Badge variant="outline">Local Mode</Badge>}
  </div>
);
```

---

## Performance Considerations

### Turso SDK Benefits

1. **Direct Connection**: No HTTP overhead
2. **Connection Pooling**: Automatic (20 concurrent requests by default)
3. **Type Safety**: Full TypeScript support
4. **Batch Queries**: Execute multiple queries in one request
5. **Prepared Statements**: Automatic with placeholders

### Query Optimization

1. **Use Indexes**: Ensure database indexes on frequently queried columns
2. **Limit Results**: Use `LIMIT` for large result sets
3. **Select Specific Columns**: Avoid `SELECT *`
4. **Use Joins Efficiently**: Optimize JOIN queries
5. **Cache Results**: React Query handles this automatically

### Concurrency

```typescript
// Increase concurrent requests if needed
const client = createClient({
  url: databaseUrl,
  authToken: authToken,
  concurrency: 50, // Increase from default 20
});
```

---

## Security Considerations

### Environment Variables

1. **Never Commit `.env` Files**: Add to `.gitignore`
2. **Use Different Credentials**: Dev vs Production
3. **Rotate Tokens Regularly**: Security best practice
4. **Limit Token Permissions**: Turso tokens can be scoped

### SQL Injection Prevention

```typescript
// ✅ SAFE: Use placeholders
await client.execute('SELECT * FROM parts WHERE id = ?', [id]);

// ❌ UNSAFE: String concatenation
await client.execute(`SELECT * FROM parts WHERE id = ${id}`);
```

---

## Testing Strategy

### Unit Tests

```typescript
// Test Turso client
describe('TursoClient', () => {
  it('should execute queries', async () => {
    const client = createTursoClient({
      url: ':memory:',
      authToken: '',
    });
    const result = await client.execute('SELECT 1 as value');
    expect(result.rows[0].value).toBe(1);
  });
});
```

### Integration Tests

```typescript
// Test data service
describe('DataService', () => {
  it('should fetch parts in cloud mode', async () => {
    process.env.VITE_MODE = 'cloud';
    const parts = await dataService.getAllParts();
    expect(Array.isArray(parts)).toBe(true);
  });
});
```

---

## Deployment Checklist

### Cloud Mode (Production)

- [ ] Set `MODE=cloud` in `.env.production`
- [ ] Configure `TURSO_DATABASE_URL`
- [ ] Configure `TURSO_AUTH_TOKEN`
- [ ] Test connection to Turso
- [ ] Verify data retrieval
- [ ] Check performance metrics

### Local Mode (Development)

- [ ] Set `MODE=local` in `.env.development`
- [ ] Ensure WPF app is running
- [ ] Configure `API_URL`
- [ ] Test API endpoints
- [ ] Verify data flow

---

## Rollback Plan

If Turso integration has issues:

1. **Switch to Local Mode**: Set `MODE=local` in `.env`
2. **Keep API Service**: Original API service still works
3. **Gradual Migration**: Can migrate back and forth
4. **No Data Loss**: Database remains unchanged

---

## Next Steps

1. **Review Plan**: Check all sections
2. **Approve Implementation**: Get stakeholder approval
3. **Start Implementation**: Begin with Phase 1
4. **Test Thoroughly**: Test both modes
5. **Deploy Gradually**: Start with development environment
6. **Monitor Performance**: Track metrics and optimize

---

**Document Status:** Ready for Implementation
**Last Updated:** 2026-03-19
**Prepared By:** Claude Code AI Planning System
