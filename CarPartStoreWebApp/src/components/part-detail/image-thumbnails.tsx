/**
 * Image Thumbnails Component
 * Horizontal scrollable thumbnail strip for image gallery
 */

import { useCallback, useEffect } from 'react';
import useEmblaCarousel from 'embla-carousel-react';
import { cn } from '@/lib/utils';

interface ImageThumbnailsProps {
  images: string[];
  currentIndex: number;
  onThumbnailClick: (index: number) => void;
  className?: string;
}

export function ImageThumbnails({
  images,
  currentIndex,
  onThumbnailClick,
  className,
}: ImageThumbnailsProps) {
  const [emblaRef, emblaApi] = useEmblaCarousel({
    align: 'center',
    skipSnaps: false,
    dragFree: true,
  });

  // Scroll to current thumbnail when index changes
  useEffect(() => {
    if (emblaApi) {
      emblaApi.scrollTo(currentIndex);
    }
  }, [currentIndex, emblaApi]);

  const handleClick = useCallback(
    (index: number) => {
      onThumbnailClick(index);
    },
    [onThumbnailClick]
  );

  return (
    <div className={cn('relative', className)}>
      <div ref={emblaRef} className="overflow-hidden">
        <div className="flex gap-2 py-1">
          {images.map((image, index) => (
            <button
              key={index}
              onClick={() => handleClick(index)}
              className={cn(
                'relative flex-shrink-0 overflow-hidden rounded-md border-2 transition-all',
                'h-16 w-16 sm:h-20 sm:w-20',
                index === currentIndex
                  ? 'border-primary ring-2 ring-primary/20'
                  : 'border-transparent hover:border-muted-foreground/50'
              )}
              aria-label={`View image ${index + 1}`}
              aria-current={index === currentIndex ? 'true' : undefined}
            >
              <img
                src={image}
                alt={`Thumbnail ${index + 1}`}
                className="h-full w-full object-cover"
              />
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
