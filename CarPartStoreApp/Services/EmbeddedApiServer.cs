using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using CarPartStoreApp.Data;
using CarPartStoreApp.Services;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// Embedded Web API Server for WPF Application
    /// Runs ASP.NET Core API in background thread on configurable port
    /// </summary>
    public class EmbeddedApiServer : IHostedService
    {
        private IHost? _host;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task? _runTask;
        private bool _isRunning;

        /// <summary>
        /// Gets the base URL where the API is accessible
        /// Default: http://localhost:5000
        /// </summary>
        public string BaseUrl { get; private set; } = "http://localhost:5000";

        /// <summary>
        /// Gets whether the API server is currently running
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Starts the API server asynchronously
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("API server is already running");
            }

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://localhost:5000");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Car Parts Storage API",
                    Version = "v1"
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactDevServer", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Register data service for controllers
            builder.Services.AddScoped<IDataService, SqliteDataService>();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseCors("AllowReactDevServer");
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapControllers();

            _host = app;
            _isRunning = true;

            // Run the server asynchronously
            _runTask = Task.Run(async () =>
            {
                try
                {
                    await _host.RunAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Server was stopped gracefully
                }
                catch (Exception ex)
                {
                    // Log the error (could be enhanced with proper logging)
                    System.Diagnostics.Debug.WriteLine($"API Server Error: {ex.Message}");
                }
                finally
                {
                    _isRunning = false;
                }
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stops the API server gracefully
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_isRunning || _host == null)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            _isRunning = false;

            try
            {
                if (_runTask != null)
                {
                    await _runTask;
                }
            }
            catch (TaskCanceledException)
            {
                // Expected cancellation
            }

            if (_host != null)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
        }
    }
}
