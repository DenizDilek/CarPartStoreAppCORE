using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CarPartStoreApp.Data;
using CarPartStoreApp.Localization;
using CarPartStoreApp.Models;
using CarPartStoreApp.Services;
using CarPartStoreApp.Views;

namespace CarPartStoreApp.ViewModels
{
    /// <summary>
    /// ViewModel for the main window
    /// Handles data binding and business logic for the main screen
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        private IDataService _dataService;
        private readonly AppSettings _settings;
        private readonly ILocalizationService _localization;
        private string _searchTerm = string.Empty;
        private CarPart? _selectedPart;
        private Category? _selectedCategory;
        private DatabaseType _selectedDatabase;
        private readonly Services.EmbeddedApiServer? _apiServer;

        public MainWindowViewModel(IDataService dataService, AppSettings settings, ILocalizationService localization, EmbeddedApiServer? apiServer = null)
        {
            _dataService = dataService;
            _settings = settings;
            _localization = localization;
            _apiServer = apiServer ?? ServiceContainer.GetService<EmbeddedApiServer>();
            _selectedDatabase = settings.SelectedDatabase;

            Parts = new ObservableCollection<CarPart>();
            Categories = new ObservableCollection<Category>();
            DatabaseTypes = new ObservableCollection<DatabaseType>
            {
                DatabaseType.Local,
                DatabaseType.Cloud
            };

            AddPartCommand = new RelayCommand(AddPart);
            EditPartCommand = new RelayCommand(EditPart, CanEditPart);
            DeletePartCommand = new RelayCommand(DeletePart, CanDeletePart);
            ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
            ChangeDatabaseCommand = new RelayCommand(ChangeDatabaseAsync);
            TestPartCommand = new RelayCommand(TestPartInsert);
        }

        #region Collections

        public ObservableCollection<CarPart> Parts { get; }
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<DatabaseType> DatabaseTypes { get; }

        #endregion

        #region Properties

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    FilterParts();
                }
            }
        }

        public CarPart? SelectedPart
        {
            get => _selectedPart;
            set => SetProperty(ref _selectedPart, value);
        }

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterParts();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected database type (Local or Cloud)
        /// When changed, switches the data source and refreshes data
        /// </summary>
        public DatabaseType SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                if (SetProperty(ref _selectedDatabase, value))
                {
                    _settings.SelectedDatabase = value;
                    _settings.Save();
                    _ = ChangeDatabaseAsync(value);
                }
            }
        }

        /// <summary>
        /// Gets the current language code
        /// </summary>
        public string CurrentLanguage => _localization.CurrentLanguage;

        /// <summary>
        /// Gets the API server status
        /// </summary>
        public string ApiServerStatus => _apiServer?.IsRunning == true ? "Running" : "Stopped";

        /// <summary>
        /// Gets the API server base URL
        /// </summary>
        public string ApiServerUrl => _apiServer?.BaseUrl ?? "Not available";

        /// <summary>
        /// Gets a localized string by key
        /// Usage: this["MainWindow.Title"]
        /// </summary>
        public string this[string key] => _localization.GetString(key);

        /// <summary>
        /// Gets the localized window title
        /// </summary>
        public string Title => this["MainWindow.Title"];

        #endregion

        #region Commands

        public ICommand AddPartCommand { get; }
        public ICommand EditPartCommand { get; }
        public ICommand DeletePartCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand ChangeDatabaseCommand { get; }
        public ICommand TestPartCommand { get; }

        #endregion

        #region Menu Items

        public string MenuFile => this["MainWindow.MenuFile"];
        public string MenuAddPart => this["MainWindow.MenuAddPart"];
        public string MenuTest => this["MainWindow.MenuTest"];
        public string MenuExit => this["MainWindow.MenuExit"];
        public string MenuEdit => this["MainWindow.MenuEdit"];
        public string MenuDeletePart => this["MainWindow.MenuDeletePart"];
        public string MenuHelp => this["MainWindow.MenuHelp"];
        public string Language => this["MainWindow.MenuLanguage"];
        public string LanguageEnglish => this["MainWindow.MenuLanguageEnglish"];
        public string LanguageTurkish => this["MainWindow.MenuLanguageTurkish"];
        public string About => this["MainWindow.MenuAbout"];
        public string ContextMenuEdit => this["MainWindow.ContextMenuEdit"];
        public string ContextMenuDelete => this["MainWindow.ContextMenuDelete"];

        #endregion

        #region Search and Filter

        public string SearchGroupBox => this["SearchAndFilter.GroupBox"];
        public string SearchLabel => this["SearchAndFilter.LabelSearch"];
        public string SearchHint => this["SearchAndFilter.HintSearchText"];
        public string CategoryLabel => this["SearchAndFilter.LabelCategory"];
        public string CategoryHint => this["SearchAndFilter.HintCategoryText"];
        public string DatabaseLabel => this["SearchAndFilter.LabelDatabase"];
        public string DatabaseHint => this["SearchAndFilter.HintDatabaseText"];

        /// <summary>
        /// Gets display name for Local database
        /// </summary>
        public string DatabaseLocal => this["SearchAndFilter.DatabaseLocal"];

        /// <summary>
        /// Gets display name for Cloud database
        /// </summary>
        public string DatabaseCloud => this["SearchAndFilter.DatabaseCloud"];

        #endregion

        #region Parts Inventory

        public string InventoryGroupBox => this["PartsInventory.GroupBox"];
        public string ColumnPartNumber => this["PartsInventory.ColumnPartNumber"];
        public string ColumnName => this["PartsInventory.ColumnName"];
        public string ColumnDescription => this["PartsInventory.ColumnDescription"];
        public string ColumnCategory => this["PartsInventory.ColumnCategory"];
        public string ColumnCost => this["PartsInventory.ColumnCost"];
        public string ColumnRetail => this["PartsInventory.ColumnRetail"];
        public string ColumnStock => this["PartsInventory.ColumnStock"];
        public string ColumnLocation => this["PartsInventory.ColumnLocation"];
        public string ColumnSupplier => this["PartsInventory.ColumnSupplier"];
        public string ColumnModel => this["PartsInventory.ColumnModel"];
        public string ColumnReleaseDate => this["PartsInventory.ColumnReleaseDate"];
        public string ColumnImage => this["PartsInventory.ColumnImage"];

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes all UI elements (called by MainWindow when needed)
        /// </summary>
        public void RefreshUI()
        {
            RefreshAllProperties();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all data from the database
        /// </summary>
        public async Task LoadDataAsync()
        {
            var parts = await _dataService.GetAllPartsAsync();
            var categories = await _dataService.GetAllCategoriesAsync();

            // Clear and reload categories
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            // Clear and reload parts
            Parts.Clear();
            foreach (var part in parts)
            {
                Parts.Add(part);
            }

            // Apply current filters
            FilterParts();
        }

        /// <summary>
        /// Filters parts based on search term and category
        /// </summary>
        private void FilterParts()
        {
            // Get all parts from database to avoid losing data
            Task.Run(async () =>
            {
                var allParts = await _dataService.GetAllPartsAsync();

                var filtered = allParts.AsEnumerable();

                // Filter by search term
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var searchTerm = SearchTerm.ToLowerInvariant();
                    filtered = filtered.Where(p =>
                        (p.PartNumber?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (p.Name?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (p.Description?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (p.Supplier?.ToLowerInvariant().Contains(searchTerm) ?? false));
                }

                // Filter by category
                if (SelectedCategory != null)
                {
                    filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
                }

                // Update UI on main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parts.Clear();
                    foreach (var part in filtered)
                    {
                        Parts.Add(part);
                    }
                });
            });
        }

        /// <summary>
        /// Opens the Add Part dialog
        /// </summary>
        private void AddPart(object? parameter)
        {
            var dialog = new PartDialog(Categories, null, _localization);
            if (dialog.ShowDialog() == true && dialog.Part != null)
            {
                _ = AddPartToDatabase(dialog.Part);
            }
        }

        /// <summary>
        /// Adds a part to the database
        /// </summary>
        private async Task AddPartToDatabase(CarPart part)
        {
            var newId = await _dataService.AddPartAsync(part);
            if (newId > 0)
            {
                part.Id = newId;
                // Set the category name for display
                var category = Categories.FirstOrDefault(c => c.Id == part.CategoryId);
                if (category != null)
                {
                    part.CategoryName = category.Name;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parts.Add(part);
                });
            }
        }

        /// <summary>
        /// Opens the Edit Part dialog for the selected part
        /// </summary>
        private void EditPart(object? parameter)
        {
            if (SelectedPart == null) return;

            var dialog = new PartDialog(Categories, SelectedPart, _localization);
            if (dialog.ShowDialog() == true && dialog.Part != null)
            {
                _ = UpdatePartInDatabase(dialog.Part);
            }
        }

        /// <summary>
        /// Determines if a part can be edited
        /// </summary>
        private bool CanEditPart(object? parameter)
        {
            return SelectedPart != null;
        }

        /// <summary>
        /// Updates a part in the database
        /// </summary>
        private async Task UpdatePartInDatabase(CarPart part)
        {
            var success = await _dataService.UpdatePartAsync(part);
            if (success)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var existingPart = Parts.FirstOrDefault(p => p.Id == part.Id);
                    if (existingPart != null)
                    {
                        // Update all properties
                        existingPart.PartNumber = part.PartNumber;
                        existingPart.Name = part.Name;
                        existingPart.Description = part.Description;
                        existingPart.CategoryId = part.CategoryId;
                        existingPart.CostPrice = part.CostPrice;
                        existingPart.RetailPrice = part.RetailPrice;
                        existingPart.StockQuantity = part.StockQuantity;
                        existingPart.Location = part.Location;
                        existingPart.Supplier = part.Supplier;
                        existingPart.ImagePath = part.ImagePath;
                        existingPart.Model = part.Model;
                        existingPart.ReleaseDate = part.ReleaseDate;

                        // Set the category name for display
                        var category = Categories.FirstOrDefault(c => c.Id == part.CategoryId);
                        if (category != null)
                        {
                            existingPart.CategoryName = category.Name;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Deletes the selected part
        /// </summary>
        private void DeletePart(object? parameter)
        {
            if (SelectedPart == null) return;

            var message = this["Messages.DeleteConfirmation"];
            var title = this["Messages.DeleteConfirmationTitle"];
            var formattedMessage = string.Format(message, SelectedPart.Name ?? "this part");

            var result = MessageBox.Show(
                formattedMessage,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _ = DeletePartFromDatabase(SelectedPart);
            }
        }

        /// <summary>
        /// Deletes a part from the database
        /// </summary>
        private async Task DeletePartFromDatabase(CarPart part)
        {
            var success = await _dataService.DeletePartAsync(part.Id);
            if (success)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parts.Remove(part);
                });
            }
        }

        /// <summary>
        /// Determines if a part can be deleted
        /// </summary>
        private bool CanDeletePart(object? parameter)
        {
            return SelectedPart != null;
        }

        /// <summary>
        /// Tests INSERT functionality with dummy data
        /// </summary>
        private async void TestPartInsert(object? parameter)
        {
            try
            {
                var testViewModel = new TestPartInsertViewModel(_dataService);
                var result = await testViewModel.TestInsertAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        result,
                        "Test INSERT Result",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });

                // Refresh parts list to show the new test part if insert succeeded
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Test failed: {ex.Message}",
                        "Test Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        }

        /// <summary>
        /// Changes the current language
        /// </summary>
        private void ChangeLanguage(object? parameter)
        {
            var languageCode = parameter?.ToString() ?? "en";
            _localization.ChangeLanguage(languageCode);
            _settings.CurrentLanguage = languageCode;
            _settings.Save();
            RefreshAllProperties();
        }

        /// <summary>
        /// Changes the database type and refreshes data (async wrapper for command)
        /// </summary>
        private async void ChangeDatabaseAsync(object? parameter)
        {
            await ChangeDatabaseAsync(SelectedDatabase);
        }

        /// <summary>
        /// Changes the database type and refreshes data
        /// </summary>
        private async Task ChangeDatabaseAsync(DatabaseType databaseType)
        {
            try
            {
                var databaseName = databaseType == DatabaseType.Cloud ? DatabaseCloud : DatabaseLocal;
                var switchingMessage = string.Format(this["SearchAndFilter.DatabaseSwitching"], databaseName);
                // Console.WriteLine($"🔄 {switchingMessage}");

                // Switch data service
                _dataService = DataServiceFactory.GetDataService(databaseType);

                // Clear current data
                Parts.Clear();
                Categories.Clear();

                // Reload data from new database
                await LoadDataAsync();

                var successMessage = string.Format(this["SearchAndFilter.DatabaseSwitchSuccess"], databaseName);
                // Console.WriteLine($"✅ {successMessage}");
            }
            catch (Exception ex)
            {
                var databaseName = databaseType == DatabaseType.Cloud ? DatabaseCloud : DatabaseLocal;
                var errorMessage = string.Format(this["SearchAndFilter.DatabaseSwitchError"], databaseName, ex.Message);
                // Console.WriteLine($"❌ {errorMessage}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        errorMessage,
                        this["Messages.ValidationErrorTitle"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        }

        /// <summary>
        /// Refreshes all localized properties when language changes
        /// </summary>
        private void RefreshAllProperties()
        {
            // Menu Items
            OnPropertyChanged(nameof(MenuFile));
            OnPropertyChanged(nameof(MenuAddPart));
            OnPropertyChanged(nameof(MenuExit));
            OnPropertyChanged(nameof(MenuEdit));
            OnPropertyChanged(nameof(MenuDeletePart));
            OnPropertyChanged(nameof(ApiServerStatus));
            OnPropertyChanged(nameof(ApiServerUrl));
            OnPropertyChanged(nameof(MenuHelp));
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(LanguageEnglish));
            OnPropertyChanged(nameof(LanguageTurkish));
            OnPropertyChanged(nameof(About));
            OnPropertyChanged(nameof(ContextMenuEdit));
            OnPropertyChanged(nameof(ContextMenuDelete));

            // Search and Filter
            OnPropertyChanged(nameof(SearchGroupBox));
            OnPropertyChanged(nameof(SearchLabel));
            OnPropertyChanged(nameof(SearchHint));
            OnPropertyChanged(nameof(CategoryLabel));
            OnPropertyChanged(nameof(CategoryHint));

            // Parts Inventory
            OnPropertyChanged(nameof(InventoryGroupBox));
            OnPropertyChanged(nameof(ColumnPartNumber));
            OnPropertyChanged(nameof(ColumnName));
            OnPropertyChanged(nameof(ColumnDescription));
            OnPropertyChanged(nameof(ColumnCategory));
            OnPropertyChanged(nameof(ColumnCost));
            OnPropertyChanged(nameof(ColumnRetail));
            OnPropertyChanged(nameof(ColumnStock));
            OnPropertyChanged(nameof(ColumnLocation));
            OnPropertyChanged(nameof(ColumnSupplier));
            OnPropertyChanged(nameof(ColumnModel));
            OnPropertyChanged(nameof(ColumnReleaseDate));
            OnPropertyChanged(nameof(ColumnImage));

            // Title
            OnPropertyChanged(nameof(Title));

            // CurrentLanguage
            OnPropertyChanged(nameof(CurrentLanguage));
        }

        #endregion
    }
}
