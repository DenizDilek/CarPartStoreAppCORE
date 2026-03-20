/**
 * Mobile Bottom Navigation Component
 * Bottom navigation bar for mobile devices
 */

import { Link, useLocation } from 'react-router-dom'
import { LayoutDashboard, Package, FolderKanban } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useMediaQuery } from '@/hooks/useMediaQuery'

const navigation = [
  { name: 'Dashboard', href: '/', icon: LayoutDashboard },
  { name: 'Parts', href: '/parts', icon: Package },
  { name: 'Categories', href: '/categories', icon: FolderKanban },
]

/**
 * Mobile Bottom Navigation Component
 */
export function MobileNav() {
  const location = useLocation()
  const isMobile = useMediaQuery('(max-width: 639px)')

  if (!isMobile) {
    return null
  }

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 border-t bg-card md:hidden">
      <div className="grid h-16 grid-cols-3 items-center">
        {navigation.map((item) => {
          const isActive = location.pathname === item.href ||
            (item.href !== '/' && location.pathname.startsWith(item.href))
          const Icon = item.icon

          return (
            <Link
              key={item.name}
              to={item.href}
              className={cn(
                'flex flex-col items-center justify-center gap-1 text-xs font-medium transition-colors',
                isActive
                  ? 'text-primary'
                  : 'text-muted-foreground'
              )}
            >
              <Icon className={cn('h-5 w-5', isActive && 'fill-current')} />
              <span>{item.name}</span>
            </Link>
          )
        })}
      </div>
    </div>
  )
}
