/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_TURSO_DATABASE_URL: string;
  readonly VITE_TURSO_AUTH_TOKEN: string;
  readonly VITE_API_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

/**
 * DTOs for Car Parts API
 * These types match the C# DTOs in the backend
 */

export interface CarPartDto {
  id: number;
  partNumber: string;
  name: string;
  description?: string;
  categoryId: number;
  categoryName?: string;
  costPrice: number;
  retailPrice: number;
  stockQuantity: number;
  location?: string;
  supplier?: string;
  imagePath?: string;
  model?: string;
  releaseDate?: number | null;
  createdDate: string;
  lastUpdated?: string;
}

export interface CreatePartDto {
  partNumber: string;
  name: string;
  description?: string;
  categoryId: number;
  costPrice: number;
  retailPrice: number;
  stockQuantity: number;
  location?: string;
  supplier?: string;
  imagePath?: string;
  model?: string;
  releaseDate?: number | null;
}

export interface UpdatePartDto {
  partNumber: string;
  name: string;
  description?: string;
  categoryId: number;
  costPrice: number;
  retailPrice: number;
  stockQuantity: number;
  location?: string;
  supplier?: string;
  imagePath?: string;
  model?: string;
  releaseDate?: number | null;
}

export interface CategoryDto {
  id: number;
  name: string;
  description?: string;
  parentCategoryId?: number;
  displayOrder: number;
  createdDate: string;
}

export interface ApiError {
  message: string;
  status?: number;
  errors?: Record<string, string[]>;
}
