/**
 * TypeScript types for Car Parts Storage App
 * These types match the C# DTOs in the backend
 */

/**
 * Car Part DTO - matches CarPartDto.cs
 */
export interface CarPartDto {
  id: number;
  partNumber?: string; // Optional in WPF
  name: string;
  description?: string;
  categoryId: number;
  categoryName?: string;
  costPrice: number;
  stockQuantity: number;
  location?: string;
  brand?: string;
  imagePath?: string;
  model?: string;
  releaseYear?: number | null;
  createdDate: string;
  lastUpdated?: string;
}

/**
 * Create Part DTO - matches CreatePartDto.cs
 */
export interface CreatePartDto {
  partNumber?: string; // Optional in WPF
  name: string;
  description?: string;
  categoryId: number;
  costPrice: number;
  stockQuantity: number;
  location?: string;
  brand?: string;
  imagePath?: string;
  model?: string;
  releaseYear?: number | null;
}

/**
 * Update Part DTO - matches UpdatePartDto.cs
 */
export interface UpdatePartDto {
  partNumber?: string; // Optional in WPF
  name: string;
  description?: string;
  categoryId: number;
  costPrice: number;
  stockQuantity: number;
  location?: string;
  brand?: string;
  imagePath?: string;
  model?: string;
  releaseYear?: number | null;
}

/**
 * Category DTO - matches CategoryDto.cs
 */
export interface CategoryDto {
  id: number;
  name: string;
  description?: string;
  parentCategoryId?: number;
  displayOrder: number;
  createdDate: string;
}

/**
 * Create Category DTO
 */
export interface CreateCategoryDto {
  name: string;
  description?: string;
  parentCategoryId?: number;
  displayOrder?: number;
}

/**
 * Update Category DTO
 */
export interface UpdateCategoryDto {
  name: string;
  description?: string;
  parentCategoryId?: number;
  displayOrder?: number;
}

/**
 * Dashboard Stats for the dashboard page
 */
export interface DashboardStats {
  totalParts: number;
  totalCategories: number;
  lowStockCount: number;
  outOfStockCount: number;
  partsByCategory: { category: string; count: number }[];
  stockStatusDistribution: { status: string; count: number }[];
  monthlyAdditions: { month: string; count: number }[];
}

/**
 * Parts filters for the parts list page
 */
export interface PartsFilters {
  search?: string;
  categoryId?: number;
  brand?: string;
  model?: string;
  minYear?: number;
  maxYear?: number;
  stockStatus?: 'in-stock' | 'low-stock' | 'out-of-stock';
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * API Error response
 */
export interface ApiError {
  message: string;
  status?: number;
  errors?: Record<string, string[]>;
}
