using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CarPartStoreApp.Localization;
using CarPartStoreApp.Models;
using CarPartStoreApp.Services;
using Microsoft.Win32;

namespace CarPartStoreApp.Views
{
    /// <summary>
    /// Dialog window for adding/editing car parts
    /// </summary>
    public partial class PartDialog : Window, INotifyPropertyChanged
    {
        private readonly ILocalizationService _localization;
        private CarPart _part = new CarPart();

        public CarPart Part
        {
            get => _part;
            set
            {
                if (_part is not null)
                {
                    _part.PropertyChanged -= OnPartPropertyChanged;
                }
                _part = value;
                if (_part is not null)
                {
                    _part.PropertyChanged += OnPartPropertyChanged;
                }
                OnPropertyChanged(nameof(Part));
                OnPropertyChanged(nameof(PartImageSource));
            }
        }

        private void OnPartPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CarPart.ImagePath))
            {
                OnPropertyChanged(nameof(PartImageSource));
                OnPropertyChanged(nameof(RemoveImageVisibility));
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<Category> Categories { get; set; } = new System.Collections.ObjectModel.ObservableCollection<Category>();

        /// <summary>
        /// Image source for the preview image control
        /// Converts Part.ImagePath to a BitmapImage for display
        /// </summary>
        public BitmapImage? PartImageSource
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_part.ImagePath))
                    return null;

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    // Allow loading from URLs (Cloudinary) or local files
                    if (_part.ImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        bitmap.UriSource = new Uri(_part.ImagePath, UriKind.Absolute);
                        // Important: Set CreateOptions to IgnoreImageCache for network images
                        // This ensures the image is fetched fresh each time
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.CacheOption = BitmapCacheOption.None;
                        // Set a reasonable decode pixel width to avoid memory issues with large images
                        bitmap.DecodePixelWidth = 800;
                    }
                    else if (File.Exists(_part.ImagePath))
                    {
                        bitmap.UriSource = new Uri(_part.ImagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.DecodePixelWidth = 800;
                    }
                    else
                    {
                        return null;
                    }
                    bitmap.EndInit();
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Visibility of the Remove Image button
        /// Shows the button only when an image is present
        /// </summary>
        public System.Windows.Visibility RemoveImageVisibility
        {
            get
            {
                return string.IsNullOrWhiteSpace(_part.ImagePath)
                    ? System.Windows.Visibility.Collapsed
                    : System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets a localized string by key for XAML binding
        /// Usage: Loc["PartDialog.TitleAdd"]
        /// </summary>
        public string this[string key] => _localization.GetString(key);

        #region Dialog Properties

        public string DialogTitleAdd => this["PartDialog.TitleAdd"];
        public string DialogTitleEdit => this["PartDialog.TitleEdit"];
        public string LabelPartNumber => this["PartDialog.LabelPartNumber"];
        public string LabelName => this["PartDialog.LabelName"];
        public string LabelDescription => this["PartDialog.LabelDescription"];
        public string LabelCategory => this["PartDialog.LabelCategory"];
        public string LabelCostPrice => this["PartDialog.LabelCostPrice"];
        public string LabelRetailPrice => this["PartDialog.LabelRetailPrice"];
        public string LabelStockQuantity => this["PartDialog.LabelStockQuantity"];
        public string LabelLocation => this["PartDialog.LabelLocation"];
        public string LabelSupplier => this["PartDialog.LabelSupplier"];
        public string LabelModel => this["PartDialog.LabelModel"];
        public string LabelReleaseDate => this["PartDialog.LabelReleaseDate"];
        public string LabelImage => this["PartDialog.LabelImage"];
        public string ButtonSave => this["PartDialog.ButtonSave"];
        public string ButtonCancel => this["PartDialog.ButtonCancel"];
        public string ButtonUploadImage => this["PartDialog.ButtonUploadImage"];
        public string ButtonRemoveImage => this["PartDialog.ButtonRemoveImage"];
        public string ToolTipImage => this["PartDialog.ToolTipImage"];

        #endregion

        public PartDialog(System.Collections.ObjectModel.ObservableCollection<Category> categories, CarPart? existingPart = null, ILocalizationService? localization = null)
        {
            InitializeComponent();

            _localization = localization ?? ServiceContainer.Resolve<ILocalizationService>();
            Categories = categories;
            DataContext = this;

            // Enable drag-to-move on the title bar
            TitleBar.MouseLeftButtonDown += (sender, e) => DragMove();

            if (existingPart != null)
            {
                // Editing existing part
                Part = existingPart;
                Title = DialogTitleEdit;
            }
            else
            {
                // Adding new part
                Part = new CarPart();
                Part.CategoryId = categories.FirstOrDefault()?.Id ?? 1;
                Title = DialogTitleAdd;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(_part.PartNumber))
            {
                MessageBox.Show(
                    _localization.GetString(ResourceKeys.MessageValidationErrorPartNumber),
                    _localization.GetString(ResourceKeys.MessageValidationErrorTitle),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_part.Name))
            {
                MessageBox.Show(
                    _localization.GetString(ResourceKeys.MessageValidationErrorName),
                    _localization.GetString(ResourceKeys.MessageValidationErrorTitle),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Image",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var sourceFileName = openFileDialog.FileName;
                if (!string.IsNullOrWhiteSpace(sourceFileName))
                {
                    await UploadImageToCloudinaryAsync(sourceFileName);
                }
            }
        }

        private async void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_part.ImagePath))
            {
                // Check if it's a Cloudinary URL
                var imageService = ServiceContainer.GetService<IImageStorageService>();

                if (imageService != null && _part.ImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Delete from Cloudinary
                    try
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        await imageService.DeleteImageAsync(_part.ImagePath);
                    }
                    catch (Exception ex)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(
                            $"Error deleting image from Cloudinary: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
                else if (File.Exists(_part.ImagePath))
                {
                    // Delete local file
                    try
                    {
                        File.Delete(_part.ImagePath);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(
                            $"Error deleting image: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }

                // Clear the image path
                _part.ImagePath = string.Empty;

                // Explicitly notify that the image source has changed to ensure UI updates
                OnPropertyChanged(nameof(PartImageSource));
            }
        }

        /// <summary>
        /// Uploads an image to Cloudinary or falls back to local storage
        /// </summary>
        private async Task UploadImageToCloudinaryAsync(string sourceFilePath)
        {
            try
            {
                // Check if Cloudinary is configured
                var imageService = ServiceContainer.GetService<IImageStorageService>();
                if (imageService == null)
                {
                    // Fall back to local storage if Cloudinary not configured
                    SaveImageToAppDirectory(sourceFilePath);
                    return;
                }

                // Validate part number before upload
                if (string.IsNullOrWhiteSpace(_part.PartNumber))
                {
                    MessageBox.Show(
                        "Please enter a Part Number before uploading an image.",
                        "Part Number Required",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show loading cursor during upload
                Mouse.OverrideCursor = Cursors.Wait;

                // Upload to Cloudinary
                var imageUrl = await imageService.UploadImageAsync(sourceFilePath, _part.PartNumber);

                // Update the part's image path with Cloudinary URL
                _part.ImagePath = imageUrl;

                // Explicitly notify that the image source has changed to ensure UI updates
                OnPropertyChanged(nameof(PartImageSource));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error uploading image: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Reset cursor
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Saves an image to the local app directory (fallback when Cloudinary is not configured)
        /// </summary>
        private void SaveImageToAppDirectory(string sourceFilePath)
        {
            try
            {
                // Get the images directory
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CarPartStoreApp",
                    "Images");

                // Ensure directory exists
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                // Generate a unique filename based on part number
                var extension = Path.GetExtension(sourceFilePath);
                var fileName = $"{_part.PartNumber}{extension}";
                var destFilePath = Path.Combine(appDataPath, fileName);

                // If file already exists, append a number
                int counter = 1;
                while (File.Exists(destFilePath))
                {
                    fileName = $"{_part.PartNumber}_{counter}{extension}";
                    destFilePath = Path.Combine(appDataPath, fileName);
                    counter++;
                }

                // Copy file to the images directory
                File.Copy(sourceFilePath, destFilePath, true);

                // Update the part's image path
                _part.ImagePath = destFilePath;

                // Explicitly notify that the image source has changed to ensure UI updates
                OnPropertyChanged(nameof(PartImageSource));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving image: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
