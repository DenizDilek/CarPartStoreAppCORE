/**
 * API Client
 * Axios-based HTTP client for communicating with the Car Parts Storage API
 */

import axios, { AxiosInstance, AxiosError } from 'axios';
import type {
  CarPartDto,
  CreatePartDto,
  UpdatePartDto,
  CategoryDto,
  CreateCategoryDto,
  UpdateCategoryDto,
  ApiError,
  DashboardStats,
} from '@/types';

// API base URL - connects to the WPF embedded API server
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
const API_BASE_URL = 'http://localhost:5000/api';
=======
>>>>>>> Stashed changes
// Can be configured via environment variable VITE_API_URL
const API_BASE_URL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : 'http://localhost:5000/api';

// Log which API URL is being used
<<<<<<< Updated upstream
console.log(`🔧 API Client initialized with base URL: ${API_BASE_URL}`);
=======
console.log(`API Client initialized with base URL: ${API_BASE_URL}`);
>>>>>>> Stashed changes
>>>>>>> Stashed changes

/**
 * Create and configure axios instance
 */
const createApiInstance = (): AxiosInstance => {
  const instance = axios.create({
    baseURL: API_BASE_URL,
    timeout: 10000,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  // Request interceptor
  instance.interceptors.request.use(
    (config) => {
      console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`);
      return config;
    },
    (error) => {
      console.error('API Request Error:', error);
      return Promise.reject(error);
    }
  );

  // Response interceptor
  instance.interceptors.response.use(
    (response) => {
      console.log(`API Response: ${response.status} ${response.config.url}`);
      return response;
    },
    (error: AxiosError<ApiError>) => {
      console.error('API Response Error:', error);

      if (error.response) {
        // Server responded with error status
        return Promise.reject({
          message: error.response.data?.message || error.message,
          status: error.response.status,
          errors: error.response.data?.errors,
        });
      } else if (error.request) {
        // Request was made but no response received
        return Promise.reject({
          message: 'No response from server. Please ensure the WPF app is running.',
          status: 0,
        });
      } else {
        // Error setting up the request
        return Promise.reject({
          message: error.message || 'Unknown error occurred',
          status: 0,
        });
      }
    }
  );

  return instance;
};

const apiClient = createApiInstance();

/**
 * Car Parts API
 */
export const partsApi = {
  /**
   * Get all parts with optional filters
   */
  getAll: async (params?: { search?: string; categoryId?: number }) => {
    const response = await apiClient.get<CarPartDto[]>('/parts', { params });
    return response.data;
  },

  /**
   * Get a specific part by ID
   */
  getById: async (id: number) => {
    const response = await apiClient.get<CarPartDto>(`/parts/${id}`);
    return response.data;
  },

  /**
   * Create a new part
   */
  create: async (data: CreatePartDto) => {
    const response = await apiClient.post<CarPartDto>('/parts', data);
    return response.data;
  },

  /**
   * Update an existing part
   */
  update: async (id: number, data: UpdatePartDto) => {
    const response = await apiClient.put<void>(`/parts/${id}`, data);
    return response.data;
  },

  /**
   * Delete a part
   */
  delete: async (id: number) => {
    const response = await apiClient.delete<void>(`/parts/${id}`);
    return response.data;
  },

  /**
   * Get parts by category
   */
  getByCategory: async (categoryId: number) => {
    const response = await apiClient.get<CarPartDto[]>(`/parts/category/${categoryId}`);
    return response.data;
  },

  /**
   * Search parts
   */
  search: async (term: string) => {
    const response = await apiClient.get<CarPartDto[]>('/parts/search', {
      params: { term },
    });
    return response.data;
  },

  /**
   * Get dashboard statistics
   */
  getDashboardStats: async () => {
    const response = await apiClient.get<DashboardStats>('/parts/dashboard/stats');
    return response.data;
  },

  /**
   * Get low stock parts (stock < 5)
   */
  getLowStock: async () => {
    const response = await apiClient.get<CarPartDto[]>('/parts/dashboard/low-stock');
    return response.data;
  },

  /**
   * Get recent parts (last 10 added)
   */
  getRecent: async () => {
    const response = await apiClient.get<CarPartDto[]>('/parts/dashboard/recent');
    return response.data;
  },
};

/**
 * Categories API
 */
export const categoriesApi = {
  /**
   * Get all categories
   */
  getAll: async () => {
    const response = await apiClient.get<CategoryDto[]>('/categories');
    return response.data;
  },

  /**
   * Get a specific category by ID
   */
  getById: async (id: number) => {
    const response = await apiClient.get<CategoryDto>(`/categories/${id}`);
    return response.data;
  },

  /**
   * Create a new category
   */
  create: async (data: CreateCategoryDto) => {
    const response = await apiClient.post<CategoryDto>('/categories', data);
    return response.data;
  },

  /**
   * Update an existing category
   */
  update: async (id: number, data: UpdateCategoryDto) => {
    const response = await apiClient.put<void>(`/categories/${id}`, data);
    return response.data;
  },

  /**
   * Delete a category
   */
  delete: async (id: number) => {
    const response = await apiClient.delete<void>(`/categories/${id}`);
    return response.data;
  },

  /**
   * Get parts within a category
   */
  getParts: async (id: number) => {
    const response = await apiClient.get<CarPartDto[]>(`/categories/${id}/parts`);
    return response.data;
  },
};

export default apiClient;
