using CarPartStoreApp.Data;
using CarPartStoreApp.Localization;
using CarPartStoreApp.Services;
using CarPartStoreApp.Views;
using System;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;

namespace CarPartStoreApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private EmbeddedApiServer? _apiServer;
    private CancellationTokenSource? _appCancellationTokenSource;
    private Task? _apiServerTask;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Create cancellation token for app-wide shutdown coordination
        _appCancellationTokenSource = new CancellationTokenSource();

        // Set shutdown mode to automatically close app when last window closes
        ShutdownMode = ShutdownMode.OnLastWindowClose;

        // Check which database to use and display info
        var databaseType = DataServiceFactory.GetDatabaseType();
        // Console.WriteLine($"🔧 Starting CarPartStoreApp with {databaseType}");

        // Initialize local database only if using SQLite (not Turso)
        if (!Data.DatabaseConfig.UseTurso())
        {
            // Console.WriteLine("📦 Initializing local SQLite database...");
            DatabaseInitializer.Initialize();
        }
        else
        {
            // Console.WriteLine("☁️ Using Turso cloud database...");
            // Initialize Turso database schema if needed
            var tursoDataService = new Services.TursoDataService();
            _ = Task.Run(async () =>
            {
                try
                {
                    await tursoDataService.InitializeDatabaseSchemaAsync();
                    // Console.WriteLine("✅ Turso database schema initialized");
                }
                catch
                {
                    // Silently ignore errors during initialization
                }
            });
        }

        // Load app settings (including language preference)
        var settings = AppSettings.Load();

        // Initialize localization service with saved language
        var localizationService = new LocalizationService(settings.CurrentLanguage);

        // Register services
        ServiceContainer.RegisterInstance(settings);
        ServiceContainer.RegisterInstance(localizationService);

        // Create data service using factory (Turso or SQLite based on config)
        var dataService = DataServiceFactory.GetDataService();

        // Create and store API server reference
        _apiServer = new EmbeddedApiServer();
        ServiceContainer.RegisterInstance(_apiServer);

        // Start the API server in background and track the task
        _apiServerTask = Task.Run(async () =>
        {
            try
            {
                await _apiServer.StartAsync(_appCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Server startup error: {ex.Message}");
            }
        });

        // Create and show MainWindow with services passed directly
        var mainWindow = new MainWindow(dataService, settings, localizationService);
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        // Signal cancellation to all background tasks
        _appCancellationTokenSource?.Cancel();

        // Stop the API server when app exits with timeout protection
        try
        {
            if (_apiServer != null)
            {
                var stopTask = _apiServer.StopAsync(CancellationToken.None);

                // Wait for stop to complete, but with a timeout
                if (stopTask.Wait(TimeSpan.FromSeconds(5)))
                {
                    // API server stopped successfully
                }
                else
                {
                    // Timeout - force shutdown
                    System.Diagnostics.Debug.WriteLine("API server stop timed out, forcing shutdown");
                }
            }

            // Wait for the API server task to complete (if still running)
            if (_apiServerTask != null && !_apiServerTask.IsCompleted)
            {
                _apiServerTask.Wait(TimeSpan.FromSeconds(2));
            }
        }
        catch (Exception ex)
        {
            // Log any errors during shutdown
            System.Diagnostics.Debug.WriteLine($"Error during shutdown: {ex.Message}");
        }
        finally
        {
            // Clean up cancellation token
            _appCancellationTokenSource?.Dispose();
        }
    }
}