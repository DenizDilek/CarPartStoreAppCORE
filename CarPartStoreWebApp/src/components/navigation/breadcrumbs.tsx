/**
 * Breadcrumbs Component
 * Displays navigation breadcrumbs for current page
 */

import { Link, useLocation } from 'react-router-dom'
import { ChevronRight, Home } from 'lucide-react'
import { cn } from '@/lib/utils'

interface Breadcrumb {
  label: string
  href?: string
}

interface BreadcrumbsProps {
  className?: string
  items?: Breadcrumb[]
  homeLabel?: string
}

/**
 * Generate breadcrumbs from current location
 */
function generateBreadcrumbs(pathname: string, homeLabel = 'Dashboard'): Breadcrumb[] {
  const breadcrumbs: Breadcrumb[] = [
    { label: homeLabel, href: '/' },
  ]

  if (pathname === '/') {
    return [{ label: homeLabel }]
  }

  const segments = pathname.split('/').filter(Boolean)

  // Build up the breadcrumb path
  let currentPath = ''

  for (let i = 0; i < segments.length; i++) {
    const segment = segments[i]
    currentPath += `/${segment}`

    // Format the segment label
    let label = segment
      .split('-')
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ')

    // Handle special cases
    if (segment === 'parts') {
      label = 'Parts'
    } else if (segment === 'categories') {
      label = 'Categories'
    } else if (segment === 'dashboard') {
      label = 'Dashboard'
    } else if (/^\d+$/.test(segment)) {
      // It's a numeric ID
      if (segments[i - 1] === 'parts') {
        label = 'Part Details'
      } else if (segments[i - 1] === 'category') {
        label = 'Category Parts'
      } else {
        label = `#${segment}`
      }
    }

    // Only add href if it's not the last segment (current page)
    const isLast = i === segments.length - 1
    breadcrumbs.push({
      label,
      href: isLast ? undefined : currentPath,
    })
  }

  return breadcrumbs
}

/**
 * Breadcrumbs Component
 */
export function Breadcrumbs({
  className,
  items,
  homeLabel = 'Dashboard',
}: BreadcrumbsProps) {
  const location = useLocation()
  const breadcrumbs = items || generateBreadcrumbs(location.pathname, homeLabel)

  if (breadcrumbs.length <= 1) {
    return null
  }

  return (
    <nav className={cn('flex items-center text-sm text-muted-foreground', className)}>
      <ol className="flex items-center gap-1">
        {breadcrumbs.map((breadcrumb, index) => {
          return (
            <li key={index} className="flex items-center gap-1">
              {index > 0 && (
                <ChevronRight className="h-4 w-4" />
              )}

              {breadcrumb.href ? (
                <Link
                  to={breadcrumb.href}
                  className="hover:text-foreground transition-colors"
                >
                  {index === 0 && <Home className="h-4 w-4 inline mr-1" />}
                  {breadcrumb.label}
                </Link>
              ) : (
                <span className="text-foreground font-medium">
                  {index === 0 && <Home className="h-4 w-4 inline mr-1" />}
                  {breadcrumb.label}
                </span>
              )}
            </li>
          )
        })}
      </ol>
    </nav>
  )
}
