/**
 * Main App Component
 * Sets up routing and provides React Query context
 */

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import './App.css';
import PartsList from './pages/PartsList';

/**
 * Configure React Query client
 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (failureCount, error) => {
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
 * Navigation component
 */
function Navigation() {
  return (
    <nav className="navbar">
      <div className="nav-brand">
        <Link to="/" className="nav-link">
          🚗 Car Parts Storage
        </Link>
      </div>
      <div className="nav-links">
        <Link to="/" className="nav-link">
          Parts Inventory
        </Link>
        <a
          href="http://localhost:5000/swagger"
          target="_blank"
          rel="noopener noreferrer"
          className="nav-link"
        >
          API Docs
        </a>
      </div>
    </nav>
  );
}

/**
 * Footer component
 */
function Footer() {
  return (
    <footer className="footer">
      <p>
        Car Parts Storage Web App - Connected to WPF Application
      </p>
      <p className="footer-links">
        <span className="status-indicator">● API Status:</span>{' '}
        <span className="status-text">Running</span>
      </p>
    </footer>
  );
}

/**
 * Main App Component
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <div className="app">
          <Navigation />
          <main className="main-content">
            <Routes>
              <Route path="/" element={<PartsList />} />
              <Route
                path="/category/:categoryId"
                element={<PartsList />}
              />
            </Routes>
          </main>
          <Footer />
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
