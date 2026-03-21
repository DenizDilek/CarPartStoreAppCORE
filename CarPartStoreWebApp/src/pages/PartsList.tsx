/**
 * Parts List Page - Portfolio Style
 * Displays car parts as a card grid with filters
 * 16 cards per page with pagination
 */

import { useState, useMemo } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useParts, useCategories } from '@/hooks/useParts';
import { useDebounce } from '@/hooks/useDebounce';
import FilterSidebar, { FilterValues } from '@/components/parts/FilterSidebar';
import PartCardGrid from '@/components/parts/PartCardGrid';
import { Button } from '@/components/ui/button';
import SEO from '@/components/seo/SEO';

const ITEMS_PER_PAGE = 16;

/**
 * Parts List Component (Portfolio-Style)
 */
function PartsList() {
  const { t } = useTranslation();
  const { categoryId } = useParams();
  const [searchParams] = useSearchParams();

  // State for filters
  const [filters, setFilters] = useState<FilterValues>({
    categoryId: categoryId || searchParams.get('category') || '',
    brand: '',
    model: '',
    minYear: '',
    maxYear: '',
    stockStatus: '',
  });

  // State for pagination
  const [currentPage, setCurrentPage] = useState(1);

  // Debounce search term for API calls
  const debouncedSearch = useDebounce(searchParams.get('search') || '', 300);

  // Memoize filter params for API
  const apiParams = useMemo(() => {
    const params: Record<string, any> = {};

    if (debouncedSearch) params.search = debouncedSearch;
    if (filters.categoryId) params.categoryId = parseInt(filters.categoryId as string);
    if (filters.brand) params.brand = filters.brand;
    if (filters.model) params.model = filters.model;
    if (filters.minYear) params.minYear = parseInt(filters.minYear);
    if (filters.maxYear) params.maxYear = parseInt(filters.maxYear);
    if (filters.stockStatus) params.stockStatus = filters.stockStatus;

    return params;
  }, [debouncedSearch, filters]);

  // Fetch data with React Query
  const {
    data: parts = [],
    isLoading: partsLoading,
    error: partsError,
    refetch: refetchParts,
  } = useParts(apiParams);

  const {
    data: categories = [],
    isLoading: categoriesLoading,
  } = useCategories();

  // Calculate pagination
  const totalPages = Math.ceil(parts.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const endIndex = startIndex + ITEMS_PER_PAGE;
  const currentParts = parts.slice(startIndex, endIndex);

  // Reset to page 1 when filters change
  const handleFiltersChange = (newFilters: FilterValues) => {
    setFilters(newFilters);
    setCurrentPage(1);
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Error state
  if (partsError) {
    return (
      <div className="container mx-auto px-4 py-16">
        <div className="rounded-lg border border-destructive bg-destructive/10 p-8 text-center max-w-md mx-auto">
          <div className="text-4xl mb-4">❌</div>
          <p className="text-destructive font-medium mb-4">
            {(partsError as Error).message || t('parts.error')}
          </p>
          <Button
            onClick={() => refetchParts()}
            className="bg-primary hover:bg-primary/90"
          >
            {t('buttons.retry')}
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <SEO
        title={t('parts.title')}
        description="Browse our complete catalog of quality car parts. Filter by category, brand, model, and year to find the perfect parts for your vehicle."
        url="/parts"
      />
      <div className="container mx-auto px-4 py-8">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-foreground mb-2">
            {filters.categoryId
              ? categories.find((c) => c.id.toString() === filters.categoryId)?.name || t('parts.title')
              : t('parts.title')}
          </h1>
          <p className="text-muted-foreground">
            {t('parts.partsAvailable', { count: parts.length })}
          </p>
        </div>

        {/* Two-Column Layout */}
        <div className="flex flex-col lg:flex-row gap-8">
          {/* Left Sidebar - Filters */}
          <aside className="lg:w-1/4">
            <div className="sticky top-24 bg-card rounded-lg border p-6">
              <FilterSidebar
                filters={filters}
                onFiltersChange={handleFiltersChange}
                categories={categories}
                isLoading={categoriesLoading}
              />
            </div>
          </aside>

          {/* Right Content - Parts Grid */}
          <main className="lg:w-3/4">
            <PartCardGrid
              parts={currentParts}
              currentPage={currentPage}
              totalPages={totalPages}
              onPageChange={handlePageChange}
              isLoading={partsLoading}
            />
          </main>
        </div>
      </div>
    </div>
  );
}

export default PartsList;
