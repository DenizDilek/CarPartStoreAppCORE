/**
 * Sidebar Navigation Component
 * Desktop sidebar with logo and navigation links
 */

import { Link, useLocation } from 'react-router-dom'
import { LayoutDashboard, Package, FolderKanban, Menu } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { useMediaQuery } from '@/hooks/useMediaQuery'
import { Sheet, SheetContent, SheetTrigger } from '@/components/ui/sheet'

const navigation = [
  { name: 'Dashboard', href: '/', icon: LayoutDashboard },
  { name: 'Parts', href: '/parts', icon: Package },
  { name: 'Categories', href: '/categories', icon: FolderKanban },
]

interface SidebarProps {
  className?: string
}

/**
 * Sidebar navigation links component
 */
export function SidebarNav({ className }: SidebarProps) {
  const location = useLocation()

  return (
    <nav className={cn('space-y-1', className)}>
      {navigation.map((item) => {
        const isActive = location.pathname === item.href ||
          (item.href !== '/' && location.pathname.startsWith(item.href))
        const Icon = item.icon

        return (
          <Link
            key={item.name}
            to={item.href}
            className={cn(
              'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
              isActive
                ? 'bg-primary text-primary-foreground'
                : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
            )}
          >
            <Icon className="h-4 w-4" />
            {item.name}
          </Link>
        )
      })}
    </nav>
  )
}

/**
 * Desktop Sidebar Component
 */
export function Sidebar({ className }: SidebarProps) {
  const isDesktop = useMediaQuery('(min-width: 1024px)')

  if (!isDesktop) {
    return null
  }

  return (
    <aside
      className={cn(
        'fixed left-0 top-0 z-40 h-screen w-64 border-r bg-card',
        className
      )}
    >
      <div className="flex h-full flex-col">
        {/* Logo */}
        <div className="flex h-16 items-center border-b px-6">
          <Link to="/" className="flex items-center gap-2 font-semibold">
            <span className="text-2xl">🚗</span>
            <span className="text-lg">Car Parts Storage</span>
          </Link>
        </div>

        {/* Navigation */}
        <div className="flex-1 overflow-y-auto px-3 py-4">
          <SidebarNav />
        </div>

        {/* Footer */}
        <div className="border-t p-4">
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <span className="h-2 w-2 rounded-full bg-green-500" />
            <span>API Connected</span>
          </div>
        </div>
      </div>
    </aside>
  )
}

/**
 * Mobile Sidebar Sheet Component
 */
export function MobileSidebar() {
  const isDesktop = useMediaQuery('(min-width: 1024px)')

  if (isDesktop) {
    return null
  }

  return (
    <Sheet>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon" className="md:hidden">
          <Menu className="h-5 w-5" />
          <span className="sr-only">Toggle menu</span>
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="w-64 p-0">
        <div className="flex h-full flex-col">
          {/* Logo */}
          <div className="flex h-16 items-center border-b px-6">
            <Link to="/" className="flex items-center gap-2 font-semibold">
              <span className="text-2xl">🚗</span>
              <span className="text-lg">Car Parts Storage</span>
            </Link>
          </div>

          {/* Navigation */}
          <div className="flex-1 overflow-y-auto px-3 py-4">
            <SidebarNav />
          </div>

          {/* Footer */}
          <div className="border-t p-4">
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <span className="h-2 w-2 rounded-full bg-green-500" />
              <span>API Connected</span>
            </div>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  )
}
