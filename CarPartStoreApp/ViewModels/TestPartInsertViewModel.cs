using CarPartStoreApp.Services;
using CarPartStoreApp.Models;
using System;
using System.Threading.Tasks;
using System.Text;

namespace CarPartStoreApp.ViewModels
{
    /// <summary>
    /// Simple test class for verifying Part INSERT functionality
    /// </summary>
    public class TestPartInsertViewModel
    {
        private readonly IDataService _dataService;

        public TestPartInsertViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Tests inserting a new part into the database using dummy data
        /// </summary>
        public async Task<string> TestInsertAsync()
        {
            var testPart = new CarPart
            {
                PartNumber = "TEST-001",
                Name = "Test Brake Pad",
                Description = "Dummy test part for INSERT verification",
                CategoryId = 1,
                CostPrice = 15,
<<<<<<< Updated upstream
                RetailPrice = 25,
                StockQuantity = 50,
                Location = "Shelf A1",
                Supplier = "Test Supplier Inc",
=======
                StockQuantity = 50,
                Location = "Shelf A1",
                Brand = "Bosch",
>>>>>>> Stashed changes
                ImagePath = "",
                Model = "Civic",
                ReleaseDate = DateTime.Now.Year,
                CreatedDate = DateTime.Now
            };

            // Enable debug tracking to capture query and response details
            _dataService.EnableDebugTracking(true);

            try
            {
                var insertedId = await _dataService.AddPartAsync(testPart);

                // Build detailed success message
                var message = new StringBuilder();
                message.AppendLine("SUCCESS: Inserted part with ID: " + insertedId);
                message.AppendLine();
                message.AppendLine("Database: " + _dataService.GetDatabaseType());
                message.AppendLine();

                // Try to get detailed query info if supported
                if (_dataService is TursoDataService tursoDataService)
                {
                    var queryInfo = tursoDataService.GetLastQueryInfo();
                    if (queryInfo != null)
                    {
                        message.AppendLine("SQL Query:");
                        message.AppendLine(queryInfo.Sql);
                        message.AppendLine();

                        if (!string.IsNullOrEmpty(queryInfo.Parameters) && queryInfo.Parameters != "(none)")
                        {
                            message.AppendLine("Parameters:");
                            message.AppendLine(queryInfo.Parameters);
                            message.AppendLine();
                        }

                        message.AppendLine("Turso Response:");
                        message.AppendLine($"  Status: {queryInfo.HttpStatusCode ?? 0} {(queryInfo.Success ? "OK" : "ERROR")}");

                        if (queryInfo.AffectedRowCount.HasValue)
                        {
                            message.AppendLine($"  Affected Rows: {queryInfo.AffectedRowCount}");
                        }

                        if (queryInfo.LastInsertRowId.HasValue)
                        {
                            message.AppendLine($"  Last Insert Row ID: {queryInfo.LastInsertRowId}");
                        }

                        if (queryInfo.RowsReturned.HasValue)
                        {
                            message.AppendLine($"  Rows Returned: {queryInfo.RowsReturned}");
                        }

                        if (queryInfo.QueryDurationMs.HasValue)
                        {
                            message.AppendLine($"  Query Duration: {queryInfo.QueryDurationMs:F2} ms");
                        }

                        message.AppendLine();

                        if (!string.IsNullOrEmpty(queryInfo.ResponseBody))
                        {
                            message.AppendLine("Full Response JSON:");
                            message.AppendLine(FormatJson(queryInfo.ResponseBody, indent: "    "));
                        }
                    }
                }

                return message.ToString();
            }
            catch (Exception ex)
            {
                // Build detailed error message
                var message = new StringBuilder();
                message.AppendLine("FAILED: " + ex.Message);
                message.AppendLine();
                message.AppendLine("Database: " + _dataService.GetDatabaseType());
                message.AppendLine();

                // Try to get query info even on error
                if (_dataService is TursoDataService tursoDataService)
                {
                    var queryInfo = tursoDataService.GetLastQueryInfo();
                    if (queryInfo != null)
                    {
                        message.AppendLine("SQL Query:");
                        message.AppendLine(queryInfo.Sql);
                        message.AppendLine();

                        if (!string.IsNullOrEmpty(queryInfo.Parameters) && queryInfo.Parameters != "(none)")
                        {
                            message.AppendLine("Parameters:");
                            message.AppendLine(queryInfo.Parameters);
                            message.AppendLine();
                        }

                        message.AppendLine("Turso Response Details:");
                        if (queryInfo.HttpStatusCode.HasValue)
                        {
                            message.AppendLine($"  HTTP Status: {queryInfo.HttpStatusCode}");
                        }

                        if (queryInfo.AffectedRowCount.HasValue)
                        {
                            message.AppendLine($"  Affected Rows: {queryInfo.AffectedRowCount}");
                        }

                        if (!string.IsNullOrEmpty(queryInfo.ErrorMessage))
                        {
                            message.AppendLine($"  Error: {queryInfo.ErrorMessage}");
                        }

                        message.AppendLine();

                        if (!string.IsNullOrEmpty(queryInfo.ResponseBody))
                        {
                            message.AppendLine("Full Response JSON:");
                            message.AppendLine(FormatJson(queryInfo.ResponseBody, indent: "    "));
                        }
                    }
                }

                return message.ToString();
            }
            finally
            {
                // Disable debug tracking after test
                _dataService.EnableDebugTracking(false);
            }
        }

        /// <summary>
        /// Formats JSON string with indentation for better readability
        /// </summary>
        private static string FormatJson(string json, string indent = "  ")
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            var result = new StringBuilder();
            var currentIndent = 0;
            var inString = false;
            var escapeNext = false;

            foreach (char c in json)
            {
                if (escapeNext)
                {
                    result.Append(c);
                    escapeNext = false;
                    continue;
                }

                if (c == '\\' && inString)
                {
                    escapeNext = true;
                    result.Append(c);
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    result.Append(c);
                    continue;
                }

                if (inString)
                {
                    result.Append(c);
                    continue;
                }

                switch (c)
                {
                    case '{':
                    case '[':
                        result.Append(c);
                        result.AppendLine();
                        currentIndent++;
                        AppendIndent(result, indent, currentIndent);
                        break;

                    case '}':
                    case ']':
                        result.AppendLine();
                        currentIndent--;
                        AppendIndent(result, indent, currentIndent);
                        result.Append(c);
                        break;

                    case ',':
                        result.Append(c);
                        result.AppendLine();
                        AppendIndent(result, indent, currentIndent);
                        break;

                    case ':':
                        result.Append(c);
                        result.Append(' ');
                        break;

                    default:
                        if (!char.IsWhiteSpace(c))
                        {
                            result.Append(c);
                        }
                        break;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Appends the indent string the specified number of times to the StringBuilder
        /// </summary>
        private static void AppendIndent(StringBuilder sb, string indent, int count)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append(indent);
            }
        }
    }
}
