// Turso configuration for React app

export interface TursoConfig {
  databaseUrl: string;
  authToken: string;
  apiUrl: string;
}

export function getTursoConfig(): TursoConfig {
  const databaseUrl = import.meta.env.VITE_TURSO_DATABASE_URL;
  const authToken = import.meta.env.VITE_TURSO_AUTH_TOKEN;
  const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';

  if (!databaseUrl || !authToken) {
    throw new Error(
      'Turso configuration is missing. Please set VITE_TURSO_DATABASE_URL and VITE_TURSO_AUTH_TOKEN in .env file'
    );
  }

  return {
    databaseUrl,
    authToken,
    apiUrl
  };
}

export function isTursoConfigured(): boolean {
  return !!import.meta.env.VITE_TURSO_DATABASE_URL && !!import.meta.env.VITE_TURSO_AUTH_TOKEN;
}

// Export default config instance
export const tursoConfig = getTursoConfig();
