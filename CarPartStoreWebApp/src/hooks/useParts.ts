/**
 * React Query Hooks for Parts
 * Custom hooks for fetching car parts data (Read-Only)
 * Uses dataService which routes to Turso (cloud) or WPF API (local)
 */

import { useQuery } from '@tanstack/react-query';
import type {
  CarPartDto,
  CategoryDto,
  DashboardStats,
} from '@/types';
import dataService from '@/services/dataService';

/**
 * Query keys for React Query cache management
 */
export const queryKeys = {
  parts: ['parts'] as const,
  partsList: (filters?: { search?: string; categoryId?: number }) =>
    ['parts', 'list', filters] as const,
  part: (id: number) => ['part', id] as const,
  categories: ['categories'] as const,
  category: (id: number) => ['category', id] as const,
  dashboardStats: ['dashboard', 'stats'] as const,
  lowStock: ['dashboard', 'low-stock'] as const,
  recentParts: ['dashboard', 'recent'] as const,
};

/**
 * Hook to fetch all parts with optional filters
 */
export const useParts = (filters?: {
  search?: string;
  categoryId?: number;
  brand?: string;
  model?: string;
  minYear?: number;
  maxYear?: number;
  stockStatus?: 'in-stock' | 'low-stock' | 'out-of-stock';
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}) => {
  return useQuery<CarPartDto[]>({
    queryKey: queryKeys.partsList(filters),
    queryFn: () => dataService.getAllParts(filters),
    staleTime: 5000, // Consider data fresh for 5 seconds
  });
};

/**
 * Hook to fetch a single part by ID
 */
export const usePart = (id: number) => {
  return useQuery<CarPartDto>({
    queryKey: queryKeys.part(id),
    queryFn: () => dataService.getPartById(id),
    enabled: !!id, // Only fetch if ID is provided
    staleTime: 5000,
  });
};

/**
 * Hook to fetch parts by category
 */
export const usePartsByCategory = (categoryId: number) => {
  return useQuery<CarPartDto[]>({
    queryKey: ['parts', 'category', categoryId] as const,
    queryFn: () => dataService.getAllParts({ categoryId }),
    enabled: !!categoryId,
    staleTime: 5000,
  });
};

/**
 * Hook to search parts
 */
export const useSearchParts = (term: string) => {
  return useQuery<CarPartDto[]>({
    queryKey: ['parts', 'search', term] as const,
    queryFn: () => dataService.getAllParts({ search: term }),
    enabled: term.length >= 2, // Only search with 2+ characters
    staleTime: 3000,
  });
};

/**
 * Hook to fetch all categories
 */
export const useCategories = () => {
  return useQuery<CategoryDto[]>({
    queryKey: queryKeys.categories,
    queryFn: () => dataService.getAllCategories(),
    staleTime: 60000, // Categories change less frequently - cache for 1 minute
  });
};

/**
 * Hook to fetch a single category by ID
 */
export const useCategory = (id: number) => {
  return useQuery<CategoryDto>({
    queryKey: queryKeys.category(id),
    queryFn: () => dataService.getCategoryById(id),
    enabled: !!id,
    staleTime: 60000,
  });
};

/**
 * Hook to fetch dashboard statistics
 */
export const useDashboardStats = () => {
  return useQuery<DashboardStats>({
    queryKey: queryKeys.dashboardStats,
    queryFn: () => dataService.getDashboardStats(),
    staleTime: 30000, // Cache for 30 seconds
  });
};

/**
 * Hook to fetch low stock parts (stock < 5)
 */
export const useLowStockParts = () => {
  return useQuery<CarPartDto[]>({
    queryKey: queryKeys.lowStock,
    queryFn: () => dataService.getLowStockParts(),
    staleTime: 10000, // Cache for 10 seconds
  });
};

/**
 * Hook to fetch recent parts (last 10 added)
 */
export const useRecentParts = () => {
  return useQuery<CarPartDto[]>({
    queryKey: queryKeys.recentParts,
    queryFn: () => dataService.getRecentParts(),
    staleTime: 15000, // Cache for 15 seconds
  });
};
