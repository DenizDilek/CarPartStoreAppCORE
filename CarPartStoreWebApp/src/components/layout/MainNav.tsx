import { NavLink } from 'react-router-dom';

export default function MainNav() {
  const navItems = [
    { path: '/about', label: 'About' },
    { path: '/parts', label: 'Parts' },
    { path: '/contact', label: 'Contact' },
  ];

  return (
    <nav className="border-b bg-background">
      <div className="container mx-auto px-4">
        <div className="flex h-12 items-center justify-center gap-8">
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `text-sm font-medium transition-colors hover:text-primary ${
                  isActive ? 'text-primary' : 'text-muted-foreground'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </div>
      </div>
    </nav>
  );
}
