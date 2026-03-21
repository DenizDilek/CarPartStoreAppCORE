import { Search } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

export default function MainHeader() {
  const { t, i18n } = useTranslation();
  const [searchQuery, setSearchQuery] = useState('');
  const navigate = useNavigate();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/parts?search=${encodeURIComponent(searchQuery.trim())}`);
    }
  };

  const changeLanguage = (lang: string) => {
    i18n.changeLanguage(lang);
  };

  const currentLanguage = i18n.language;

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        {/* Logo - Left */}
        <div className="flex items-center gap-2">
          <a href="/" className="flex items-center gap-2 text-xl font-bold text-primary hover:text-primary/90 transition-colors">
            <span className="text-2xl">🚗</span>
            <span className="hidden sm:inline">{t('header.title')}</span>
          </a>
        </div>

        {/* Search Bar - Center */}
        <form onSubmit={handleSearch} className="flex-1 max-w-md mx-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <input
              type="search"
              placeholder={t('header.searchPlaceholder')}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full rounded-full border bg-background pl-10 pr-4 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
            />
          </div>
        </form>

        {/* Contact Link & Language Switcher - Right */}
        <div className="flex items-center gap-4">
          <a
            href="/contact"
            className="text-sm font-medium text-foreground hover:text-primary transition-colors"
          >
            {t('header.contact')}
          </a>

          {/* Language Switcher */}
          <div className="flex items-center gap-1 text-sm border-l pl-4">
            <button
              onClick={() => changeLanguage('en')}
              className={`transition-colors hover:text-primary ${
                currentLanguage === 'en'
                  ? 'font-bold text-primary'
                  : 'text-muted-foreground'
              }`}
            >
              ENG
            </button>
            <span className="text-muted-foreground">|</span>
            <button
              onClick={() => changeLanguage('tr')}
              className={`transition-colors hover:text-primary ${
                currentLanguage === 'tr'
                  ? 'font-bold text-primary'
                  : 'text-muted-foreground'
              }`}
            >
              TR
            </button>
          </div>
        </div>
      </div>
    </header>
  );
}
