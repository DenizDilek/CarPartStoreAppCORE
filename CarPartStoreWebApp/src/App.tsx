/**
 * Main App Component
 * Portfolio-style car parts showcase website
 * Sets up routing, theme provider, and provides React Query context
 */

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@/components/theme-provider';
import { Toaster } from '@/components/ui/toaster';
import { ThemeToggle } from '@/components/theme-toggle';
import MainHeader from '@/components/layout/MainHeader';
import MainNav from '@/components/layout/MainNav';
import MainFooter from '@/components/layout/MainFooter';
import AboutPage from './pages/AboutPage';
import PartsList from './pages/PartsList';
import PartDetail from './pages/PartDetail';
import ContactPage from './pages/ContactPage';

/**
 * Configure React Query client
 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (failureCount, error: any) => {
        // Don't retry on 404 (not found) or 4xx errors
        if (error?.response?.status >= 400 && error?.response?.status < 500) {
          return false;
        }
        // Retry up to 3 times for other errors
        return failureCount < 3;
      },
      refetchOnWindowFocus: true, // Refetch when window regains focus
    },
    mutations: {
      retry: 1, // Retry mutations once on failure
    },
  },
});

/**
 * Main App Component
 */
function App() {
  return (
    <ThemeProvider
      attribute="class"
      defaultTheme="light"
      enableSystem={false}
      disableTransitionOnChange
      forcedTheme="light"
    >
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <div className="min-h-screen bg-background font-sans flex flex-col">
            {/* Header - Logo + Search Bar */}
            <MainHeader />

            {/* Navigation Bar - About/Parts/Contact */}
            <MainNav />

            {/* Main Content */}
            <main className="flex-1">
              <Routes>
                <Route path="/" element={<PartsList />} />
                <Route path="/about" element={<AboutPage />} />
                <Route path="/parts" element={<PartsList />} />
                <Route path="/parts/:id" element={<PartDetail />} />
                <Route path="/contact" element={<ContactPage />} />
                <Route path="/category/:categoryId" element={<PartsList />} />
                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
            </main>

            {/* Footer */}
            <MainFooter />

            {/* Theme Toggle Button (Fixed Position) */}
            <div className="fixed bottom-4 right-4 z-50">
              <ThemeToggle />
            </div>

            {/* Toaster for notifications */}
            <Toaster />
          </div>
        </BrowserRouter>
      </QueryClientProvider>
    </ThemeProvider>
  );
}

export default App;
