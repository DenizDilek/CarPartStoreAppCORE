/**
 * React Query Hooks for Parts
 * Custom hooks for fetching and managing car parts data
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { partsApi, categoriesApi } from '../services/api';

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
};

/**
 * Hook to fetch all parts with optional filters
 */
export const useParts = (filters?: { search?: string; categoryId?: number }) => {
  return useQuery({
    queryKey: queryKeys.partsList(filters),
    queryFn: () => partsApi.getAll(filters),
    staleTime: 5000, // Consider data fresh for 5 seconds
  });
};

/**
 * Hook to fetch a single part by ID
 */
export const usePart = (id: number) => {
  return useQuery({
    queryKey: queryKeys.part(id),
    queryFn: () => partsApi.getById(id),
    enabled: !!id, // Only fetch if ID is provided
    staleTime: 5000,
  });
};

/**
 * Hook to fetch parts by category
 */
export const usePartsByCategory = (categoryId: number) => {
  return useQuery({
    queryKey: ['parts', 'category', categoryId] as const,
    queryFn: () => partsApi.getByCategory(categoryId),
    enabled: !!categoryId,
    staleTime: 5000,
  });
};

/**
 * Hook to search parts
 */
export const useSearchParts = (term: string) => {
  return useQuery({
    queryKey: ['parts', 'search', term] as const,
    queryFn: () => partsApi.search(term),
    enabled: term.length >= 2, // Only search with 2+ characters
    staleTime: 3000,
  });
};

/**
 * Hook to create a new part
 */
export const useCreatePart = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreatePartDto) => partsApi.create(data),
    onSuccess: () => {
      // Invalidate and refetch parts list
      queryClient.invalidateQueries({ queryKey: queryKeys.partsList() });
      queryClient.invalidateQueries({ queryKey: queryKeys.categories });
    },
  });
};

/**
 * Hook to update an existing part
 */
export const useUpdatePart = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdatePartDto }) =>
      partsApi.update(id, data),
    onSuccess: (_, variables) => {
      // Invalidate and refetch parts list and specific part
      queryClient.invalidateQueries({ queryKey: queryKeys.partsList() });
      queryClient.invalidateQueries({ queryKey: queryKeys.part(variables.id) });
    },
  });
};

/**
 * Hook to delete a part
 */
export const useDeletePart = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => partsApi.delete(id),
    onSuccess: () => {
      // Invalidate and refetch parts list
      queryClient.invalidateQueries({ queryKey: queryKeys.partsList() });
      queryClient.invalidateQueries({ queryKey: queryKeys.categories });
    },
  });
};

/**
 * Hook to fetch all categories
 */
export const useCategories = () => {
  return useQuery({
    queryKey: queryKeys.categories,
    queryFn: () => categoriesApi.getAll(),
    staleTime: 60000, // Categories change less frequently - cache for 1 minute
  });
};

/**
 * Hook to fetch a single category by ID
 */
export const useCategory = (id: number) => {
  return useQuery({
    queryKey: queryKeys.category(id),
    queryFn: () => categoriesApi.getById(id),
    enabled: !!id,
    staleTime: 60000,
  });
};
