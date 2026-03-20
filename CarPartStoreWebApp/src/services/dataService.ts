/**
 * Data Service - Mode-based data fetching
 * Routes requests to Turso (cloud mode) or WPF API (local mode)
 */

import type {
  CarPartDto,
  CategoryDto,
  DashboardStats,
  PartsFilters,
} from '@/types';
import { getTursoClient, initializeTursoClient } from './tursoClient';
import { partsApi, categoriesApi } from './api';

/**
 * Application Mode
 */
type AppMode = 'cloud' | 'local';

/**
 * Data Service - Handles all data operations with mode-based routing
 */
class DataService {
  private mode: AppMode = 'local';
  private tursoInitialized = false;

  constructor() {
    // Determine mode from environment variable
    const modeFromEnv = import.meta.env.VITE_MODE as string;
    this.mode = modeFromEnv === 'cloud' ? 'cloud' : 'local';

    console.log(`DataService initialized in ${this.mode.toUpperCase()} mode`);

    // Initialize Turso client if in cloud mode
    if (this.isCloudMode()) {
      this.tursoInitialized = initializeTursoClient();
      if (!this.tursoInitialized) {
        console.warn(
          'Failed to initialize Turso client. Falling back to local API mode.'
        );
        this.mode = 'local';
      }
    }
  }

  /**
   * Check if running in cloud mode
   */
  isCloudMode(): boolean {
    return this.mode === 'cloud';
  }

  /**
   * Check if running in local mode
   */
  isLocalMode(): boolean {
    return this.mode === 'local';
  }

  // ==================== PARTS ====================

  /**
   * Get all parts with optional filters
   */
  async getAllParts(filters?: Partial<PartsFilters>): Promise<CarPartDto[]> {
    if (this.isCloudMode()) {
      return this.getPartsFromTurso(filters);
    } else {
      return this.getPartsFromAPI(filters);
    }
  }

  /**
   * Get part by ID
   */
  async getPartById(id: number): Promise<CarPartDto> {
    if (this.isCloudMode()) {
      return this.getPartByIdFromTurso(id);
    } else {
      return partsApi.getById(id);
    }
  }

  /**
   * Get dashboard statistics
   */
  async getDashboardStats(): Promise<DashboardStats> {
    if (this.isCloudMode()) {
      return this.getDashboardStatsFromTurso();
    } else {
      return partsApi.getDashboardStats();
    }
  }

  /**
   * Get low stock parts (stock < 5)
   */
  async getLowStockParts(): Promise<CarPartDto[]> {
    if (this.isCloudMode()) {
      return this.getLowStockPartsFromTurso();
    } else {
      return partsApi.getLowStock();
    }
  }

  /**
   * Get recent parts (last 10 added)
   */
  async getRecentParts(): Promise<CarPartDto[]> {
    if (this.isCloudMode()) {
      return this.getRecentPartsFromTurso();
    } else {
      return partsApi.getRecent();
    }
  }

  // ==================== CATEGORIES ====================

  /**
   * Get all categories
   */
  async getAllCategories(): Promise<CategoryDto[]> {
    if (this.isCloudMode()) {
      return this.getCategoriesFromTurso();
    } else {
      return categoriesApi.getAll();
    }
  }

  /**
   * Get category by ID
   */
  async getCategoryById(id: number): Promise<CategoryDto> {
    if (this.isCloudMode()) {
      return this.getCategoryByIdFromTurso(id);
    } else {
      return categoriesApi.getById(id);
    }
  }

  // ==================== TURSO IMPLEMENTATIONS ====================

  private async getPartsFromTurso(
    filters?: Partial<PartsFilters>
  ): Promise<CarPartDto[]> {
    try {
      const client = getTursoClient();

    let sql = `
      SELECT
        p.id as id,
        p.partNumber as partNumber,
        p.name as name,
        p.description as description,
        p.categoryId as categoryId,
        c.name as categoryName,
        p.costPrice as costPrice,
        p.stockQuantity as stockQuantity,
        p.location as location,
        p.brand as brand,
        p.imagePath as imagePath,
        p.model as model,
        p.releaseYear as releaseYear,
        p.createdDate as createdDate,
        p.lastUpdated as lastUpdated
      FROM parts p
      LEFT JOIN categories c ON p.categoryId = c.id
      WHERE 1=1
    `;

    const args: any[] = [];

    // Apply filters
    if (filters?.search) {
      sql += ` AND (
        p.name LIKE ? OR
        p.description LIKE ? OR
        p.partNumber LIKE ? OR
        p.brand LIKE ? OR
        p.model LIKE ?
      )`;
      const searchTerm = `%${filters.search}%`;
      args.push(searchTerm, searchTerm, searchTerm, searchTerm, searchTerm);
    }

    if (filters?.categoryId) {
      sql += ` AND p.categoryId = ?`;
      args.push(filters.categoryId);
    }

    if (filters?.brand) {
      sql += ` AND p.brand = ?`;
      args.push(filters.brand);
    }

    if (filters?.model) {
      sql += ` AND p.model = ?`;
      args.push(filters.model);
    }

    if (filters?.minYear) {
      sql += ` AND (p.releaseYear IS NOT NULL AND p.releaseYear >= ?)`;
      args.push(filters.minYear);
    }

    if (filters?.maxYear) {
      sql += ` AND (p.releaseYear IS NOT NULL AND p.releaseYear <= ?)`;
      args.push(filters.maxYear);
    }

    if (filters?.stockStatus) {
      switch (filters.stockStatus) {
        case 'in-stock':
          sql += ` AND p.stockQuantity >= 5`;
          break;
        case 'low-stock':
          sql += ` AND p.stockQuantity > 0 AND p.stockQuantity < 5`;
          break;
        case 'out-of-stock':
          sql += ` AND p.stockQuantity = 0`;
          break;
      }
    }

    // Sorting
    if (filters?.sortBy) {
      const sortOrder = filters.sortOrder || 'asc';
      const validSortColumns = [
        'name',
        'brand',
        'model',
        'releaseYear',
        'costPrice',
        'stockQuantity',
        'createdDate',
      ];

      if (validSortColumns.includes(filters.sortBy)) {
        sql += ` ORDER BY p.${filters.sortBy} ${sortOrder.toUpperCase()}`;
      }
    } else {
      sql += ` ORDER BY p.id`;
    }

    // Pagination
    if (filters?.page && filters?.pageSize) {
      const offset = (filters.page - 1) * filters.pageSize;
      sql += ` LIMIT ? OFFSET ?`;
      args.push(filters.pageSize, offset);
    }

    const rows = await client.execute<any>(sql, args);
    return this.mapRowsToParts(rows);
    } catch (error) {
      console.error('Turso fetch error:', error);
      throw error;
    }
  }

  private async getPartByIdFromTurso(id: number): Promise<CarPartDto> {
    const client = getTursoClient();

    const sql = `
      SELECT
        p.id as id,
        p.partNumber as partNumber,
        p.name as name,
        p.description as description,
        p.categoryId as categoryId,
        c.name as categoryName,
        p.costPrice as costPrice,
        p.stockQuantity as stockQuantity,
        p.location as location,
        p.brand as brand,
        p.imagePath as imagePath,
        p.model as model,
        p.releaseYear as releaseYear,
        p.createdDate as createdDate,
        p.lastUpdated as lastUpdated
      FROM parts p
      LEFT JOIN categories c ON p.categoryId = c.id
      WHERE p.id = ?
    `;

    const rows = await client.execute<any>(sql, [id]);

    if (rows.length === 0) {
      throw new Error(`Part with ID ${id} not found`);
    }

    return this.mapRowToPart(rows[0]);
  }

  private async getDashboardStatsFromTurso(): Promise<DashboardStats> {
    const client = getTursoClient();

    // Get total parts and categories
    const basicStats = await client.execute<any>(`
      SELECT
        (SELECT COUNT(*) FROM parts) as totalParts,
        (SELECT COUNT(*) FROM categories) as totalCategories,
        (SELECT COUNT(*) FROM parts WHERE stockQuantity > 0 AND stockQuantity < 5) as lowStockCount,
        (SELECT COUNT(*) FROM parts WHERE stockQuantity = 0) as outOfStockCount
    `);

    // Get parts by category
    const byCategory = await client.execute<any>(`
      SELECT c.name as category, COUNT(p.id) as count
      FROM categories c
      LEFT JOIN parts p ON c.id = p.categoryId
      GROUP BY c.id, c.name
      ORDER BY count DESC
    `);

    // Get stock status distribution
    const stockDistribution = await client.execute<any>(`
      SELECT
        CASE
          WHEN stockQuantity = 0 THEN 'Out of Stock'
          WHEN stockQuantity < 5 THEN 'Low Stock'
          ELSE 'In Stock'
        END as status,
        COUNT(*) as count
      FROM parts
      GROUP BY status
      ORDER BY
        CASE status
          WHEN 'Out of Stock' THEN 1
          WHEN 'Low Stock' THEN 2
          ELSE 3
        END
    `);

    // Get monthly additions (last 6 months)
    const monthlyAdditions = await client.execute<any>(`
      SELECT
        strftime('%Y-%m', createdDate) as month,
        COUNT(*) as count
      FROM parts
      WHERE createdDate >= date('now', '-6 months')
      GROUP BY month
      ORDER BY month DESC
    `);

    return {
      totalParts: Number(basicStats[0]?.totalParts || 0),
      totalCategories: Number(basicStats[0]?.totalCategories || 0),
      lowStockCount: Number(basicStats[0]?.lowStockCount || 0),
      outOfStockCount: Number(basicStats[0]?.outOfStockCount || 0),
      partsByCategory: byCategory.map((row) => ({
        category: String(row.category || 'Uncategorized'),
        count: Number(row.count || 0),
      })),
      stockStatusDistribution: stockDistribution.map((row) => ({
        status: String(row.status),
        count: Number(row.count || 0),
      })),
      monthlyAdditions: monthlyAdditions.map((row) => ({
        month: String(row.month),
        count: Number(row.count || 0),
      })),
    };
  }

  private async getLowStockPartsFromTurso(): Promise<CarPartDto[]> {
    const client = getTursoClient();

    const sql = `
      SELECT
        p.id as id,
        p.partNumber as partNumber,
        p.name as name,
        p.description as description,
        p.categoryId as categoryId,
        c.name as categoryName,
        p.costPrice as costPrice,
        p.stockQuantity as stockQuantity,
        p.location as location,
        p.brand as brand,
        p.imagePath as imagePath,
        p.model as model,
        p.releaseYear as releaseYear,
        p.createdDate as createdDate,
        p.lastUpdated as lastUpdated
      FROM parts p
      LEFT JOIN categories c ON p.categoryId = c.id
      WHERE p.stockQuantity > 0 AND p.stockQuantity < 5
      ORDER BY p.stockQuantity ASC
    `;

    const rows = await client.execute<any>(sql);
    return this.mapRowsToParts(rows);
  }

  private async getRecentPartsFromTurso(): Promise<CarPartDto[]> {
    const client = getTursoClient();

    const sql = `
      SELECT
        p.id as id,
        p.partNumber as partNumber,
        p.name as name,
        p.description as description,
        p.categoryId as categoryId,
        c.name as categoryName,
        p.costPrice as costPrice,
        p.stockQuantity as stockQuantity,
        p.location as location,
        p.brand as brand,
        p.imagePath as imagePath,
        p.model as model,
        p.releaseYear as releaseYear,
        p.createdDate as createdDate,
        p.lastUpdated as lastUpdated
      FROM parts p
      LEFT JOIN categories c ON p.categoryId = c.id
      ORDER BY p.createdDate DESC
      LIMIT 10
    `;

    const rows = await client.execute<any>(sql);
    return this.mapRowsToParts(rows);
  }

  private async getCategoriesFromTurso(): Promise<CategoryDto[]> {
    const client = getTursoClient();

    const sql = `
      SELECT
        id as id,
        name as name,
        description as description,
        parentCategoryId as parentCategoryId,
        displayOrder as displayOrder,
        createdDate as createdDate
      FROM categories
      ORDER BY displayOrder ASC, name ASC
    `;

    const rows = await client.execute<any>(sql);
    return rows.map((row) => ({
      id: Number(row.id),
      name: String(row.name),
      description: row.description || undefined,
      parentCategoryId: row.parentCategoryId ? Number(row.parentCategoryId) : undefined,
      displayOrder: Number(row.displayOrder),
      createdDate: String(row.createdDate),
    }));
  }

  private async getCategoryByIdFromTurso(id: number): Promise<CategoryDto> {
    const client = getTursoClient();

    const sql = `
      SELECT
        id as id,
        name as name,
        description as description,
        parentCategoryId as parentCategoryId,
        displayOrder as displayOrder,
        createdDate as createdDate
      FROM categories
      WHERE id = ?
    `;

    const rows = await client.execute<any>(sql, [id]);

    if (rows.length === 0) {
      throw new Error(`Category with ID ${id} not found`);
    }

    const row = rows[0];
    return {
      id: Number(row.id),
      name: String(row.name),
      description: row.description || undefined,
      parentCategoryId: row.parentCategoryId ? Number(row.parentCategoryId) : undefined,
      displayOrder: Number(row.displayOrder),
      createdDate: String(row.createdDate),
    };
  }

  // ==================== API IMPLEMENTATIONS ====================

  private async getPartsFromAPI(
    filters?: Partial<PartsFilters>
  ): Promise<CarPartDto[]> {
    const params: Record<string, string | number> = {
      page: filters?.page || 1,
      pageSize: filters?.pageSize || 50,
    };

    if (filters?.search) params.search = filters.search;
    if (filters?.categoryId) params.categoryId = filters.categoryId;
    if (filters?.brand) params.brand = filters.brand;
    if (filters?.model) params.model = filters.model;
    if (filters?.minYear) params.minYear = filters.minYear;
    if (filters?.maxYear) params.maxYear = filters.maxYear;
    if (filters?.stockStatus) params.stockStatus = filters.stockStatus;
    if (filters?.sortBy) params.sortBy = filters.sortBy;
    if (filters?.sortOrder) params.sortOrder = filters.sortOrder;

    return partsApi.getAll(params);
  }

  // ==================== MAPPING HELPERS ====================

  private mapRowsToParts(rows: any[]): CarPartDto[] {
    return rows.map((row) => this.mapRowToPart(row));
  }

  private mapRowToPart(row: any): CarPartDto {
    return {
      id: Number(row.id),
      partNumber: row.partNumber || undefined,
      name: String(row.name || ''),
      description: row.description || undefined,
      categoryId: Number(row.categoryId),
      categoryName: row.categoryName || undefined,
      costPrice: Number(row.costPrice || 0),
      stockQuantity: Number(row.stockQuantity || 0),
      location: row.location || undefined,
      brand: row.brand || undefined,
      imagePath: row.imagePath || undefined,
      model: row.model || undefined,
      releaseYear: row.releaseYear ? Number(row.releaseYear) : null,
      createdDate: String(row.createdDate),
      lastUpdated: row.lastUpdated ? String(row.lastUpdated) : undefined,
    };
  }
}

// Export singleton instance
const dataService = new DataService();

export default dataService;
