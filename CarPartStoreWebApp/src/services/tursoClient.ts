/**
 * Turso Database Client
 * Direct connection to Turso database using @libsql/client SDK
 */

import { createClient } from '@libsql/client';

export interface TursoConfig {
  url: string;
  authToken: string;
  concurrency?: number;
}

/**
 * Turso Client - Singleton pattern for database connections
 */
class TursoClient {
  private client: ReturnType<typeof createClient> | null = null;

  /**
   * Initialize the Turso client with configuration
   */
  initialize(config: TursoConfig): void {
    if (this.client) {
      console.warn('TursoClient already initialized');
      return;
    }

    this.client = createClient({
      url: config.url,
      authToken: config.authToken,
      concurrency: config.concurrency || 20,
    });

    console.log('TursoClient initialized with:', {
      url: config.url.replace(/\/[^/]*@/, '/***@'), // Hide auth in logs
      concurrency: config.concurrency || 20,
    });
  }

  /**
   * Ensure client is initialized
   */
  private ensureInitialized(): void {
    if (!this.client) {
      throw new Error(
        'TursoClient not initialized. Call initialize() with proper config first.'
      );
    }
  }

  /**
   * Execute a SQL query with optional parameters
   */
  async execute<T = any>(sql: string, args: any[] = []): Promise<T[]> {
    this.ensureInitialized();

    try {
      const result = await this.client!.execute(sql, args);
      return result.rows as T[];
    } catch (error) {
      console.error('Turso query error:', error);
      throw error;
    }
  }

  /**
   * Execute a batch of SQL statements in a transaction
   */
  async batch(statements: { sql: string; args?: any[] }[]): Promise<void> {
    this.ensureInitialized();

    try {
      await this.client!.batch(statements);
    } catch (error) {
      console.error('Turso batch error:', error);
      throw error;
    }
  }

  /**
   * Check if client is initialized
   */
  isInitialized(): boolean {
    return this.client !== null;
  }

  /**
   * Close the client connection
   */
  close(): void {
    if (this.client) {
      this.client = null;
      console.log('TursoClient closed');
    }
  }
}

// Export singleton instance
const tursoClient = new TursoClient();

/**
 * Initialize Turso client from environment variables
 */
export function initializeTursoClient(): boolean {
  const databaseUrl = import.meta.env.VITE_TURSO_DATABASE_URL;
  const authToken = import.meta.env.VITE_TURSO_AUTH_TOKEN;

  if (!databaseUrl || !authToken) {
    console.warn(
      'Turso configuration missing. Set VITE_TURSO_DATABASE_URL and VITE_TURSO_AUTH_TOKEN in .env'
    );
    return false;
  }

  tursoClient.initialize({
    url: databaseUrl,
    authToken: authToken,
  });

  return true;
}

/**
 * Get the Turso client instance
 */
export function getTursoClient(): TursoClient {
  return tursoClient;
}

export default tursoClient;
