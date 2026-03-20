/**
 * Parts Filters Component
 * Advanced filter panel for the parts list with search, category, brand, model, year filters
 */

import { useState } from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Search,
  X,
  SlidersHorizontal,
} from 'lucide-react';
import type { CategoryDto } from '@/types';

export interface PartsFilterValues {
  search: string;
  categoryId: number | undefined;
  brand: string;
  model: string;
  minYear: string;
  maxYear: string;
  stockStatus: 'all' | 'in-stock' | 'low-stock' | 'out-of-stock';
}

interface PartsFiltersProps {
  categories: CategoryDto[] | undefined;
  filters: PartsFilterValues;
  onFiltersChange: (filters: PartsFilterValues) => void;
  onReset: () => void;
  isLoading?: boolean;
  className?: string;
}

const uniqueBrands = [
  'Toyota', 'Honda', 'Ford', 'Chevrolet', 'BMW', 'Mercedes-Benz',
  'Audi', 'Volkswagen', 'Nissan', 'Hyundai', 'Kia', 'Subaru',
  'Mazda', 'Lexus', 'Acura', 'Infiniti', 'Volvo', 'Jaguar',
  'Land Rover', 'Porsche', 'Tesla', 'Genesis', 'Mitsubishi'
];

export function PartsFilters({
  categories,
  filters,
  onFiltersChange,
  onReset,
  isLoading = false,
  className,
}: PartsFiltersProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  const updateFilter = (key: keyof PartsFilterValues, value: string | number | undefined) => {
    onFiltersChange({
      ...filters,
      [key]: value,
    });
  };

  const hasActiveFilters =
    filters.search ||
    filters.categoryId ||
    filters.brand ||
    filters.model ||
    filters.minYear ||
    filters.maxYear ||
    filters.stockStatus !== 'all';

  return (
    <Card className={className}>
      <CardContent className="pt-6">
        {/* Main filter row - always visible */}
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          {/* Search input */}
          <div className="flex-1">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search parts by name, number, brand..."
                value={filters.search}
                onChange={(e) => updateFilter('search', e.target.value)}
                disabled={isLoading}
                className="pl-10"
              />
            </div>
          </div>

          {/* Filter toggle and reset */}
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setIsExpanded(!isExpanded)}
              className="gap-2"
            >
              <SlidersHorizontal className="h-4 w-4" />
              <span className="hidden sm:inline">Filters</span>
              {hasActiveFilters && (
                <span className="ml-1 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-xs text-primary-foreground">
                  !
                </span>
              )}
            </Button>
            {hasActiveFilters && (
              <Button
                variant="ghost"
                size="sm"
                onClick={onReset}
                disabled={isLoading}
                className="gap-2"
              >
                <X className="h-4 w-4" />
                <span className="hidden sm:inline">Reset</span>
              </Button>
            )}
          </div>
        </div>

        {/* Expanded filters */}
        {isExpanded && (
          <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {/* Category filter */}
            <div className="space-y-2">
              <Label htmlFor="category">Category</Label>
              <Select
                value={filters.categoryId?.toString() || 'all'}
                onValueChange={(value) => {
                  updateFilter('categoryId', value === 'all' ? undefined : parseInt(value));
                }}
                disabled={isLoading}
              >
                <SelectTrigger id="category">
                  <SelectValue placeholder="All Categories" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Categories</SelectItem>
                  {categories?.map((category) => (
                    <SelectItem key={category.id} value={category.id.toString()}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Brand filter */}
            <div className="space-y-2">
              <Label htmlFor="brand">Brand</Label>
              <Select
                value={filters.brand || 'all'}
                onValueChange={(value) => {
                  updateFilter('brand', value === 'all' ? '' : value);
                }}
                disabled={isLoading}
              >
                <SelectTrigger id="brand">
                  <SelectValue placeholder="All Brands" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Brands</SelectItem>
                  {uniqueBrands.map((brand) => (
                    <SelectItem key={brand} value={brand}>
                      {brand}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Model filter */}
            <div className="space-y-2">
              <Label htmlFor="model">Model</Label>
              <Input
                id="model"
                placeholder="e.g., Camry, F-150"
                value={filters.model}
                onChange={(e) => updateFilter('model', e.target.value)}
                disabled={isLoading}
              />
            </div>

            {/* Stock status filter */}
            <div className="space-y-2">
              <Label htmlFor="stockStatus">Stock Status</Label>
              <Select
                value={filters.stockStatus}
                onValueChange={(value: any) => updateFilter('stockStatus', value)}
                disabled={isLoading}
              >
                <SelectTrigger id="stockStatus">
                  <SelectValue placeholder="All Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="in-stock">In Stock</SelectItem>
                  <SelectItem value="low-stock">Low Stock</SelectItem>
                  <SelectItem value="out-of-stock">Out of Stock</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Year range filters */}
            <div className="space-y-2 sm:col-span-2">
              <Label>Release Year Range</Label>
              <div className="flex gap-2">
                <Input
                  placeholder="Min year"
                  type="number"
                  min="1900"
                  max={new Date().getFullYear() + 1}
                  value={filters.minYear}
                  onChange={(e) => updateFilter('minYear', e.target.value)}
                  disabled={isLoading}
                />
                <Input
                  placeholder="Max year"
                  type="number"
                  min="1900"
                  max={new Date().getFullYear() + 1}
                  value={filters.maxYear}
                  onChange={(e) => updateFilter('maxYear', e.target.value)}
                  disabled={isLoading}
                />
              </div>
            </div>

            {/* Apply/Reset buttons for mobile */}
            <div className="flex items-end gap-2 sm:col-span-2 lg:col-span-2">
              <Button
                variant="outline"
                className="flex-1"
                onClick={onReset}
                disabled={isLoading || !hasActiveFilters}
              >
                Reset Filters
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
