import { X } from 'lucide-react';
import { useTranslation } from 'react-i18next';

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
  const { t } = useTranslation();

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

  // Helper function to get stock status label
  const getStockStatusLabel = (status: string): string => {
    switch (status) {
      case 'in-stock':
        return t('filters.inStock');
      case 'low-stock':
        return t('filters.lowStock');
      case 'out-of-stock':
        return t('filters.outOfStock');
      default:
        return status;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-foreground">{t('filters.title')}</h2>
        {hasActiveFilters && (
          <button
            onClick={clearFilters}
            className="text-sm text-primary hover:text-primary/90 flex items-center gap-1"
          >
            <X className="h-4 w-4" />
            {t('filters.clearAll')}
          </button>
        )}
      </div>

      {/* Category Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">{t('filters.category')}</label>
        {isLoading ? (
          <div className="w-full h-10 bg-muted rounded-md animate-pulse" />
        ) : (
          <select
            value={filters.categoryId || ''}
            onChange={(e) => updateFilter('categoryId', e.target.value)}
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
          >
            <option value="">{t('filters.allCategories')}</option>
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
        <label className="text-sm font-medium text-foreground">{t('filters.brand')}</label>
        <input
          type="text"
          value={filters.brand || ''}
          onChange={(e) => updateFilter('brand', e.target.value)}
          placeholder={t('filters.brandPlaceholder')}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        />
      </div>

      {/* Model Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">{t('filters.model')}</label>
        <input
          type="text"
          value={filters.model || ''}
          onChange={(e) => updateFilter('model', e.target.value)}
          placeholder={t('filters.modelPlaceholder')}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        />
      </div>

      {/* Year Range Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">{t('filters.releaseYear')}</label>
        <div className="flex gap-2">
          <input
            type="number"
            value={filters.minYear || ''}
            onChange={(e) => updateFilter('minYear', e.target.value)}
            placeholder={t('filters.from')}
            min="1900"
            max={new Date().getFullYear() + 1}
            className="flex-1 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
          />
          <input
            type="number"
            value={filters.maxYear || ''}
            onChange={(e) => updateFilter('maxYear', e.target.value)}
            placeholder={t('filters.to')}
            min="1900"
            max={new Date().getFullYear() + 1}
            className="flex-1 rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
          />
        </div>
      </div>

      {/* Stock Status Filter */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">{t('filters.stockStatus')}</label>
        <select
          value={filters.stockStatus || ''}
          onChange={(e) => updateFilter('stockStatus', e.target.value)}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        >
          <option value="">{t('filters.all')}</option>
          <option value="in-stock">{t('filters.inStock')}</option>
          <option value="low-stock">{t('filters.lowStock')}</option>
          <option value="out-of-stock">{t('filters.outOfStock')}</option>
        </select>
      </div>

      {/* Active Filters Display */}
      {hasActiveFilters && (
        <div className="pt-4 border-t">
          <p className="text-sm font-medium text-foreground mb-2">{t('filters.active')}</p>
          <div className="flex flex-wrap gap-2">
            {filters.categoryId && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                {t('filters.categoryLabel')} {categories.find((c) => String(c.id) === String(filters.categoryId))?.name || 'Unknown'}
              </span>
            )}
            {filters.brand && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                {t('filters.brandLabel')} {filters.brand}
              </span>
            )}
            {filters.model && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                {t('filters.modelLabel')} {filters.model}
              </span>
            )}
            {filters.minYear && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                {t('filters.fromLabel')} {filters.minYear}
              </span>
            )}
            {filters.maxYear && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                {t('filters.toLabel')} {filters.maxYear}
              </span>
            )}
            {filters.stockStatus && (
              <span className="text-xs bg-primary/10 text-primary px-2 py-1 rounded-full">
                {getStockStatusLabel(filters.stockStatus)}
              </span>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
