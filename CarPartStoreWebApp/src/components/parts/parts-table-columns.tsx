/**
 * Parts Table Columns
 * Column definitions for TanStack Table in the parts list (Read-Only)
 */

import { createColumnHelper } from '@tanstack/react-table';
import type { CarPartDto } from '@/types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Eye } from 'lucide-react';
import { Link } from 'react-router-dom';

const columnHelper = createColumnHelper<CarPartDto>();

export interface PartsTableColumnMeta {
  header: string;
  className?: string;
}

// Get stock status variant
const getStockStatus = (quantity: number): 'default' | 'success' | 'warning' | 'destructive' => {
  if (quantity === 0) return 'destructive';
  if (quantity < 5) return 'warning';
  return 'success';
};

// Get stock label
const getStockLabel = (quantity: number): string => {
  if (quantity === 0) return 'Out';
  if (quantity < 5) return 'Low';
  return 'OK';
};

// Format currency
const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
};

// Image column
export const imageColumn = columnHelper.accessor('imagePath', {
  id: 'image',
  header: 'Image',
  cell: (info) => {
    const imagePath = info.getValue();
    const firstImage = imagePath?.split(' ')[0];

    if (firstImage) {
      return (
        <img
          src={firstImage}
          alt=""
          className="h-10 w-10 rounded-md object-cover bg-muted"
        />
      );
    }

    return (
      <div className="flex h-10 w-10 items-center justify-center rounded-md bg-muted">
        <span className="text-xs text-muted-foreground">N/A</span>
      </div>
    );
  },
  enableSorting: false,
  meta: { header: 'Image', className: 'w-16' },
});

// Part number column
export const partNumberColumn = columnHelper.accessor('partNumber', {
  id: 'partNumber',
  header: 'Part Number',
  cell: (info) => (
    <span className="font-mono text-sm">
      {info.getValue() || '-'}
    </span>
  ),
  meta: { header: 'Part Number', className: 'w-28' },
});

// Name column
export const nameColumn = columnHelper.accessor('name', {
  id: 'name',
  header: 'Name',
  cell: (info) => {
    const part = info.row.original;
    return (
      <Link
        to={`/parts/${part.id}`}
        className="font-medium hover:text-primary hover:underline"
      >
        {info.getValue()}
      </Link>
    );
  },
  meta: { header: 'Name' },
});

// Category column
export const categoryColumn = columnHelper.accessor('categoryName', {
  id: 'category',
  header: 'Category',
  cell: (info) => info.getValue() || '-',
  meta: { header: 'Category', className: 'w-36' },
});

// Brand column
export const brandColumn = columnHelper.accessor('brand', {
  id: 'brand',
  header: 'Brand',
  cell: (info) => info.getValue() || '-',
  meta: { header: 'Brand', className: 'w-28' },
});

// Model column
export const modelColumn = columnHelper.accessor('model', {
  id: 'model',
  header: 'Model',
  cell: (info) => info.getValue() || '-',
  meta: { header: 'Model', className: 'w-28' },
});

// Year column
export const yearColumn = columnHelper.accessor('releaseYear', {
  id: 'year',
  header: 'Year',
  cell: (info) => info.getValue() || '-',
  meta: { header: 'Year', className: 'w-16' },
});

// Price column
export const priceColumn = columnHelper.accessor('costPrice', {
  id: 'price',
  header: 'Price',
  cell: (info) => (
    <span className="text-right">
      {formatCurrency(info.getValue() ?? 0)}
    </span>
  ),
  meta: { header: 'Price', className: 'w-24 text-right' },
});

// Stock column
export const stockColumn = columnHelper.accessor('stockQuantity', {
  id: 'stock',
  header: 'Stock',
  cell: (info) => {
    const quantity = info.getValue();
    return (
      <Badge variant={getStockStatus(quantity)} className="text-xs">
        {quantity} ({getStockLabel(quantity)})
      </Badge>
    );
  },
  meta: { header: 'Stock', className: 'w-28' },
});

// Location column
export const locationColumn = columnHelper.accessor('location', {
  id: 'location',
  header: 'Location',
  cell: (info) => info.getValue() || '-',
  meta: { header: 'Location', className: 'w-32' },
});

// Actions column (View only)
export const actionsColumn = () =>
  columnHelper.display({
    id: 'actions',
    header: 'Actions',
    cell: (info) => {
      const part = info.row.original;

      return (
        <Link to={`/parts/${part.id}`}>
          <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
            <Eye className="h-4 w-4" />
            <span className="sr-only">View details</span>
          </Button>
        </Link>
      );
    },
    enableSorting: false,
    meta: { header: 'Actions', className: 'w-16 text-center' },
  });

// Default columns for the table
export const getDefaultColumns = () => [
  imageColumn,
  partNumberColumn,
  nameColumn,
  categoryColumn,
  brandColumn,
  modelColumn,
  yearColumn,
  priceColumn,
  stockColumn,
  locationColumn,
  actionsColumn(),
];

// Responsive columns for smaller screens
export const getResponsiveColumns = (breakpoint: 'sm' | 'md' | 'lg') => {
  switch (breakpoint) {
    case 'sm':
      return [
        imageColumn,
        nameColumn,
        stockColumn,
        actionsColumn(),
      ];
    case 'md':
      return [
        imageColumn,
        nameColumn,
        categoryColumn,
        brandColumn,
        stockColumn,
        actionsColumn(),
      ];
    default:
      return getDefaultColumns();
  }
};
