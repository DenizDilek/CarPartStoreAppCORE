using CarPartStoreApp.Data;
using CarPartStoreApp.Localization;
using CarPartStoreApp.Services;
using CarPartStoreApp.ViewModels;
using System.Windows;

namespace CarPartStoreApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILocalizationService _localization;

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    public MainWindow(IDataService dataService, AppSettings settings, ILocalizationService localization)
    {
        InitializeComponent();

        _localization = localization;

        // Initialize view model with dependencies
        var viewModel = new MainWindowViewModel(dataService, settings, localization);

        // Set the data context for data binding
        DataContext = viewModel;

        // Set up column headers
        UpdateColumnHeaders();

        // Subscribe to language changes
        _localization.LanguageChanged += (sender, e) => UpdateColumnHeaders();

        // Load data after window is loaded
        Loaded += async (sender, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadDataAsync();
            }
        };
    }

    public void UpdateColumnHeaders()
    {
        if (ViewModel == null) return;

        if (PartsDataGrid.Columns.Count >= 10)
        {
            PartsDataGrid.Columns[0].Header = ViewModel.ColumnPartNumber;
            PartsDataGrid.Columns[1].Header = ViewModel.ColumnName;
            PartsDataGrid.Columns[2].Header = ViewModel.ColumnDescription;
            PartsDataGrid.Columns[3].Header = ViewModel.ColumnCategory;
            PartsDataGrid.Columns[4].Header = ViewModel.ColumnCost;
            PartsDataGrid.Columns[5].Header = ViewModel.ColumnRetail;
            PartsDataGrid.Columns[6].Header = ViewModel.ColumnStock;
            PartsDataGrid.Columns[7].Header = ViewModel.ColumnLocation;
            PartsDataGrid.Columns[8].Header = ViewModel.ColumnSupplier;
            PartsDataGrid.Columns[9].Header = ViewModel.ColumnImage;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            _localization.GetString(ResourceKeys.MessageAboutText),
            _localization.GetString(ResourceKeys.MessageAboutTitle),
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}