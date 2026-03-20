/**
 * Image Gallery Component
 * Main image display with thumbnail navigation for PartDetail page
 */

import { useState, useCallback } from 'react';
import {
  ChevronLeft,
  ChevronRight,
  ZoomIn,
  Image as ImageIcon,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { ImageThumbnails } from './image-thumbnails';
import { LightboxView } from './lightbox-view';

interface ImageGalleryProps {
  images: string[];
  alt: string;
  className?: string;
}

export function ImageGallery({ images, alt, className }: ImageGalleryProps) {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isLightboxOpen, setIsLightboxOpen] = useState(false);

  const hasImages = images && images.length > 0;
  const currentImage = hasImages ? images[currentIndex] : null;

  const handlePrevious = useCallback(() => {
    setCurrentIndex((prev) => (prev > 0 ? prev - 1 : images.length - 1));
  }, [images.length]);

  const handleNext = useCallback(() => {
    setCurrentIndex((prev) => (prev < images.length - 1 ? prev + 1 : 0));
  }, [images.length]);

  const handleThumbnailClick = useCallback((index: number) => {
    setCurrentIndex(index);
  }, []);

  const handleLightboxOpen = useCallback(() => {
    setIsLightboxOpen(true);
  }, []);

  const handleLightboxClose = useCallback(() => {
    setIsLightboxOpen(false);
  }, []);

  // Keyboard navigation
  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === 'ArrowLeft') {
        handlePrevious();
      } else if (e.key === 'ArrowRight') {
        handleNext();
      } else if (e.key === 'Enter' || e.key === ' ') {
        handleLightboxOpen();
      }
    },
    [handlePrevious, handleNext, handleLightboxOpen]
  );

  if (!hasImages) {
    return (
      <div
        className={cn(
          'flex aspect-square w-full items-center justify-center rounded-lg bg-muted',
          className
        )}
      >
        <div className="flex flex-col items-center gap-2 text-muted-foreground">
          <ImageIcon className="h-12 w-12" />
          <span className="text-sm">No image available</span>
        </div>
      </div>
    );
  }

  return (
    <div className={cn('space-y-4', className)}>
      {/* Main image */}
      <div
        className="relative aspect-square w-full overflow-hidden rounded-lg bg-muted group"
        onKeyDown={handleKeyDown}
        tabIndex={0}
        role="img"
        aria-label={`${alt}, image ${currentIndex + 1} of ${images.length}`}
      >
        <img
          src={currentImage}
          alt={`${alt} - View ${currentIndex + 1}`}
          className="h-full w-full object-contain transition-transform duration-300 group-hover:scale-105"
        />

        {/* Navigation arrows - show on hover */}
        {images.length > 1 && (
          <>
            <Button
              variant="secondary"
              size="icon"
              className="absolute left-2 top-1/2 -translate-y-1/2 opacity-0 transition-opacity group-hover:opacity-100"
              onClick={handlePrevious}
              aria-label="Previous image"
            >
              <ChevronLeft className="h-5 w-5" />
            </Button>
            <Button
              variant="secondary"
              size="icon"
              className="absolute right-2 top-1/2 -translate-y-1/2 opacity-0 transition-opacity group-hover:opacity-100"
              onClick={handleNext}
              aria-label="Next image"
            >
              <ChevronRight className="h-5 w-5" />
            </Button>
          </>
        )}

        {/* Lightbox button */}
        <Button
          variant="secondary"
          size="icon"
          className="absolute right-2 top-2 opacity-0 transition-opacity group-hover:opacity-100"
          onClick={handleLightboxOpen}
          aria-label="Open lightbox"
        >
          <ZoomIn className="h-5 w-5" />
        </Button>

        {/* Image counter */}
        {images.length > 1 && (
          <div className="absolute bottom-2 left-1/2 -translate-x-1/2 rounded-md bg-black/60 px-3 py-1 text-xs text-white">
            {currentIndex + 1} / {images.length}
          </div>
        )}
      </div>

      {/* Thumbnails */}
      {images.length > 1 && (
        <ImageThumbnails
          images={images}
          currentIndex={currentIndex}
          onThumbnailClick={handleThumbnailClick}
        />
      )}

      {/* Lightbox */}
      <LightboxView
        images={images}
        initialIndex={currentIndex}
        isOpen={isLightboxOpen}
        onClose={handleLightboxClose}
        alt={alt}
      />
    </div>
  );
}
