/**
 * Recent Parts Component
 * Displays a table of the most recently added parts
 */

import { Link } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Clock } from 'lucide-react';
import type { CarPartDto } from '@/types';

interface RecentPartsProps {
  parts: CarPartDto[];
  isLoading?: boolean;
  className?: string;
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 60) {
    return `${diffMins}m ago`;
  } else if (diffHours < 24) {
    return `${diffHours}h ago`;
  } else if (diffDays < 7) {
    return `${diffDays}d ago`;
  } else {
    return date.toLocaleDateString();
  }
}

function getStockStatus(quantity: number): 'default' | 'success' | 'warning' | 'destructive' {
  if (quantity === 0) return 'destructive';
  if (quantity < 5) return 'warning';
  return 'success';
}

function getStockLabel(quantity: number): string {
  if (quantity === 0) return 'Out';
  if (quantity < 5) return 'Low';
  return 'OK';
}

export function RecentParts({ parts, isLoading = false, className }: RecentPartsProps) {
  if (isLoading) {
    return (
      <Card className={className}>
        <CardHeader>
          <Skeleton className="h-6 w-36" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-12 w-full" />
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Clock className="h-5 w-5" />
          Recent Parts
        </CardTitle>
        <CardDescription>Last 10 parts added to inventory</CardDescription>
      </CardHeader>
      <CardContent>
        {parts.length > 0 ? (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Part</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Brand</TableHead>
                  <TableHead className="text-right">Stock</TableHead>
                  <TableHead className="text-right">Added</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {parts.map((part) => (
                  <TableRow key={part.id}>
                    <TableCell>
                      <Link
                        to={`/parts/${part.id}`}
                        className="font-medium hover:underline"
                      >
                        {part.name}
                      </Link>
                      {part.partNumber && (
                        <p className="text-xs text-muted-foreground font-mono">
                          {part.partNumber}
                        </p>
                      )}
                    </TableCell>
                    <TableCell>{part.categoryName || '-'}</TableCell>
                    <TableCell>{part.brand || '-'}</TableCell>
                    <TableCell className="text-right">
                      <Badge variant={getStockStatus(part.stockQuantity)} className="text-xs">
                        {part.stockQuantity} ({getStockLabel(part.stockQuantity)})
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right text-sm text-muted-foreground">
                      {formatDate(part.createdDate)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        ) : (
          <div className="flex h-[200px] items-center justify-center text-muted-foreground">
            No parts added yet
          </div>
        )}
      </CardContent>
    </Card>
  );
}
