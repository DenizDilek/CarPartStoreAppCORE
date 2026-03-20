/**
 * Category Stats Component
 * Displays statistics about category parts distribution
 */

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Progress } from '@/components/ui/progress'
import type { CategoryDto } from '@/types'

interface CategoryPartsCount {
  categoryId: number
  categoryName: string
  count: number
}

interface CategoryStatsProps {
  categories: CategoryDto[]
  partsCounts: CategoryPartsCount[]
  totalParts: number
}

export function CategoryStats({ categories, partsCounts, totalParts }: CategoryStatsProps) {
  // Create a map of category ID to parts count
  const countMap = new Map(partsCounts.map((item) => [item.categoryId, item.count]))

  // Sort categories by parts count (descending)
  const sortedCategories = [...categories].sort((a, b) => {
    const countA = countMap.get(a.id) || 0
    const countB = countMap.get(b.id) || 0
    return countB - countA
  })

  const maxCount = Math.max(...sortedCategories.map((c) => countMap.get(c.id) || 0), 1)

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Parts by Category</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {sortedCategories.map((category) => {
            const count = countMap.get(category.id) || 0
            const percentage = totalParts > 0 ? (count / totalParts) * 100 : 0

            return (
              <div key={category.id} className="space-y-2">
                <div className="flex items-center justify-between text-sm">
                  <span className="font-medium">{category.name}</span>
                  <span className="text-muted-foreground">
                    {count} parts ({percentage.toFixed(1)}%)
                  </span>
                </div>
                <Progress value={(count / maxCount) * 100} className="h-2" />
              </div>
            )
          })}

          {sortedCategories.length === 0 && (
            <p className="py-4 text-center text-sm text-muted-foreground">
              No categories found
            </p>
          )}
        </div>
      </CardContent>
    </Card>
  )
}

/**
 * Simple summary card for categories
 */
interface CategorySummaryProps {
  totalCategories: number
  totalParts: number
  isLoading?: boolean
}

export function CategorySummary({
  totalCategories,
  totalParts,
  isLoading = false,
}: CategorySummaryProps) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Total Categories</CardTitle>
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth="2"
            className="h-4 w-4 text-muted-foreground"
          >
            <path d="M7 7h10v10H7z" />
            <path d="M7 7V4a1 1 0 0 1 1-1h8a1 1 0 0 1 1 1v3" />
            <path d="M7 17v3a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1v-3" />
          </svg>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {isLoading ? '...' : totalCategories}
          </div>
          <p className="text-xs text-muted-foreground">
            Active categories
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">Total Parts</CardTitle>
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth="2"
            className="h-4 w-4 text-muted-foreground"
          >
            <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
          </svg>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {isLoading ? '...' : totalParts}
          </div>
          <p className="text-xs text-muted-foreground">
            Across all categories
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
