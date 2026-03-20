using System;
using System.Collections.ObjectModel;
using System.IO;
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
<<<<<<< Updated upstream
        private DatabaseType _selectedDatabase;
        private readonly Services.EmbeddedApiServer? _apiServer;
=======
<<<<<<< Updated upstream
        private readonly Services.EmbeddedApiServer _apiServer;
=======
        private DatabaseType _selectedDatabase;
        private string? _selectedBrand;
        private int? _selectedYear;
        private string? _selectedModel;
        private readonly Services.EmbeddedApiServer? _apiServer;
>>>>>>> Stashed changes
>>>>>>> Stashed changes

        public MainWindowViewModel(IDataService dataService, AppSettings settings, ILocalizationService localization, EmbeddedApiServer? apiServer = null)
        {
            _dataService = dataService;
            _settings = settings;
            _localization = localization;
            _apiServer = apiServer ?? ServiceContainer.GetService<EmbeddedApiServer>();
            _selectedDatabase = settings.SelectedDatabase;

            Parts = new ObservableCollection<CarPart>();
            Categories = new ObservableCollection<Category>();
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
=======
            Brands = new ObservableCollection<string>();
            Years = new ObservableCollection<int>();
            Models = new ObservableCollection<string>();
>>>>>>> Stashed changes
            DatabaseTypes = new ObservableCollection<DatabaseType>
            {
                DatabaseType.Local,
                DatabaseType.Cloud
            };
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes

            AddPartCommand = new RelayCommand(AddPart);
            EditPartCommand = new RelayCommand(EditPart, CanEditPart);
            DeletePartCommand = new RelayCommand(DeletePart, CanDeletePart);
            DisplayPartCommand = new RelayCommand(DisplayPart, CanDisplayPart);
            ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
<<<<<<< Updated upstream
            ChangeDatabaseCommand = new RelayCommand(ChangeDatabaseAsync);
            TestPartCommand = new RelayCommand(TestPartInsert);
=======
<<<<<<< Updated upstream
=======
            ChangeDatabaseCommand = new RelayCommand(ChangeDatabaseAsync);
            TestPartCommand = new RelayCommand(TestPartInsert);
            SearchCommand = new RelayCommand(ExecuteSearch);
            ResetCommand = new RelayCommand(ExecuteReset);
>>>>>>> Stashed changes
>>>>>>> Stashed changes
        }

        #region Collections

        public ObservableCollection<CarPart> Parts { get; }
        public ObservableCollection<Category> Categories { get; }
<<<<<<< Updated upstream
        public ObservableCollection<DatabaseType> DatabaseTypes { get; }
=======
<<<<<<< Updated upstream
=======
        public ObservableCollection<string> Brands { get; }
        public ObservableCollection<int> Years { get; }
        public ObservableCollection<string> Models { get; }
        public ObservableCollection<DatabaseType> DatabaseTypes { get; }
>>>>>>> Stashed changes
>>>>>>> Stashed changes

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

        public string? SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (SetProperty(ref _selectedBrand, value))
                {
                    FilterParts();
                }
            }
        }

        public int? SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    FilterParts();
                }
            }
        }

        public string? SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (SetProperty(ref _selectedModel, value))
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
        public ICommand DisplayPartCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
<<<<<<< Updated upstream
        public ICommand ChangeDatabaseCommand { get; }
        public ICommand TestPartCommand { get; }
=======
<<<<<<< Updated upstream
=======
        public ICommand ChangeDatabaseCommand { get; }
        public ICommand TestPartCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetCommand { get; }
>>>>>>> Stashed changes
>>>>>>> Stashed changes

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
        public string ContextMenuDisplay => this["MainWindow.ContextMenuDisplay"];
        public string ContextMenuDelete => this["MainWindow.ContextMenuDelete"];

        #endregion

        #region Search and Filter

        public string SearchGroupBox => this["SearchAndFilter.GroupBox"];
        public string SearchLabel => this["SearchAndFilter.LabelSearch"];
        public string SearchHint => this["SearchAndFilter.HintSearchText"];
        public string CategoryLabel => this["SearchAndFilter.LabelCategory"];
        public string CategoryHint => this["SearchAndFilter.HintCategoryText"];
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
=======
        public string BrandLabel => this["SearchAndFilter.LabelBrand"];
        public string BrandHint => this["SearchAndFilter.HintBrandText"];
        public string YearLabel => this["SearchAndFilter.LabelYear"];
        public string YearHint => this["SearchAndFilter.HintYearText"];
        public string ModelLabel => this["SearchAndFilter.LabelModel"];
        public string ModelHint => this["SearchAndFilter.HintModelText"];
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes

        /// <summary>
        /// Gets the Search button label
        /// </summary>
        public string ButtonSearch => this["SearchAndFilter.ButtonSearch"];

        /// <summary>
        /// Gets the Reset button label
        /// </summary>
        public string ButtonReset => this["SearchAndFilter.ButtonReset"];

        /// <summary>
        /// Gets the Search button tooltip
        /// </summary>
        public string ToolTipSearch => this["SearchAndFilter.ToolTipSearch"];

        /// <summary>
        /// Gets the Reset button tooltip
        /// </summary>
        public string ToolTipReset => this["SearchAndFilter.ToolTipReset"];
>>>>>>> Stashed changes

        #endregion

        #region Parts Inventory

        public string InventoryGroupBox => this["PartsInventory.GroupBox"];
        public string ColumnPartNumber => this["PartsInventory.ColumnPartNumber"];
        public string ColumnName => this["PartsInventory.ColumnName"];
        public string ColumnDescription => this["PartsInventory.ColumnDescription"];
        public string ColumnCategory => this["PartsInventory.ColumnCategory"];
        public string ColumnCost => this["PartsInventory.ColumnCost"];
        public string ColumnStock => this["PartsInventory.ColumnStock"];
        public string ColumnLocation => this["PartsInventory.ColumnLocation"];
<<<<<<< Updated upstream
        public string ColumnSupplier => this["PartsInventory.ColumnSupplier"];
<<<<<<< Updated upstream
        public string ColumnModel => this["PartsInventory.ColumnModel"];
        public string ColumnReleaseDate => this["PartsInventory.ColumnReleaseDate"];
=======
=======
        public string ColumnBrand => this["PartsInventory.ColumnBrand"];
        public string ColumnModel => this["PartsInventory.ColumnModel"];
        public string ColumnReleaseDate => this["PartsInventory.ColumnReleaseDate"];
>>>>>>> Stashed changes
>>>>>>> Stashed changes
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

            // Populate unique brands
            Brands.Clear();
            var uniqueBrands = parts
                .Where(p => !string.IsNullOrWhiteSpace(p.Brand))
                .Select(p => p.Brand!)
                .Distinct()
                .OrderBy(b => b);
            foreach (var brand in uniqueBrands)
            {
                Brands.Add(brand);
            }

            // Populate unique years
            Years.Clear();
            var uniqueYears = parts
                .Where(p => p.ReleaseDate.HasValue)
                .Select(p => p.ReleaseDate!.Value)
                .Distinct()
                .OrderByDescending(y => y);
            foreach (var year in uniqueYears)
            {
                Years.Add(year);
            }

            // Populate unique models
            Models.Clear();
            var uniqueModels = parts
                .Where(p => !string.IsNullOrWhiteSpace(p.Model))
                .Select(p => p.Model!)
                .Distinct()
                .OrderBy(m => m);
            foreach (var model in uniqueModels)
            {
                Models.Add(model);
            }

            // Apply current filters
            FilterParts();
        }

        /// <summary>
        /// Filters parts based on search term, category, brand, year, and model
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
                        (p.Brand?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (p.Model?.ToLowerInvariant().Contains(searchTerm) ?? false));
                }

                // Filter by category
                if (SelectedCategory != null)
                {
                    filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
                }

                // Filter by brand
                if (!string.IsNullOrWhiteSpace(SelectedBrand))
                {
                    filtered = filtered.Where(p => p.Brand == SelectedBrand);
                }

                // Filter by year
                if (SelectedYear.HasValue)
                {
                    filtered = filtered.Where(p => p.ReleaseDate == SelectedYear.Value);
                }

                // Filter by model
                if (!string.IsNullOrWhiteSpace(SelectedModel))
                {
                    filtered = filtered.Where(p => p.Model == SelectedModel);
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
        /// Executes the search with current filter values
        /// Triggered by the Search button
        /// </summary>
        private void ExecuteSearch(object? parameter)
        {
            FilterParts();
        }

        /// <summary>
        /// Resets all filters and shows all parts
        /// Triggered by the Reset button
        /// </summary>
        private void ExecuteReset(object? parameter)
        {
            // Clear search term
            SearchTerm = string.Empty;

            // Clear category filter
            SelectedCategory = null;

            // Clear brand filter
            SelectedBrand = null;

            // Clear year filter
            SelectedYear = null;

            // Clear model filter
            SelectedModel = null;

            // Reload all parts from database
            Task.Run(async () =>
            {
                var allParts = await _dataService.GetAllPartsAsync();

                // Update UI on main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parts.Clear();
                    foreach (var part in allParts)
                    {
                        Parts.Add(part);
                    }
                });
            });
        }

        /// <summary>
        /// Handles pending image uploads after a part is saved
        /// </summary>
        private async Task HandlePendingImageUploadsAsync(PartDialog dialog)
        {
            if (dialog.HasPendingImages && dialog.Part.Id > 0 && dialog.TemporaryImages.Count > 0)
            {
                try
                {
                    var imageService = ServiceContainer.GetService<IImageStorageService>();
                    var uploadedPaths = new List<string>();

                    // Get existing images
                    if (dialog.Part.ImagePaths.Count > 0)
                    {
                        uploadedPaths.AddRange(dialog.Part.ImagePaths);
                    }

                    // Upload temporary images
                    for (int i = 0; i < dialog.TemporaryImages.Count; i++)
                    {
                        string imageUrl;

                        if (imageService != null)
                        {
                            // Upload to Cloudinary
                            var publicId = $"{dialog.Part.Id}_{uploadedPaths.Count}";
                            imageUrl = await imageService.UploadImageBytesAsync(dialog.TemporaryImages[i].ProcessedData, publicId);
                        }
                        else
                        {
                            // Save locally
                            var appDataPath = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "CarPartStoreApp",
                                "Images");

                            if (!Directory.Exists(appDataPath))
                                Directory.CreateDirectory(appDataPath);

                            var fileName = $"{dialog.Part.Id}_{uploadedPaths.Count}.jpg";
                            var destFilePath = Path.Combine(appDataPath, fileName);
                            await File.WriteAllBytesAsync(destFilePath, dialog.TemporaryImages[i].ProcessedData);
                            imageUrl = destFilePath;
                        }

                        uploadedPaths.Add(imageUrl);
                    }

                    // Update part with all image paths
                    dialog.Part.ImagePaths = uploadedPaths;

                    // Save the updated part to database
                    await _dataService.UpdatePartAsync(dialog.Part);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error uploading images: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Opens the Add Part dialog
        /// </summary>
        private async void AddPart(object? parameter)
        {
            var dialog = new PartDialog(Categories, null, _localization);
            if (dialog.ShowDialog() == true && dialog.Part != null)
            {
                // Save part to database first
                var newId = await _dataService.AddPartAsync(dialog.Part);
                dialog.Part.Id = newId;

                // Handle pending image uploads
                await HandlePendingImageUploadsAsync(dialog);

                // Refresh parts list
                await LoadDataAsync();
            }
        }

        /// <summary>
        /// Opens the Edit Part dialog for the selected part
        /// </summary>
        private async void EditPart(object? parameter)
        {
            if (SelectedPart == null) return;

            var dialog = new PartDialog(Categories, SelectedPart, _localization);
            if (dialog.ShowDialog() == true && dialog.Part != null)
            {
                await UpdatePartInDatabase(dialog.Part);
                // Refresh data to show updated images
                await LoadDataAsync();
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
                await Application.Current.Dispatcher.InvokeAsync(() =>
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
                        existingPart.Brand = part.Brand;
                        existingPart.StockQuantity = part.StockQuantity;
                        existingPart.Location = part.Location;
                        existingPart.ImagePath = part.ImagePath;
<<<<<<< Updated upstream
                        existingPart.Model = part.Model;
                        existingPart.ReleaseDate = part.ReleaseDate;
=======
<<<<<<< Updated upstream
=======
                        existingPart.ImagePaths = part.ImagePaths;
                        existingPart.Model = part.Model;
                        existingPart.ReleaseDate = part.ReleaseDate;
>>>>>>> Stashed changes
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
        /// Deletes a part from the database and its image from Cloudinary/local storage
        /// </summary>
        private async Task DeletePartFromDatabase(CarPart part)
        {
            // Delete image from Cloudinary if it's a URL
            var imageService = ServiceContainer.GetService<IImageStorageService>();
            if (imageService != null && !string.IsNullOrEmpty(part.ImagePath) && part.ImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await imageService.DeleteImageAsync(part.ImagePath);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with deletion
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            $"Warning: Could not delete image from Cloudinary: {ex.Message}\n\nThe part will still be deleted from the database.",
=======
<<<<<<< Updated upstream
        /// Deletes a part from the database
        /// </summary>
        private async Task DeletePartFromDatabase(CarPart part)
        {
=======
        /// Deletes a part from the database and ALL images from Cloudinary/local storage
        /// </summary>
        private async Task DeletePartFromDatabase(CarPart part)
        {
            var imageService = ServiceContainer.GetService<IImageStorageService>();

            // Delete ALL images from Cloudinary or local storage
            if (part.ImagePaths != null && part.ImagePaths.Count > 0)
            {
                var deletionErrors = new List<string>();

                foreach (var imagePath in part.ImagePaths)
                {
                    if (string.IsNullOrWhiteSpace(imagePath))
                        continue;

                    try
                    {
                        // Delete Cloudinary images (URLs starting with http)
                        if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            if (imageService != null)
                            {
                                await imageService.DeleteImageAsync(imagePath);
                            }
                        }
                        // Delete local files
                        else if (File.Exists(imagePath))
                        {
                            File.Delete(imagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Collect errors but continue deleting other images
                        deletionErrors.Add($"- {imagePath}: {ex.Message}");
                    }
                }

                // Show warning if any images failed to delete
                if (deletionErrors.Count > 0)
                {
                    var errorMessage = string.Join("\n", deletionErrors);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            $"Warning: Some images could not be deleted:\n\n{errorMessage}\n\nThe part will still be deleted from the database.",
>>>>>>> Stashed changes
                            "Image Deletion Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    });
                }
            }
<<<<<<< Updated upstream
            else if (!string.IsNullOrEmpty(part.ImagePath) && File.Exists(part.ImagePath))
            {
                // Delete local file
                try
                {
                    File.Delete(part.ImagePath);
                }
                catch
                {
                    // Silently ignore errors deleting local files
                }
            }

            // Delete part from database
=======

            // Delete part from database
>>>>>>> Stashed changes
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
=======
        /// Opens the Part Detail window to display the selected part in read-only mode
        /// </summary>
        private void DisplayPart(object? parameter)
        {
            if (SelectedPart == null) return;

            var window = new PartDetailWindow(SelectedPart, _localization);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        /// <summary>
        /// Determines if a part can be displayed
        /// </summary>
        private bool CanDisplayPart(object? parameter)
        {
            return SelectedPart != null;
        }

        /// <summary>
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes
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
            OnPropertyChanged(nameof(ContextMenuDisplay));
            OnPropertyChanged(nameof(ContextMenuDelete));

            // Search and Filter
            OnPropertyChanged(nameof(SearchGroupBox));
            OnPropertyChanged(nameof(SearchLabel));
            OnPropertyChanged(nameof(SearchHint));
            OnPropertyChanged(nameof(CategoryLabel));
            OnPropertyChanged(nameof(CategoryHint));
            OnPropertyChanged(nameof(BrandLabel));
            OnPropertyChanged(nameof(BrandHint));
            OnPropertyChanged(nameof(YearLabel));
            OnPropertyChanged(nameof(YearHint));
            OnPropertyChanged(nameof(ModelLabel));
            OnPropertyChanged(nameof(ModelHint));
            OnPropertyChanged(nameof(ButtonSearch));
            OnPropertyChanged(nameof(ButtonReset));
            OnPropertyChanged(nameof(ToolTipSearch));
            OnPropertyChanged(nameof(ToolTipReset));

            // Parts Inventory
            OnPropertyChanged(nameof(InventoryGroupBox));
            OnPropertyChanged(nameof(ColumnPartNumber));
            OnPropertyChanged(nameof(ColumnName));
            OnPropertyChanged(nameof(ColumnDescription));
            OnPropertyChanged(nameof(ColumnCategory));
            OnPropertyChanged(nameof(ColumnCost));
            OnPropertyChanged(nameof(ColumnStock));
            OnPropertyChanged(nameof(ColumnLocation));
<<<<<<< Updated upstream
            OnPropertyChanged(nameof(ColumnSupplier));
<<<<<<< Updated upstream
            OnPropertyChanged(nameof(ColumnModel));
            OnPropertyChanged(nameof(ColumnReleaseDate));
            OnPropertyChanged(nameof(ColumnImage));
=======
=======
            OnPropertyChanged(nameof(ColumnBrand));
            OnPropertyChanged(nameof(ColumnModel));
            OnPropertyChanged(nameof(ColumnReleaseDate));
            OnPropertyChanged(nameof(ColumnImage));
>>>>>>> Stashed changes
>>>>>>> Stashed changes

            // Title
            OnPropertyChanged(nameof(Title));

            // CurrentLanguage
            OnPropertyChanged(nameof(CurrentLanguage));
        }

        #endregion
    }
}
