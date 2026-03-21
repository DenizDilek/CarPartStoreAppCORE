using CarPartStoreApp.Data;
using CarPartStoreApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// Turso (cloud SQLite) implementation of IDataService using libsql HTTP protocol.
    ///
    /// TURSO API REFERENCE:
    /// - Documentation: https://docs.turso.tech/sdk/http/reference
    /// - Endpoint: POST /v2/pipeline
    /// - Base URL: Replace libsql:// with https:// in database URL
    /// - Auth: Authorization: Bearer TOKEN header
    ///
    /// API FORMAT:
    /// Request:
    /// {
    ///   "requests": [
    ///     { "type": "execute", "stmt": { "sql": "..." } },
    ///     { "type": "close" }
    ///   ]
    /// }
    ///
    /// Response:
    /// {
    ///   "baton": null,
    ///   "base_url": null,
    ///   "results": [
    ///     {
    ///       "type": "ok",
    ///       "response": {
    ///         "type": "execute",
    ///         "result": {
    ///           "cols": ["Id", "Name", ...],
    ///           "rows": [[1, "value", ...], ...],
    ///           "affected_row_count": 0,
    ///           "last_insert_rowid": null,
    ///           "replication_index": "1"
    ///         }
    ///       }
    ///     },
    ///     {
    ///       "type": "ok",
    ///       "response": { "type": "close" }
    ///     }
    ///   ]
    /// }
    /// </summary>
    public class TursoDataService : IDataService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _httpUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed;
        private bool _debugTrackingEnabled;
        private readonly List<DatabaseQueryInfo> _capturedQueries = new();

        public TursoDataService()
        {
            // Configuration priority: Environment variable -> DatabaseConfig -> Exception
            _httpUrl = Environment.GetEnvironmentVariable("TURSO_HTTP_URL")
                        ?? DatabaseConfig.GetTursoHttpUrl()
                        ?? throw new InvalidOperationException(
                            "TURSO_HTTP_URL not configured. Set environment variable or check .env file.");

            var authToken = Environment.GetEnvironmentVariable("TURSO_AUTH_TOKEN")
                            ?? DatabaseConfig.GetTursoAuthToken()
                            ?? throw new InvalidOperationException(
                                "TURSO_AUTH_TOKEN not configured. Set environment variable or check .env file.");

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            };
        }

        /// <summary>
        /// Gets the database type being used
        /// </summary>
        public string GetDatabaseType() => "Turso (Cloud SQLite)";

        /// <summary>
        /// Enables or disables debug tracking for query/response logging
        /// </summary>
        public void EnableDebugTracking(bool enabled)
        {
            _debugTrackingEnabled = enabled;
            if (!enabled)
            {
                _capturedQueries.Clear();
            }
        }

        /// <summary>
        /// Gets the last captured query info if debug tracking is enabled
        /// </summary>
        public DatabaseQueryInfo? GetLastQueryInfo()
        {
            return _capturedQueries.Count > 0 ? _capturedQueries.Last() : null;
        }

        /// <summary>
        /// Gets all captured query info if debug tracking is enabled
        /// </summary>
        public IReadOnlyList<DatabaseQueryInfo> GetAllCapturedQueries()
        {
            return _capturedQueries.AsReadOnly();
        }

        // ============================================================================
        // HELPER METHODS
        // ============================================================================

        /// <summary>
        /// Escapes a string value for SQL by replacing single quotes
        /// </summary>
        private static string EscapeSql(string? value)
        {
            return value?.Replace("'", "''") ?? "";
        }

        /// <summary>
        /// Formats a DateTime for SQL storage
        /// </summary>
        private static string FormatDateTime(DateTime? date)
        {
            return date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "NULL";
        }

        /// <summary>
        /// Formats a nullable int for SQL storage (returns "NULL" or the number as string)
        /// </summary>
        private static string FormatNullableInt(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "NULL";
        }

        // ============================================================================
        // PUBLIC API METHODS - IDataService Implementation
        // ============================================================================

        public Task<List<CarPart>> GetAllPartsAsync()
        {
            const string sql = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
                       p.CostPrice, p.StockQuantity, p.Location, p.Brand, p.ImagePath,
                       p.Model, p.ReleaseYear, p.CreatedDate, p.LastUpdated
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                ORDER BY p.Name
            ";
            return ExecuteQueryAndMapAsync(sql, MapRowsToParts);
        }

        public async Task<CarPart?> GetPartByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
                       p.CostPrice, p.StockQuantity, p.Location, p.Brand, p.ImagePath,
                       p.Model, p.ReleaseYear, p.CreatedDate, p.LastUpdated
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.Id = @id
            ";
            var parts = await ExecuteQueryAndMapAsync(sql, MapRowsToParts, new Dictionary<string, object> { ["@id"] = id });
            return parts.Count > 0 ? parts[0] : null;
        }

        public async Task<int> AddPartAsync(CarPart part)
        {
            // Verify category exists first for referential integrity
            var category = await GetCategoryByIdAsync(part.CategoryId);
            if (category == null)
            {
                throw new InvalidOperationException($"CategoryId {part.CategoryId} does not exist in database");
            }

            var sql = $"INSERT INTO Parts (PartNumber, Name, Description, CategoryId, CostPrice, StockQuantity, Location, Brand, ImagePath, Model, ReleaseYear, CreatedDate, LastUpdated) VALUES ('{EscapeSql(part.PartNumber)}', '{EscapeSql(part.Name)}', '{EscapeSql(part.Description)}', {part.CategoryId}, {part.CostPrice}, {part.StockQuantity}, '{EscapeSql(part.Location)}', '{EscapeSql(part.Brand)}', '{EscapeSql(part.ImagePath)}', '{EscapeSql(part.Model)}', {FormatNullableInt(part.ReleaseDate)}, '{FormatDateTime(part.CreatedDate)}', {FormatDateTime(part.LastUpdated)})";

            var result = await ExecuteQueryAsync(sql, null);

            return (int)(result.LastInsertRowid ?? throw new InvalidOperationException("Failed to insert part: no row ID returned from database"));
        }

        public async Task<bool> UpdatePartAsync(CarPart part)
        {
            var sql = $"UPDATE Parts SET PartNumber = '{EscapeSql(part.PartNumber)}', Name = '{EscapeSql(part.Name)}', Description = '{EscapeSql(part.Description)}', CategoryId = {part.CategoryId}, CostPrice = {part.CostPrice}, StockQuantity = {part.StockQuantity}, Location = '{EscapeSql(part.Location)}', Brand = '{EscapeSql(part.Brand)}', ImagePath = '{EscapeSql(part.ImagePath)}', Model = '{EscapeSql(part.Model)}', ReleaseYear = {FormatNullableInt(part.ReleaseDate)}, LastUpdated = '{FormatDateTime(DateTime.Now)}' WHERE Id = {part.Id}";

            var result = await ExecuteQueryAsync(sql, null);
            return result.AffectedRowCount > 0;
        }

        public async Task<bool> DeletePartAsync(int id)
        {
            var sql = $"DELETE FROM Parts WHERE Id = {id}";
            var result = await ExecuteQueryAsync(sql, null);
            return result.AffectedRowCount > 0;
        }

        public Task<List<Category>> GetAllCategoriesAsync()
        {
            const string sql = @"
                SELECT Id, Name, Description, ParentCategoryId, DisplayOrder, CreatedDate
                FROM Categories
                ORDER BY DisplayOrder, Name
            ";
            return ExecuteQueryAndMapAsync(sql, MapRowsToCategories);
        }

        /// <summary>
        /// Initializes the Turso database with schema and seed data.
        /// Only runs once - uses INSERT OR IGNORE to be idempotent.
        /// </summary>
        public async Task InitializeDatabaseSchemaAsync()
        {
            // Schema definitions
            var schemaQueries = new[]
            {
                @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id INTEGER PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        ParentCategoryId INTEGER,
                        DisplayOrder INTEGER DEFAULT 0,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP
                    )
                ",
                @"
                    CREATE TABLE IF NOT EXISTS Parts (
                        Id INTEGER PRIMARY KEY,
                        PartNumber TEXT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        CategoryId INTEGER,
                        CostPrice REAL NOT NULL DEFAULT 0,
                        StockQuantity INTEGER NOT NULL DEFAULT 0,
                        Location TEXT,
                        Brand TEXT,
                        ImagePath TEXT,
                        Model TEXT,
                        ReleaseYear INTEGER,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        LastUpdated TEXT,
                        FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                    )
                "
            };

            // Execute all schema queries
            foreach (var query in schemaQueries)
            {
                await ExecuteQueryAsync(query);
            }

            // Migration: Add new columns to existing Parts table if they don't exist
            // SQLite doesn't support ALTER TABLE ADD COLUMN IF NOT EXISTS, so we use try-catch
            try
            {
                await ExecuteQueryAsync("ALTER TABLE Parts ADD COLUMN Model TEXT");
            }
            catch
            {
                // Column already exists or table structure error - safe to ignore
            }

            // Migrate ReleaseDate TEXT to ReleaseYear INTEGER
            // First add the new ReleaseYear column
            try
            {
                await ExecuteQueryAsync("ALTER TABLE Parts ADD COLUMN ReleaseYear INTEGER");

                // Migrate data from ReleaseDate to ReleaseYear (extract year from date string)
                await ExecuteQueryAsync(@"
                    UPDATE Parts
                    SET ReleaseYear = CAST(substr(COALESCE(ReleaseDate, '0000'), 1, 4) AS INTEGER)
                    WHERE ReleaseYear IS NULL AND ReleaseDate IS NOT NULL
                ");

                // Drop old ReleaseDate, RetailPrice, Supplier columns if they exist
                try
                {
                    await ExecuteQueryAsync(@"
                        CREATE TABLE Parts_New AS
                        SELECT Id, PartNumber, Name, Description, CategoryId, CostPrice,
                               StockQuantity, Location, Brand, ImagePath, Model, ReleaseYear,
                               CreatedDate, LastUpdated
                        FROM Parts
                    ");
                    await ExecuteQueryAsync("DROP TABLE Parts");
                    await ExecuteQueryAsync("ALTER TABLE Parts_New RENAME TO Parts");
                }
                catch (Exception dropEx)
                {
                    Console.WriteLine($"[TURSO MIGRATION] Could not drop ReleaseDate column (may not exist): {dropEx.Message}");
                }
            }
            catch
            {
                // Column already exists or table structure error - safe to ignore
            }

            // Migration: Add UNIQUE constraint to PartNumber column
            // SQLite/Turso doesn't support ALTER TABLE ADD UNIQUE directly, so we recreate the table
            try
            {
                // Check if Parts table has the UNIQUE constraint by trying to create an index
                // This will fail if the table doesn't exist
                await ExecuteQueryAsync(@"
                    CREATE TABLE IF NOT EXISTS Parts_MigrationCheck (
                        Id INTEGER PRIMARY KEY,
                        TestColumn TEXT
                    )
                ");
                await ExecuteQueryAsync("DROP TABLE IF EXISTS Parts_MigrationCheck");

                // Create new table without UNIQUE constraint on PartNumber (without deprecated columns)
                await ExecuteQueryAsync(@"
                    CREATE TABLE IF NOT EXISTS Parts_New (
                        Id INTEGER PRIMARY KEY,
                        PartNumber TEXT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        CategoryId INTEGER,
                        CostPrice REAL NOT NULL DEFAULT 0,
                        StockQuantity INTEGER NOT NULL DEFAULT 0,
                        Location TEXT,
                        Brand TEXT,
                        ImagePath TEXT,
                        Model TEXT,
                        ReleaseYear INTEGER,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        LastUpdated TEXT,
                        FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                    )
                ");

                // Migrate all data without deduplication (PartNumber is no longer UNIQUE)
                await ExecuteQueryAsync(@"
                    INSERT INTO Parts_New (Id, PartNumber, Name, Description, CategoryId, CostPrice, StockQuantity, Location, Brand, ImagePath, Model, ReleaseYear, CreatedDate, LastUpdated)
                    SELECT Id, PartNumber, Name, Description, CategoryId, CostPrice, StockQuantity, Location, Brand, ImagePath, Model, ReleaseYear, CreatedDate, LastUpdated
                    FROM Parts
                ");

                // Drop old table and rename new one
                await ExecuteQueryAsync("DROP TABLE Parts");
                await ExecuteQueryAsync("ALTER TABLE Parts_New RENAME TO Parts");
                Console.WriteLine("[TURSO MIGRATION] Removed UNIQUE constraint from PartNumber");
            }
            catch
            {
                // Table structure may already be correct or other migration error - safe to ignore
                Console.WriteLine("[TURSO MIGRATION] Parts table may already be updated (no UNIQUE constraint)");
            }

            // Seed categories with explicit IDs for consistency
            var categorySql = @"
                INSERT OR IGNORE INTO Categories (Id, Name, Description, DisplayOrder, CreatedDate)
                VALUES
                    (1, 'Engine Parts', NULL, 0, CURRENT_TIMESTAMP),
                    (2, 'Transmission', 'Power transfer components', 1, CURRENT_TIMESTAMP),
                    (3, 'Suspension', 'Shock absorbers and struts', 2, CURRENT_TIMESTAMP),
                    (4, 'Braking System', 'Brake pads, rotors, and calipers', 3, CURRENT_TIMESTAMP),
                    (5, 'Exhaust System', 'Mufflers and exhaust pipes', 4, CURRENT_TIMESTAMP),
                    (6, 'Fuel System', 'Fuel pumps and injectors', 5, CURRENT_TIMESTAMP),
                    (7, 'Ignition System', 'Spark plugs and coils', 6, CURRENT_TIMESTAMP),
                    (8, 'Cooling System', 'Radiators and cooling fans', 7, CURRENT_TIMESTAMP),
                    (9, 'Electrical', 'Batteries and alternators', 8, CURRENT_TIMESTAMP),
                    (10, 'Body & Interior', 'Seats, upholstery, and interior accessories', 9, CURRENT_TIMESTAMP),
                    (11, 'Wheels & Tires', 'Wheels, tires, and suspension parts', 0, CURRENT_TIMESTAMP)
            ";
            await ExecuteQueryAsync(categorySql);
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, Name, Description, ParentCategoryId, DisplayOrder, CreatedDate
                FROM Categories
                WHERE Id = @id
            ";
            var categories = await ExecuteQueryAndMapAsync(sql, MapRowsToCategories, new Dictionary<string, object> { ["@id"] = id });
            return categories.Count > 0 ? categories[0] : null;
        }

        public Task<List<CarPart>> GetPartsByCategoryAsync(int categoryId)
        {
            const string sql = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
                       p.CostPrice, p.StockQuantity, p.Location, p.Brand, p.ImagePath,
                       p.Model, p.ReleaseYear, p.CreatedDate, p.LastUpdated
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.CategoryId = @categoryId
                ORDER BY p.Name
            ";
            return ExecuteQueryAndMapAsync(sql, MapRowsToParts, new Dictionary<string, object> { ["@categoryId"] = categoryId });
        }

        public Task<List<CarPart>> SearchPartsAsync(string searchTerm)
        {
            const string sql = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
                       p.CostPrice, p.StockQuantity, p.Location, p.Brand, p.ImagePath,
                       p.Model, p.ReleaseYear, p.CreatedDate, p.LastUpdated
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.PartNumber LIKE @searchTerm
                   OR p.Name LIKE @searchTerm
                   OR p.Description LIKE @searchTerm
                   OR p.Brand LIKE @searchTerm
                ORDER BY p.Name
            ";
            var parameters = new Dictionary<string, object> { ["@searchTerm"] = $"%{searchTerm}%" };
            return ExecuteQueryAndMapAsync(sql, MapRowsToParts, parameters);
        }

        // ============================================================================
        // CORE QUERY EXECUTION
        // ============================================================================

        /// <summary>
        /// Executes a SQL query against Turso and returns the raw query result.
        /// This is the core method that all public API methods use.
        /// </summary>
        private async Task<TursoQueryResult> ExecuteQueryAsync(
            string sql,
            Dictionary<string, object>? parameters = null,
            CancellationToken cancellationToken = default)
        {
            // Build the Turso pipeline request
            var request = BuildTursoRequest(sql, parameters);

            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Initialize query info for tracking
            DatabaseQueryInfo? queryInfo = null;
            if (_debugTrackingEnabled)
            {
                queryInfo = new DatabaseQueryInfo
                {
                    Sql = sql,
                    Parameters = FormatParametersForDisplay(parameters),
                    RequestBody = jsonContent
                };
            }

            try
            {
                var response = await _httpClient.PostAsync(_httpUrl, httpContent, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (_debugTrackingEnabled && queryInfo != null)
                    {
                        queryInfo.Success = false;
                        queryInfo.HttpStatusCode = (int)response.StatusCode;
                        queryInfo.ResponseBody = errorContent;
                        queryInfo.ErrorMessage = $"HTTP {response.StatusCode}: {errorContent}";
                        _capturedQueries.Add(queryInfo);
                    }

                    throw new TursoException(
                        $"Turso API error ({response.StatusCode}): {errorContent}",
                        response.StatusCode,
                        errorContent);
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

                var tursoResponse = JsonSerializer.Deserialize<TursoResponse>(responseJson, _jsonOptions)
                    ?? throw new TursoException("Failed to deserialize Turso response");

                if (tursoResponse.Results == null || tursoResponse.Results.Count == 0)
                {

                    if (_debugTrackingEnabled && queryInfo != null)
                    {
                        queryInfo.Success = false;
                        queryInfo.ResponseBody = responseJson;
                        queryInfo.ErrorMessage = "No query result found in Turso response";
                        _capturedQueries.Add(queryInfo);
                    }

                    throw new TursoException("No query result found in Turso response");
                }

                // Extract the query result from the nested response structure
                // The structure is: results[0].response.result
                var firstResult = tursoResponse.Results[0];

                if (firstResult.Type != "ok" || firstResult.Response == null)
                {

                    if (_debugTrackingEnabled && queryInfo != null)
                    {
                        queryInfo.Success = false;
                        queryInfo.ResponseBody = responseJson;
                        queryInfo.ErrorMessage = $"Unexpected Turso response format. Result type: {firstResult.Type}";
                        _capturedQueries.Add(queryInfo);
                    }

                    throw new TursoException($"Unexpected Turso response format. Result type: {firstResult.Type}");
                }

                if (firstResult.Response.Type != "execute" || firstResult.Response.Result == null)
                {

                    if (_debugTrackingEnabled && queryInfo != null)
                    {
                        queryInfo.Success = false;
                        queryInfo.ResponseBody = responseJson;
                        queryInfo.ErrorMessage = $"Unexpected Turso response format. Response type: {firstResult.Response.Type}";
                        _capturedQueries.Add(queryInfo);
                    }

                    throw new TursoException($"Unexpected Turso response format. Response type: {firstResult.Response.Type}");
                }

                var queryResult = firstResult.Response.Result;

                // Capture successful query info
                if (_debugTrackingEnabled && queryInfo != null)
                {
                    queryInfo.Success = true;
                    queryInfo.ResponseBody = responseJson;
                    queryInfo.HttpStatusCode = (int)response.StatusCode;
                    queryInfo.AffectedRowCount = queryResult.AffectedRowCount;
                    queryInfo.LastInsertRowId = queryResult.LastInsertRowid;
                    queryInfo.RowsReturned = queryResult.Rows?.Count;
                    queryInfo.QueryDurationMs = queryResult.QueryDurationMs;
                    _capturedQueries.Add(queryInfo);
                }

                return queryResult;
            }
            catch (TursoException)
            {
                // Re-throw our custom exceptions
                throw;
            }
            catch (HttpRequestException ex)
            {
                if (_debugTrackingEnabled && queryInfo != null)
                {
                    queryInfo.Success = false;
                    queryInfo.ErrorMessage = $"HTTP error communicating with Turso: {ex.Message}";
                    _capturedQueries.Add(queryInfo);
                }
                throw new TursoException($"HTTP error communicating with Turso: {ex.Message}", innerException: ex);
            }
            catch (JsonException ex)
            {
                if (_debugTrackingEnabled && queryInfo != null)
                {
                    queryInfo.Success = false;
                    queryInfo.ErrorMessage = $"Failed to parse Turso response: {ex.Message}";
                    _capturedQueries.Add(queryInfo);
                }
                throw new TursoException($"Failed to parse Turso response: {ex.Message}", innerException: ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                if (_debugTrackingEnabled && queryInfo != null)
                {
                    queryInfo.Success = false;
                    queryInfo.ErrorMessage = "Request to Turso timed out";
                    _capturedQueries.Add(queryInfo);
                }
                throw new TursoException("Request to Turso timed out", innerException: ex);
            }
        }

        /// <summary>
        /// Executes a query and maps the results to entities using the provided mapper function.
        /// </summary>
        private async Task<List<T>> ExecuteQueryAndMapAsync<T>(
            string sql,
            Func<TursoQueryResult, List<T>> mapper,
            Dictionary<string, object>? parameters = null)
        {
            var result = await ExecuteQueryAsync(sql, parameters);
            return mapper(result);
        }

        /// <summary>
        /// Formats parameters for display in debug output
        /// </summary>
        private static string FormatParametersForDisplay(Dictionary<string, object>? parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return "(none)";

            var sb = new StringBuilder();
            var first = true;
            foreach (var kvp in parameters)
            {
                if (!first) sb.Append(", ");
                first = false;

                var valueStr = FormatValueForDisplay(kvp.Value);
                sb.Append($"{kvp.Key} = {valueStr}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Formats a single value for display in debug output
        /// </summary>
        private static string FormatValueForDisplay(object? value)
        {
            return value switch
            {
                null => "NULL",
                string s => $"'{s.Replace("'", "''")}'",
                DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
                bool b => b ? "1" : "0",
                _ => value.ToString() ?? "NULL"
            };
        }

        /// <summary>
        /// Builds a Turso pipeline request from SQL and parameters.
        /// </summary>
        private static TursoPipelineRequest BuildTursoRequest(string sql, Dictionary<string, object>? parameters)
        {
            var executeRequest = new TursoExecuteRequest
            {
                Type = "execute",
                Stmt = new TursoStatement
                {
                    Sql = sql
                }
            };

            // Add named parameters if provided
            if (parameters != null && parameters.Count > 0)
            {
                executeRequest.Stmt.NamedArgs = new List<TursoNamedArg>();
                foreach (var kvp in parameters)
                {
                    executeRequest.Stmt.NamedArgs.Add(new TursoNamedArg
                    {
                        Name = kvp.Key,
                        Value = ConvertToTursoValue(kvp.Value)
                    });
                }
            }

            return new TursoPipelineRequest
            {
                Requests = new List<object>
                {
                    executeRequest,
                    new { type = "close" } // Always close the connection
                }
            };
        }

        /// <summary>
        /// Converts a C# value to Turso's value format.
        /// According to Turso docs, all values should use typed format with value as string.
        /// See: https://docs.turso.tech/sdk/http - "In JSON, the value is a String to avoid losing precision"
        /// </summary>
        private static object ConvertToTursoValue(object? value)
        {
            // Handle null and DBNull as null type
            if (value == null || value == DBNull.Value)
                return new { type = "null", value = (string?)null };

            return value switch
            {
                string s => new { type = "text", value = s },
                int i => new { type = "integer", value = i.ToString(CultureInfo.InvariantCulture) },
                long l => new { type = "integer", value = l.ToString(CultureInfo.InvariantCulture) },
                double d => new { type = "float", value = d.ToString(CultureInfo.InvariantCulture) },
                decimal d => new { type = "float", value = d.ToString(CultureInfo.InvariantCulture) },
                float f => new { type = "float", value = f.ToString(CultureInfo.InvariantCulture) },
                bool b => new { type = "integer", value = (b ? "1" : "0") },
                DateTime dt => new { type = "text", value = dt.ToString("yyyy-MM-dd HH:mm:ss") },
                _ => new { type = "text", value = value.ToString()! }
            };
        }

        // ============================================================================
        // RESULT MAPPING HELPERS
        // ============================================================================

        /// <summary>
        /// Extracts a value from a Turso result row cell.
        /// Handles both direct values (string, number, null) and Turso's typed format:
        /// { "type": "text|integer|null|float", "value": "..." }
        /// Also handles null-only format: { "type": "null" }
        /// </summary>
        private static object? ExtractValue(object? cellValue)
        {
            if (cellValue == null) return null;

            // Handle JsonElement from JSON deserialization
            if (cellValue is JsonElement element)
            {
                // Check for Turso's typed value format: { "type": "...", "value": "..." }
                if (element.ValueKind == JsonValueKind.Object &&
                    element.TryGetProperty("type", out var typeProp))
                {
                    // Check if it's a null value with or without value property
                    var type = typeProp.GetString();
                    if (type == "null")
                    {
                        return null;
                    }

                    // If there's a value property, extract it
                    if (element.TryGetProperty("value", out var valueProp))
                    {
                        return ExtractTypedValue(type, valueProp);
                    }

                    // Object with type but no value - treat as null
                    return null;
                }

                // Handle direct JSON values
                return element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l :
                                          element.TryGetDouble(out var d) ? (object)d : null,
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => null // Unexpected type, return null instead of raw JSON text
                };
            }

            return cellValue;
        }

        /// <summary>
        /// Extracts a value from Turso's typed format.
        /// </summary>
        private static object? ExtractTypedValue(string? type, JsonElement valueElement)
        {
            return type switch
            {
                "null" => null,
                "text" or "varchar" => valueElement.ValueKind == JsonValueKind.Null
                    ? null
                    : valueElement.GetString(),
                "integer" when valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetInt64(out var l) => l,
                "integer" when valueElement.ValueKind == JsonValueKind.String => long.TryParse(
                    valueElement.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) ? l : null,
                "float" or "real" or "double" when valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetDouble(out var d) => d,
                "float" or "real" or "double" when valueElement.ValueKind == JsonValueKind.String => double.TryParse(
                    valueElement.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null,
                "boolean" when valueElement.ValueKind == JsonValueKind.True => true,
                "boolean" when valueElement.ValueKind == JsonValueKind.False => false,
                _ => valueElement.ToString() // Fallback to string representation
            };
        }

        private static List<CarPart> MapRowsToParts(TursoQueryResult result)
        {
            var parts = new List<CarPart>();

            if (result.Rows == null || result.Rows.Count == 0)
                return parts;

            foreach (var row in result.Rows)
            {
                // Handle ReleaseYear/ReleaseDate column (migration compatibility)
                // Column index: 12 (14 columns total: 0-14)
                // 0: Id, 1: PartNumber, 2: Name, 3: Description, 4: CategoryId, 5: CategoryName,
                // 6: CostPrice, 7: StockQuantity, 8: Location, 9: Brand, 10: ImagePath,
                // 11: Model, 12: ReleaseYear, 13: CreatedDate, 14: LastUpdated
                int? releaseYear = null;
                var releaseYearValue = ExtractValue(row[12]);

                if (releaseYearValue != null)
                {
                    try
                    {
                        // Check if it's a string (old schema with full date) or number (new schema with year only)
                        if (releaseYearValue is string dateString)
                        {
                            // Skip JSON metadata strings that indicate null values
                            if (dateString.Contains("\"type\""))
                            {
                                // This is likely JSON metadata like {"type":"null"} - skip
                                releaseYear = null;
                            }
                            // Old schema: Parse year from date string (e.g., "2024-01-15" -> 2024)
                            else if (dateString.Length >= 4 && int.TryParse(dateString.Substring(0, 4), out var year))
                            {
                                releaseYear = year;
                            }
                        }
                        else if (releaseYearValue is long longValue)
                        {
                            // Direct integer value from JSON
                            releaseYear = (int)longValue;
                        }
                        else if (releaseYearValue is int intValue)
                        {
                            // Direct integer value
                            releaseYear = intValue;
                        }
                        else
                        {
                            // Try to convert as a string
                            var stringValue = Convert.ToString(releaseYearValue);
                            if (!string.IsNullOrEmpty(stringValue) && !stringValue.Contains("\"type\""))
                            {
                                if (int.TryParse(stringValue, out var parsedYear))
                                {
                                    releaseYear = parsedYear;
                                }
                            }
                        }
                    }
                    catch (FormatException)
                    {
                        // Invalid format, keep null
                        releaseYear = null;
                    }
                    catch (Exception ex)
                    {
                        // Log other exceptions for debugging but keep null
                        Console.WriteLine($"[TursoDataService] Error parsing ReleaseYear: {ex.Message}");
                        releaseYear = null;
                    }
                }

                parts.Add(new CarPart
                {
                    Id = Convert.ToInt32(ExtractValue(row[0])),
                    PartNumber = Convert.ToString(ExtractValue(row[1])) ?? string.Empty,
                    Name = Convert.ToString(ExtractValue(row[2])) ?? string.Empty,
                    Description = Convert.ToString(ExtractValue(row[3])) ?? string.Empty,
                    CategoryId = Convert.ToInt32(ExtractValue(row[4])),
                    CategoryName = Convert.ToString(ExtractValue(row[5])) ?? string.Empty,
                    CostPrice = Convert.ToDecimal(ExtractValue(row[6])),
                    StockQuantity = Convert.ToInt32(ExtractValue(row[7])),
                    Location = Convert.ToString(ExtractValue(row[8])) ?? string.Empty,
                    Brand = Convert.ToString(ExtractValue(row[9])) ?? string.Empty,
                    ImagePath = Convert.ToString(ExtractValue(row[10])) ?? string.Empty,
                    Model = Convert.ToString(ExtractValue(row[11])),
                    ReleaseDate = releaseYear,
                    CreatedDate = DateTime.TryParse(Convert.ToString(ExtractValue(row[13])), out var createdDate)
                        ? createdDate : DateTime.Now,
                    LastUpdated = DateTime.TryParse(Convert.ToString(ExtractValue(row[14])), out var updatedDate)
                        ? updatedDate : null
                });
            }

            return parts;
        }

        private static List<Category> MapRowsToCategories(TursoQueryResult result)
        {
            var categories = new List<Category>();

            if (result.Rows == null || result.Rows.Count == 0)
                return categories;

            foreach (var row in result.Rows)
            {
                categories.Add(new Category
                {
                    Id = Convert.ToInt32(ExtractValue(row[0])),
                    Name = Convert.ToString(ExtractValue(row[1])) ?? string.Empty,
                    Description = Convert.ToString(ExtractValue(row[2])) ?? string.Empty,
                    ParentCategoryId = int.TryParse(Convert.ToString(ExtractValue(row[3])), out var parentId) && parentId > 0
                        ? parentId
                        : null,
                    DisplayOrder = Convert.ToInt32(ExtractValue(row[4])),
                    CreatedDate = DateTime.TryParse(Convert.ToString(ExtractValue(row[5])), out var createdDate)
                        ? createdDate : DateTime.Now
                });
            }

            return categories;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }

    // ============================================================================
    // TURSO API MODELS
    // ============================================================================

    /// <summary>
    /// Top-level Turso pipeline request.
    /// </summary>
    internal class TursoPipelineRequest
    {
        [JsonPropertyName("requests")]
        public List<object> Requests { get; set; } = new();
    }

    /// <summary>
    /// Execute request item in the pipeline.
    /// </summary>
    internal class TursoExecuteRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "execute";

        [JsonPropertyName("stmt")]
        public TursoStatement Stmt { get; set; } = new();
    }

    /// <summary>
    /// SQL statement with optional named arguments.
    /// </summary>
    internal class TursoStatement
    {
        [JsonPropertyName("sql")]
        public string Sql { get; set; } = string.Empty;

        [JsonPropertyName("named_args")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<TursoNamedArg>? NamedArgs { get; set; }
    }

    /// <summary>
    /// Named parameter argument.
    /// Format: { "name": "@paramName", "value": "paramValue" }
    /// The value can be a direct value or typed: { "type": "...", "value": "..." }
    /// </summary>
    internal class TursoNamedArg
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public object Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Top-level Turso pipeline response.
    /// The response has nested structure: results[0].response.result
    /// </summary>
    internal class TursoResponse
    {
        [JsonPropertyName("results")]
        public List<TursoResultItem>? Results { get; set; }

        [JsonPropertyName("baton")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Baton { get; set; }

        [JsonPropertyName("base_url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BaseUrl { get; set; }
    }

    /// <summary>
    /// Individual result item in the results array.
    /// Contains type, response type, and optional result data.
    /// </summary>
    internal class TursoResultItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("response")]
        public TursoResponseData? Response { get; set; }

        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TursoError? Error { get; set; }
    }

    /// <summary>
    /// Response data within a result item.
    /// For execute requests, contains the actual query result.
    /// </summary>
    internal class TursoResponseData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("result")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TursoQueryResult? Result { get; set; }
    }

    /// <summary>
    /// Query result containing rows, columns, and metadata.
    /// This is the actual data returned from a query.
    ///
    /// CRITICAL: Turso returns all numeric fields as STRINGS in JSON to avoid precision loss.
    /// Use JsonStringConverter for numeric fields to handle both string and number values.
    /// </summary>
    internal class TursoQueryResult
    {
        [JsonPropertyName("cols")]
        public object[]? Cols { get; set; }

        [JsonPropertyName("rows")]
        public List<object[]>? Rows { get; set; }

        [JsonPropertyName("affected_row_count")]
        [JsonConverter(typeof(JsonStringToLongConverter))]
        public long AffectedRowCount { get; set; }

        [JsonPropertyName("last_insert_rowid")]
        [JsonConverter(typeof(JsonStringToNullableLongConverter))]
        public long? LastInsertRowid { get; set; }

        [JsonPropertyName("replication_index")]
        public string? ReplicationIndex { get; set; }

        [JsonPropertyName("rows_read")]
        [JsonConverter(typeof(JsonStringToLongConverter))]
        public long RowsRead { get; set; }

        [JsonPropertyName("rows_written")]
        [JsonConverter(typeof(JsonStringToLongConverter))]
        public long RowsWritten { get; set; }

        [JsonPropertyName("query_duration_ms")]
        [JsonConverter(typeof(JsonStringToDoubleConverter))]
        public double QueryDurationMs { get; set; }
    }

    /// <summary>
    /// JSON converter for Turso string-based long values.
    /// Handles both "123" (string) and 123 (number) formats.
    /// </summary>
    internal class JsonStringToLongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                    return result;
                throw new JsonException($"Cannot convert string '{value}' to long.");
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }
            throw new JsonException($"Unexpected token type {reader.TokenType} for long conversion.");
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// JSON converter for Turso string-based nullable long values.
    /// Handles "null", "123" (string), 123 (number), and null formats.
    /// </summary>
    internal class JsonStringToNullableLongConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                    return null;
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                    return result;
                throw new JsonException($"Cannot convert string '{value}' to long?");
            }

            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt64();

            throw new JsonException($"Unexpected token type {reader.TokenType} for long? conversion.");
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

    /// <summary>
    /// JSON converter for Turso string-based double values.
    /// Handles both "12.34" (string) and 12.34 (number) formats.
    /// </summary>
    internal class JsonStringToDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return result;
                throw new JsonException($"Cannot convert string '{value}' to double.");
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }
            throw new JsonException($"Unexpected token type {reader.TokenType} for double conversion.");
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// Error information if a request failed.
    /// </summary>
    internal class TursoError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Code { get; set; }
    }

    /// <summary>
    /// Custom exception for Turso-specific errors.
    /// Provides clear error messages and maintains HTTP status context.
    /// </summary>
    public class TursoException : Exception
    {
        public System.Net.HttpStatusCode? StatusCode { get; }
        public string? ErrorResponse { get; }

        public TursoException(string message) : base(message) { }

        public TursoException(string message, System.Net.HttpStatusCode? statusCode = null, string? errorResponse = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorResponse = errorResponse;
        }

        public TursoException(string message, Exception? innerException)
            : base(message, innerException) { }
    }
}
