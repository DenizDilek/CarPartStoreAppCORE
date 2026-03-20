import { X } from 'lucide-react';

export interface FilterValues {
  categoryId?: string;
  brand?: string;
  model?: string;
  minYear?: string;
  maxYear?: string;
  stockStatus?: string;
}

interface FilterSidebarProps {
  filters: FilterValues;
  onFiltersChange: (filters: FilterValues) => void;
  categories: Array<{ id: number; name: string }>;
  isLoading?: boolean;
}

export default function FilterSidebar({ filters, onFiltersChange, categories, isLoading }: FilterSidebarProps) {
  const updateFilter = (key: keyof FilterValues, value: string) => {
    onFiltersChange({
      ...filters,
      [key]: value || undefined,
    });
  };

  const clearFilters = () => {
    onFiltersChange({});
  };

  const hasActiveFilters = Object.values(filters).some((v) => v !== undefined);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-foreground">Filters</h2>
        {hasActiveFilters && (
          <button
            onClick={clearFilters}
            className="text-sm text-primary hover:text-primary/90 flex items-center gap-1"
          >
            <X className="h-4 w-4" />
            Clear All
          </button>
        )}
      </div>

      {/* Category Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">Category</label>
        {isLoading ? (
          <div className="w-full h-10 bg-muted rounded-md animate-pulse" />
        ) : (
          <select
            value={filters.categoryId || ''}
            onChange={(e) => updateFilter('categoryId', e.target.value)}
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
          >
            <option value="">All Categories</option>
            {categories.map((category) => (
              <option key={category.id} value={String(category.id || '')}>
                {category.name}
              </option>
            ))}
          </select>
        )}
      </div>

      {/* Brand Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">Brand</label>
        <input
          type="text"
          value={filters.brand || ''}
          onChange={(e) => updateFilter('brand', e.target.value)}
          placeholder="e.g., Toyota, BMW"
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        />
      </div>

      {/* Model Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">Model</label>
        <input
          type="text"
          value={filters.model || ''}
          onChange={(e) => updateFilter('model', e.target.value)}
          placeholder="e.g., Camry, X5"
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        />
      </div>

      {/* Year Range Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">Release Year</label>
        <div className="flex gap-2">
          <input
            type="number"
            value={filters.minYear || ''}
            onChange={(e) => updateFilter('minYear', e.target.value)}
            placeholder="From"
            min="1900"
            max={new Date().getFullYear() + 1}
            className="flex-1 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
          />
          <input
            type="number"
            value={filters.maxYear || ''}
            onChange={(e) => updateFilter('maxYear', e.target.value)}
            placeholder="To"
            min="1900"
            max={new Date().getFullYear() + 1}
            className="flex-1 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
          />
        </div>
      </div>

      {/* Stock Status Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">Stock Status</label>
        <select
          value={filters.stockStatus || ''}
          onChange={(e) => updateFilter('stockStatus', e.target.value)}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        >
          <option value="">All</option>
          <option value="in-stock">In Stock</option>
          <option value="low-stock">Low Stock</option>
          <option value="out-of-stock">Out of Stock</option>
        </select>
      </div>

      {/* Active Filters Display */}
      {hasActiveFilters && (
        <div className="pt-4 border-t">
          <p className="text-sm font-medium text-foreground mb-2">Active Filters:</p>
          <div className="flex flex-wrap gap-2">
            {filters.categoryId && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                Category: {categories.find((c) => String(c.id) === String(filters.categoryId))?.name || 'Unknown'}
              </span>
            )}
            {filters.brand && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                Brand: {filters.brand}
              </span>
            )}
            {filters.model && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                Model: {filters.model}
              </span>
            )}
            {filters.minYear && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                From: {filters.minYear}
              </span>
            )}
            {filters.maxYear && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                To: {filters.maxYear}
              </span>
            )}
            {filters.stockStatus && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                {filters.stockStatus === 'in-stock' && 'In Stock'}
                {filters.stockStatus === 'low-stock' && 'Low Stock'}
                {filters.stockStatus === 'out-of-stock' && 'Out of Stock'}
              </span>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
