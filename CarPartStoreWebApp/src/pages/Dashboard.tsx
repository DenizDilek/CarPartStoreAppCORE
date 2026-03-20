/**
 * Dashboard Page
 * Main dashboard showing KPIs, charts, and recent activity
 */

import { useDashboardStats, useLowStockParts, useRecentParts } from '@/hooks/useParts';
import { KPICard } from '@/components/dashboard/kpi-card';
import { CategoryChart } from '@/components/dashboard/category-chart';
import { StockChart } from '@/components/dashboard/stock-chart';
import { RecentParts } from '@/components/dashboard/recent-parts';
import { LowStockAlert } from '@/components/dashboard/low-stock-alert';
import {
  Package,
  FolderTree,
  AlertTriangle,
  XCircle,
} from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';

export default function Dashboard() {
  // Fetch dashboard data
  const {
    data: stats,
    isLoading: statsLoading,
    error: statsError,
    refetch: refetchStats,
  } = useDashboardStats();

  const {
    data: lowStockParts,
    isLoading: lowStockLoading,
  } = useLowStockParts();

  const {
    data: recentParts,
    isLoading: recentLoading,
  } = useRecentParts();

  // Show error state
  if (statsError) {
    return (
      <Card className="border-destructive">
        <CardContent className="pt-6">
          <div className="flex flex-col items-center gap-4 text-center">
            <AlertTriangle className="h-12 w-12 text-destructive" />
            <div>
              <h3 className="text-lg font-semibold">Error Loading Dashboard</h3>
              <p className="text-sm text-muted-foreground">
                {(statsError as Error).message || 'Failed to load dashboard data. Please try again.'}
              </p>
            </div>
            <button
              onClick={() => refetchStats()}
              className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
            >
              Retry
            </button>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground">
          Overview of your car parts inventory
        </p>
      </div>

      {/* KPI Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <KPICard
          title="Total Parts"
          value={stats?.totalParts ?? 0}
          icon={Package}
          description="Parts in inventory"
          variant="default"
          isLoading={statsLoading}
        />
        <KPICard
          title="Total Categories"
          value={stats?.totalCategories ?? 0}
          icon={FolderTree}
          description="Product categories"
          variant="default"
          isLoading={statsLoading}
        />
        <KPICard
          title="Low Stock Items"
          value={stats?.lowStockCount ?? 0}
          icon={AlertTriangle}
          description="Stock below 5 units"
          variant="warning"
          isLoading={statsLoading}
        />
        <KPICard
          title="Out of Stock"
          value={stats?.outOfStockCount ?? 0}
          icon={XCircle}
          description="Items to reorder"
          variant="destructive"
          isLoading={statsLoading}
        />
      </div>

      {/* Charts Row */}
      <div className="grid gap-4 lg:grid-cols-2">
        <CategoryChart
          data={stats?.partsByCategory ?? []}
          isLoading={statsLoading}
        />
        <StockChart
          data={stats?.stockStatusDistribution ?? []}
          isLoading={statsLoading}
        />
      </div>

      {/* Bottom Section: Recent Parts and Low Stock Alerts */}
      <div className="grid gap-4 lg:grid-cols-2">
        <RecentParts parts={recentParts ?? []} isLoading={recentLoading} />
        <LowStockAlert parts={lowStockParts ?? []} isLoading={lowStockLoading} />
      </div>
    </div>
  );
}
