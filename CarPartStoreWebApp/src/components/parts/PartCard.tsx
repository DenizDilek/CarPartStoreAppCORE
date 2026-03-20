import { Link } from 'react-router-dom';
import PlaceholderImage from '@/components/common/PlaceholderImage';
import { CarPartDto } from '@/types';

interface PartCardProps {
  part: CarPartDto;
}

export default function PartCard({ part }: PartCardProps) {
  // Get first image from imagePath (space-separated URLs)
  const firstImage = part.imagePath
    ? part.imagePath.split(' ')[0]
    : null;

  // Format price - handle undefined/null/NaN
  const price = part.costPrice || 0;
  const formattedPrice = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(price);

  // Build title: Brand + Model + Name + ReleaseDate
  const titleParts = [
    part.brand,
    part.model,
    part.name,
    part.releaseYear ? `(${part.releaseYear})` : ''
  ].filter(Boolean);

  const title = titleParts.join(' ') || part.name || 'Unnamed Part';

  return (
    <Link
      to={`/parts/${part.id}`}
      className="group block bg-white rounded-lg border border-gray-200 overflow-hidden hover:shadow-lg hover:-translate-y-1 transition-all duration-200"
      style={{ display: 'block', textDecoration: 'none', color: 'inherit' }}
    >
      {/* Image */}
      <div style={{ aspectRatio: '4/3', overflow: 'hidden', backgroundColor: '#f3f4f6' }}>
        {firstImage ? (
          <img
            src={firstImage}
            alt={title}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
            style={{ width: '100%', height: '100%', objectFit: 'cover' }}
            onError={(e) => {
              // If image fails to load, show placeholder
              e.currentTarget.style.display = 'none';
            }}
          />
        ) : (
          <PlaceholderImage className="w-full h-full" />
        )}
      </div>

      {/* Content */}
      <div style={{ padding: '16px' }}>
        {/* Title */}
        <h3
          style={{
            fontSize: '16px',
            fontWeight: '600',
            color: '#212529',
            marginBottom: '8px',
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            overflow: 'hidden',
            minHeight: '48px'
          }}
          className="group-hover:text-primary"
        >
          {title}
        </h3>

        {/* Price */}
        <p
          style={{
            fontSize: '24px',
            fontWeight: 'bold',
            color: '#FF6B6B',
            marginBottom: '8px',
            margin: '0 0 8px 0'
          }}
        >
          {formattedPrice}
        </p>

        {/* Stock Status */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '8px' }}>
          <span
            style={{
              fontSize: '12px',
              padding: '4px 8px',
              borderRadius: '9999px',
              fontWeight: '500',
              backgroundColor: part.stockQuantity > 10
                ? 'rgba(25, 135, 84, 0.1)'
                : part.stockQuantity > 0
                ? 'rgba(255, 193, 7, 0.1)'
                : 'rgba(220, 53, 69, 0.1)',
              color: part.stockQuantity > 10
                ? '#198754'
                : part.stockQuantity > 0
                ? '#FFC107'
                : '#DC3545'
            }}
          >
            {part.stockQuantity > 10
              ? 'In Stock'
              : part.stockQuantity > 0
              ? `Low Stock (${part.stockQuantity})`
              : 'Out of Stock'}
          </span>

          {/* View Details Button - Visible on Hover */}
          <span
            style={{
              fontSize: '14px',
              color: '#FF6B6B',
              opacity: '0',
              transition: 'opacity 0.2s'
            }}
            className="group-hover:opacity-100"
          >
            View Details →
          </span>
        </div>

        {/* Category Badge */}
        {part.categoryName && (
          <div style={{ marginTop: '8px' }}>
            <span
              style={{
                fontSize: '12px',
                color: '#6C757D',
                backgroundColor: '#F8F9FA',
                padding: '4px 8px',
                borderRadius: '4px'
              }}
            >
              {part.categoryName}
            </span>
          </div>
        )}
      </div>
    </Link>
  );
}
