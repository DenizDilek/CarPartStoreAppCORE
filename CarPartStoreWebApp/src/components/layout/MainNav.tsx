import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

export default function MainNav() {
  const { t } = useTranslation();

  const navItems = [
    { path: '/about', labelKey: 'nav.about' },
    { path: '/parts', labelKey: 'nav.parts' },
    { path: '/contact', labelKey: 'nav.contact' },
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
              {t(item.labelKey)}
            </NavLink>
          ))}
        </div>
      </div>
    </nav>
  );
}
