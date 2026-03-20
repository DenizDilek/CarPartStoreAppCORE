/**
 * Category Chart Component
 * Displays a pie chart showing the distribution of parts by category
 */

import {
  PieChart as RechartsPieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Legend,
  Tooltip,
} from 'recharts';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';

const CHART_COLORS = [
  '#3182CE', // primary
  '#48BB78', // success
  '#ECC94B', // warning
  '#F56565', // destructive
  '#9F7AEA', // purple
  '#ED8936', // orange
  '#38B2AC', // teal
  '#667EEA', // indigo
  '#ED64A6', // pink
  '#4299E1', // blue
];

export interface CategoryChartData {
  category: string;
  count: number;
}

interface CategoryChartProps {
  data: CategoryChartData[];
  isLoading?: boolean;
  className?: string;
}

export function CategoryChart({ data, isLoading = false, className }: CategoryChartProps) {
  if (isLoading) {
    return (
      <Card className={className}>
        <CardHeader>
          <Skeleton className="h-6 w-40" />
          <Skeleton className="h-4 w-56" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-[300px] w-full" />
        </CardContent>
      </Card>
    );
  }

  // Format data for the chart
  const chartData = data.map((item, index) => ({
    ...item,
    color: CHART_COLORS[index % CHART_COLORS.length],
  }));

  const renderTooltip = (props: any) => {
    if (!props.active || !props.payload) {
      return null;
    }
    const data = props.payload[0];
    return (
      <div className="rounded-lg border bg-background p-2 shadow-sm">
        <p className="text-sm font-medium">{data.payload.category}</p>
        <p className="text-sm text-muted-foreground">
          {data.payload.count} part{data.payload.count !== 1 ? 's' : ''}
        </p>
      </div>
    );
  };

  const renderLabel = (entry: CategoryChartData) => {
    const total = chartData.reduce((sum, item) => sum + item.count, 0);
    const percentage = total > 0 ? Math.round((entry.count / total) * 100) : 0;
    return percentage > 5 ? `${percentage}%` : '';
  };

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>Parts by Category</CardTitle>
        <CardDescription>Distribution of parts across all categories</CardDescription>
      </CardHeader>
      <CardContent>
        {chartData.length > 0 ? (
          <ResponsiveContainer width="100%" height={300}>
            <RechartsPieChart>
              <Pie
                data={chartData}
                cx="50%"
                cy="50%"
                labelLine={false}
                label={renderLabel}
                outerRadius={100}
                dataKey="count"
                nameKey="category"
              >
                {chartData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip content={renderTooltip} />
              <Legend
                verticalAlign="bottom"
                height={36}
                iconType="circle"
                formatter={(value: string, entry: any) => (
                  <span className="text-sm">
                    {value} ({entry.payload.count})
                  </span>
                )}
              />
            </RechartsPieChart>
          </ResponsiveContainer>
        ) : (
          <div className="flex h-[300px] items-center justify-center text-muted-foreground">
            No data available
          </div>
        )}
      </CardContent>
    </Card>
  );
}
