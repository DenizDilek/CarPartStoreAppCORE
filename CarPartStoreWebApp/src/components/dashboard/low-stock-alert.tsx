/**
 * Low Stock Alert Component
 * Displays parts with low stock levels (stock < 5)
 */

import { Link } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { AlertTriangle, Package } from 'lucide-react';
import type { CarPartDto } from '@/types';
import { cn } from '@/lib/utils';

interface LowStockAlertProps {
  parts: CarPartDto[];
  isLoading?: boolean;
  className?: string;
  maxDisplay?: number;
}

export function LowStockAlert({
  parts,
  isLoading = false,
  className,
  maxDisplay = 5,
}: LowStockAlertProps) {
  const displayParts = parts.slice(0, maxDisplay);
  const hasOutOfStock = parts.some((p) => p.stockQuantity === 0);
  const lowStockCount = parts.filter((p) => p.stockQuantity > 0 && p.stockQuantity < 5).length;
  const outOfStockCount = parts.filter((p) => p.stockQuantity === 0).length;

  if (isLoading) {
    return (
      <Card className={cn('border-warning/50', className)}>
        <CardHeader>
          <Skeleton className="h-6 w-40" />
          <Skeleton className="h-4 w-56" />
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {[...Array(3)].map((_, i) => (
              <Skeleton key={i} className="h-16 w-full" />
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={cn(hasOutOfStock ? 'border-destructive/50' : 'border-warning/50', className)}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <AlertTriangle className={cn('h-5 w-5', hasOutOfStock ? 'text-destructive' : 'text-warning')} />
          Low Stock Alerts
        </CardTitle>
        <CardDescription>
          {outOfStockCount > 0 && `${outOfStockCount} out of stock. `}
          {lowStockCount} part{lowStockCount !== 1 ? 's' : ''} running low
        </CardDescription>
      </CardHeader>
      <CardContent>
        {displayParts.length > 0 ? (
          <div className="space-y-3">
            {displayParts.map((part) => (
              <Link
                key={part.id}
                to={`/parts/${part.id}`}
                className="block rounded-lg border p-3 transition-colors hover:bg-accent/50"
              >
                <div className="flex items-start justify-between gap-4">
                  <div className="flex items-start gap-3 min-w-0 flex-1">
                    {part.imagePath ? (
                      <img
                        src={part.imagePath.split(' ')[0]}
                        alt={part.name}
                        className="h-12 w-12 flex-shrink-0 rounded-md object-cover bg-muted"
                      />
                    ) : (
                      <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-md bg-muted">
                        <Package className="h-6 w-6 text-muted-foreground" />
                      </div>
                    )}
                    <div className="min-w-0 flex-1">
                      <p className="font-medium truncate">{part.name}</p>
                      {part.partNumber && (
                        <p className="text-xs text-muted-foreground font-mono">
                          {part.partNumber}
                        </p>
                      )}
                      <div className="flex flex-wrap gap-1 mt-1">
                        <Badge variant="outline" className="text-xs">
                          {part.categoryName || '-'}
                        </Badge>
                        {part.brand && (
                          <Badge variant="outline" className="text-xs">
                            {part.brand}
                          </Badge>
                        )}
                      </div>
                    </div>
                  </div>
                  <div className="flex-shrink-0 text-right">
                    <Badge
                      variant={part.stockQuantity === 0 ? 'destructive' : 'warning'}
                      className="text-sm"
                    >
                      {part.stockQuantity} left
                    </Badge>
                    <p className="text-xs text-muted-foreground mt-1">
                      {part.location || 'No location'}
                    </p>
                  </div>
                </div>
              </Link>
            ))}
            {parts.length > maxDisplay && (
              <Link
                to="/parts?stockStatus=low-stock"
                className="block text-center text-sm text-muted-foreground hover:text-foreground py-2"
              >
                View all {parts.length} low stock parts
              </Link>
            )}
          </div>
        ) : (
          <div className="flex h-[150px] flex-col items-center justify-center text-center">
            <Package className="h-12 w-12 text-muted-foreground/50" />
            <p className="mt-2 text-sm font-medium">All stock levels are healthy</p>
            <p className="text-xs text-muted-foreground">No parts running low on inventory</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
