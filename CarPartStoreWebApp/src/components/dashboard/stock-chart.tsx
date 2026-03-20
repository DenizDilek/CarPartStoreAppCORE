/**
 * Stock Status Chart Component
 * Displays a bar chart showing the distribution of stock status
 */

import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';

const STATUS_COLORS: Record<string, string> = {
  'In Stock': '#48BB78', // success
  'Low Stock': '#ECC94B', // warning
  'Out of Stock': '#F56565', // destructive
};

const STATUS_ORDER = ['In Stock', 'Low Stock', 'Out of Stock'];

export interface StockChartData {
  status: string;
  count: number;
}

interface StockChartProps {
  data: StockChartData[];
  isLoading?: boolean;
  className?: string;
}

export function StockChart({ data, isLoading = false, className }: StockChartProps) {
  if (isLoading) {
    return (
      <Card className={className}>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-[300px] w-full" />
        </CardContent>
      </Card>
    );
  }

  // Sort data by status order and add colors
  const chartData = STATUS_ORDER.map((status) => {
    const found = data.find((item) => item.status === status);
    return {
      status,
      count: found?.count || 0,
      color: STATUS_COLORS[status],
    };
  }).filter((item) => item.count > 0);

  const renderTooltip = (props: any) => {
    if (!props.active || !props.payload) {
      return null;
    }
    const data = props.payload[0];
    return (
      <div className="rounded-lg border bg-background p-2 shadow-sm">
        <p className="text-sm font-medium">{data.payload.status}</p>
        <p className="text-sm text-muted-foreground">
          {data.payload.count} part{data.payload.count !== 1 ? 's' : ''}
        </p>
      </div>
    );
  };

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>Stock Status Distribution</CardTitle>
        <CardDescription>Overview of inventory stock levels</CardDescription>
      </CardHeader>
      <CardContent>
        {chartData.length > 0 ? (
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis
                dataKey="status"
                tick={{ fill: 'hsl(var(--muted-foreground))' }}
                tickLine={false}
              />
              <YAxis
                tick={{ fill: 'hsl(var(--muted-foreground))' }}
                tickLine={false}
                axisLine={false}
              />
              <Tooltip content={renderTooltip} />
              <Bar dataKey="count" radius={[8, 8, 0, 0]}>
                {chartData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Bar>
            </BarChart>
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
