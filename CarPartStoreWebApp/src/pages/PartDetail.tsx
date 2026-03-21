/**
 * Part Detail Page - Portfolio Style
 * Enhanced detailed view with warm colors and spacious layout
 */

import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePart } from '@/hooks/useParts';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Separator } from '@/components/ui/separator';
import { ImageGallery } from '@/components/part-detail/image-gallery';
import PlaceholderImage from '@/components/common/PlaceholderImage';
import SEO from '@/components/seo/SEO';
import {
  ArrowLeft,
  Package,
  Hash,
  MapPin,
  Car,
  Mail,
} from 'lucide-react';

export default function PartDetail() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const partId = parseInt(id || '0');

  const { data: part, isLoading, error } = usePart(partId);

  // Loading state
  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Skeleton className="h-12 w-64 mb-8" />
        <div className="grid gap-8 lg:grid-cols-2">
          <Skeleton className="aspect-square rounded-lg" />
          <div className="space-y-6">
            <Skeleton className="h-16 w-3/4" />
            <Skeleton className="h-8 w-1/2" />
            <div className="space-y-4">
              {[...Array(6)].map((_, i) => (
                <Skeleton key={i} className="h-24 w-full" />
              ))}
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Error state
  if (error || !part) {
    return (
      <div className="container mx-auto px-4 py-16">
        <Card className="border-destructive max-w-md mx-auto">
          <CardContent className="flex flex-col items-center justify-center py-16">
            <div className="text-6xl mb-4">🔍</div>
            <h3 className="text-2xl font-bold text-foreground mb-2">{t('partDetail.notFound')}</h3>
            <p className="text-muted-foreground mb-6 text-center">
              {(error as Error)?.message || t('partDetail.notFoundMessage')}
            </p>
            <Link to="/parts">
              <Button className="bg-primary hover:bg-primary/90">
                <ArrowLeft className="mr-2 h-4 w-4" />
                {t('partDetail.backToParts')}
              </Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Get all images from imagePath (space-separated URLs)
  const images = part.imagePath ? part.imagePath.split(' ').filter(Boolean) : [];

  // Stock status
  const getStockStatus = (quantity: number): 'success' | 'warning' | 'destructive' => {
    if (quantity === 0) return 'destructive';
    if (quantity < 5) return 'warning';
    return 'success';
  };

  const getStockLabel = (quantity: number): string => {
    if (quantity === 0) return t('common.outOfStock');
    if (quantity < 5) return t('common.lowStock');
    return t('common.inStock');
  };

  // Format currency
  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'TRY',
    }).format(amount);
  };

  // Build title: Brand + Model + Name + ReleaseDate
  const titleParts = [
    part.brand,
    part.model,
    part.name,
    part.releaseYear ? `(${part.releaseYear})` : ''
  ].filter(Boolean);

  const title = titleParts.join(' ');

  // Build SEO title with priority: PartNumber (SKU) > Brand > Year > Name
  const seoTitleParts: string[] = [];
  if (part.partNumber) {
    seoTitleParts.push(`SKU: ${part.partNumber}`);
  }
  if (part.brand) {
    seoTitleParts.push(part.brand);
  }
  if (part.releaseYear) {
    seoTitleParts.push(part.releaseYear.toString());
  }
  if (part.name) {
    seoTitleParts.push(part.name);
  }
  const seoTitle = seoTitleParts.join(' | ');

  // Get first image for SEO
  const seoImage = images.length > 0 ? images[0] : undefined;

  // Build SEO description
  const seoDescription = part.description
    ? part.description.slice(0, 160)
    : `${part.name} - ${part.brand || 'Unknown'} ${part.model || ''} ${part.releaseYear || ''}`.trim();

  return (
    <div className="min-h-screen bg-background">
      {/* Dynamic SEO for Part Detail */}
      <SEO
        title={seoTitle}
        description={seoDescription}
        image={seoImage}
        url={`/parts/${part.id}`}
        type="product"
      />
      <div className="container mx-auto px-4 py-8">
        {/* Back Button */}
        <div className="mb-6">
          <Link to="/parts">
            <Button variant="ghost" className="gap-2">
              <ArrowLeft className="h-4 w-4" />
              {t('partDetail.backToParts')}
            </Button>
          </Link>
        </div>

        {/* Main Content Grid */}
        <div className="grid gap-8 lg:grid-cols-2 mb-12">
          {/* Image Gallery - Left */}
          <div className="lg:sticky lg:top-8 lg:self-start space-y-4">
            {images.length > 0 ? (
              <ImageGallery images={images} alt={title} />
            ) : (
              <div className="aspect-square rounded-lg overflow-hidden bg-muted border">
                <PlaceholderImage className="w-full h-full" />
              </div>
            )}
          </div>

          {/* Part Information - Right */}
          <div className="space-y-6">
            {/* Title and Price */}
            <div>
              <h1 className="text-4xl font-bold text-foreground mb-4">{title}</h1>
              <div className="flex items-center gap-4 mb-4">
                <p className="text-5xl font-bold text-primary">
                  {formatCurrency(part.costPrice)}
                </p>
                <Badge variant={getStockStatus(part.stockQuantity)} className="text-sm px-3 py-1">
                  {getStockLabel(part.stockQuantity)}
                </Badge>
              </div>
              {part.categoryName && (
                <div className="flex items-center gap-2">
                  <Package className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">{part.categoryName}</span>
                </div>
              )}
            </div>

            <Separator />

            {/* Description */}
            {part.description && (
              <div>
                <h2 className="text-xl font-semibold text-foreground mb-3">{t('partDetail.description')}</h2>
                <p className="text-muted-foreground leading-relaxed">{part.description}</p>
              </div>
            )}

            {/* Quick Info Grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {/* Part Number */}
              {part.partNumber && (
                <Card className="border bg-muted/50">
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <Hash className="h-5 w-5 text-primary" />
                      <div>
                        <p className="text-xs text-muted-foreground">{t('partDetail.partNumber')}</p>
                        <p className="font-semibold text-foreground font-mono">{part.partNumber}</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}

              {/* Stock */}
              <Card className="border bg-muted/50">
                <CardContent className="p-4">
                  <div className="flex items-center gap-3">
                    <Package className="h-5 w-5 text-primary" />
                    <div>
                      <p className="text-xs text-muted-foreground">{t('partDetail.stock')}</p>
                      <p className="font-semibold text-foreground">{part.stockQuantity} {t('common.units')}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Location */}
              {part.location && (
                <Card className="border bg-muted/50">
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <MapPin className="h-5 w-5 text-primary" />
                      <div>
                        <p className="text-xs text-muted-foreground">{t('partDetail.location')}</p>
                        <p className="font-semibold text-foreground">{part.location}</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}

              {/* Vehicle Info */}
              {(part.brand || part.model || part.releaseYear) && (
                <Card className="border bg-muted/50">
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <Car className="h-5 w-5 text-primary" />
                      <div>
                        <p className="text-xs text-muted-foreground">{t('partDetail.vehicle')}</p>
                        <p className="font-semibold text-foreground">
                          {[part.brand, part.model, part.releaseYear].filter(Boolean).join(' ')}
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}
            </div>

            <Separator />

            {/* Contact Button */}
            <Link to="/contact" className="block">
              <Button
                size="lg"
                className="w-full bg-primary hover:bg-primary/90 text-primary-foreground text-lg py-6"
              >
                <Mail className="mr-2 h-5 w-5" />
                {t('partDetail.contactAboutPart')}
              </Button>
            </Link>

            {/* Metadata */}
            <div className="text-xs text-muted-foreground space-y-1 pt-4">
              <p>{t('common.added')}: {new Date(part.createdDate).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
              })}</p>
              {part.lastUpdated && (
                <p>{t('common.lastUpdated')}: {new Date(part.lastUpdated).toLocaleDateString('en-US', {
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric',
                })}</p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
