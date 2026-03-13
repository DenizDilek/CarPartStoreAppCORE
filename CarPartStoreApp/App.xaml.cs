using CarPartStoreApp.Data;
using CarPartStoreApp.Localization;
using CarPartStoreApp.Services;
using CarPartStoreApp.Views;
using System.Windows;
using System.Threading;

namespace CarPartStoreApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize database
        DatabaseInitializer.Initialize();

        // Load app settings (including language preference)
        var settings = AppSettings.Load();

        // Initialize localization service with saved language
        var localizationService = new LocalizationService(settings.CurrentLanguage);

        // Register services
        ServiceContainer.RegisterInstance(settings);
        ServiceContainer.RegisterInstance(localizationService);

        // Create data service
        var dataService = new SqliteDataService();

        // Register API server service
        var apiServer = new EmbeddedApiServer();
        ServiceContainer.RegisterInstance(apiServer);

        // Start the API server in background
        _ = Task.Run(async () => await apiServer.StartAsync(CancellationToken.None));

        // Create and show MainWindow with services passed directly
        var mainWindow = new MainWindow(dataService, settings, localizationService);
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        // Stop the API server when app exits
        var apiServer = ServiceContainer.GetService<EmbeddedApiServer>();
        apiServer?.StopAsync(CancellationToken.None).Wait();
    }
}