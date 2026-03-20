/**
 * Categories Page
 * Read-only grid view of all categories with statistics
 */

import * as React from 'react'
import { Package } from 'lucide-react'
import { Link } from 'react-router-dom'
import { CategoryStats } from '@/components/categories/category-stats'
import { Skeleton } from '@/components/ui/skeleton'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { useCategories, useParts } from '@/hooks/useParts'

/**
 * Summary cards component for categories overview
 */
function CategorySummary({
  totalCategories,
  totalParts,
  isLoading,
}: {
  totalCategories: number
  totalParts: number
  isLoading: boolean
}) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <Card>
        <CardContent className="pt-6">
          {isLoading ? (
            <Skeleton className="h-20 w-full" />
          ) : (
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Total Categories
                </p>
                <p className="text-3xl font-bold">{totalCategories}</p>
              </div>
              <Package className="h-10 w-10 text-muted-foreground/50" />
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6">
          {isLoading ? (
            <Skeleton className="h-20 w-full" />
          ) : (
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Total Parts Across All Categories
                </p>
                <p className="text-3xl font-bold">{totalParts}</p>
              </div>
              <Package className="h-10 w-10 text-muted-foreground/50" />
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

export default function Categories() {
  // Fetch categories
  const { data: categories = [], isLoading: categoriesLoading } = useCategories()

  // Fetch all parts to count per category
  const { data: allParts = [], isLoading: partsLoading } = useParts()

  /**
   * Calculate parts count per category
   */
  const partsCounts = React.useMemo(() => {
    const counts = new Map<number, number>()
    allParts.forEach((part) => {
      counts.set(part.categoryId, (counts.get(part.categoryId) || 0) + 1)
    })
    return counts
  }, [allParts])

  const totalParts = allParts.length

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Categories</h1>
          <p className="text-muted-foreground">
            View product categories for organizing parts inventory
          </p>
        </div>
      </div>

      {/* Summary Cards */}
      <CategorySummary
        totalCategories={categories.length}
        totalParts={totalParts}
        isLoading={categoriesLoading || partsLoading}
      />

      {/* Stats Section */}
      {!categoriesLoading && !partsLoading && categories.length > 0 && (
        <div className="grid gap-6 md:grid-cols-2">
          <CategoryStats
            categories={categories}
            partsCounts={Array.from(partsCounts.entries()).map(([categoryId, count]) => ({
              categoryId,
              categoryName: categories.find((c) => c.id === categoryId)?.name || 'Unknown',
              count,
            }))}
            totalParts={totalParts}
          />
        </div>
      )}

      {/* Categories Grid */}
      {(categoriesLoading || partsLoading) ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <div key={i} className="space-y-3 rounded-lg border p-6">
              <Skeleton className="h-6 w-2/3" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-1/2" />
            </div>
          ))}
        </div>
      ) : categories.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {categories.map((category) => {
            const partsCount = partsCounts.get(category.id) || 0
            return (
              <Link
                key={category.id}
                to={`/category/${category.id}`}
                className="group transition-shadow hover:shadow-md rounded-lg border p-6 block hover:bg-accent/50"
              >
                <div className="flex items-start justify-between mb-4">
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold">{category.name}</h3>
                    {category.description && (
                      <p className="mt-1 text-sm text-muted-foreground line-clamp-2">
                        {category.description}
                      </p>
                    )}
                  </div>
                  <Badge variant="secondary" className="ml-2">
                    #{category.displayOrder}
                  </Badge>
                </div>

                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Package className="h-4 w-4" />
                  <span>{partsCount} part{partsCount !== 1 ? 's' : ''}</span>
                </div>

                <div className="mt-4 text-sm text-primary group-hover:underline">
                  View Parts →
                </div>
              </Link>
            )
          })}
        </div>
      ) : (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-muted">
              <Package className="h-6 w-6 text-muted-foreground" />
            </div>
            <h3 className="mt-4 text-lg font-semibold">No categories found</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              No categories are available in the system.
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
