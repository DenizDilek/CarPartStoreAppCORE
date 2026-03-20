<<<<<<< Updated upstream
using System;
using System.ComponentModel;
=======
<<<<<<< Updated upstream
>>>>>>> Stashed changes
using System.IO;
using System.Threading.Tasks;
using System.Windows;
<<<<<<< Updated upstream
using System.Windows.Input;
=======
=======
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
>>>>>>> Stashed changes
>>>>>>> Stashed changes
using System.Windows.Media.Imaging;
using CarPartStoreApp.Helpers;
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
        private List<TemporaryImage> _temporaryImages = new List<TemporaryImage>();
        private const int MaxImages = 6;

        public CarPart Part
        {
            get => _part;
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
            set => _part = value;
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            if (e.PropertyName == nameof(CarPart.ImagePath))
            {
                OnPropertyChanged(nameof(PartImageSource));
                OnPropertyChanged(nameof(RemoveImageVisibility));
            }
=======
            if (e.PropertyName == nameof(CarPart.ImagePath) || e.PropertyName == nameof(CarPart.ImagePaths))
            {
                OnPropertyChanged(nameof(PartImageSource));
                OnPropertyChanged(nameof(RemoveImageVisibility));
                OnPropertyChanged(nameof(HasImages));
                OnPropertyChanged(nameof(HasExistingImages));
            }
>>>>>>> Stashed changes
>>>>>>> Stashed changes
        }

        public System.Collections.ObjectModel.ObservableCollection<Category> Categories { get; set; } = new System.Collections.ObjectModel.ObservableCollection<Category>();

        /// <summary>
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
=======
        /// Collection of temporary images waiting to be uploaded
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<TemporaryImage> TemporaryImages { get; set; }
            = new System.Collections.ObjectModel.ObservableCollection<TemporaryImage>();

        /// <summary>
        /// Flag indicating if there are pending images to upload after save
        /// </summary>
        public bool HasPendingImages { get; set; }

        /// <summary>
        /// Returns true if there are any images (temporary or existing)
        /// </summary>
        public bool HasImages => TemporaryImages.Count > 0 || _part.ImagePaths.Count > 0;

        /// <summary>
        /// Returns true if there are existing images on the part being edited
        /// </summary>
        public bool HasExistingImages => _part.ImagePaths.Count > 0;

        /// <summary>
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes
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
        public string LabelStockQuantity => this["PartDialog.LabelStockQuantity"];
        public string LabelLocation => this["PartDialog.LabelLocation"];
<<<<<<< Updated upstream
        public string LabelSupplier => this["PartDialog.LabelSupplier"];
<<<<<<< Updated upstream
        public string LabelModel => this["PartDialog.LabelModel"];
        public string LabelReleaseDate => this["PartDialog.LabelReleaseDate"];
=======
=======
        public string LabelBrand => this["PartDialog.LabelBrand"];
        public string LabelModel => this["PartDialog.LabelModel"];
        public string LabelReleaseDate => this["PartDialog.LabelReleaseDate"];
>>>>>>> Stashed changes
>>>>>>> Stashed changes
        public string LabelImage => this["PartDialog.LabelImage"];
        public string ButtonSave => this["PartDialog.ButtonSave"];
        public string ButtonCancel => this["PartDialog.ButtonCancel"];
        public string ButtonUploadImage => this["PartDialog.ButtonUploadImage"];
        public string ButtonRemoveImage => this["PartDialog.ButtonRemoveImage"];
        public string ToolTipImage => this["PartDialog.ToolTipImage"];
        public string ToolTipBrand => this["PartDialog.ToolTipBrand"];

        #endregion

        public PartDialog(System.Collections.ObjectModel.ObservableCollection<Category> categories, CarPart? existingPart = null, ILocalizationService? localization = null)
        {
            InitializeComponent();

            _localization = localization ?? ServiceContainer.Resolve<ILocalizationService>();
            Categories = categories;
            DataContext = this;

<<<<<<< Updated upstream
            // Enable drag-to-move on the title bar
            TitleBar.MouseLeftButtonDown += (sender, e) => DragMove();

=======
<<<<<<< Updated upstream
=======
            // Clear temporary images
            _temporaryImages.Clear();
            TemporaryImages.Clear();

            // Enable drag-to-move on the title bar
            TitleBar.MouseLeftButtonDown += (sender, e) => DragMove();

>>>>>>> Stashed changes
>>>>>>> Stashed changes
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

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(_part.Name))
            {
                MessageBox.Show(
                    _localization.GetString(ResourceKeys.MessageValidationErrorName),
                    _localization.GetString(ResourceKeys.MessageValidationErrorTitle),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // First save the part to get an ID (for new parts)
            if (_part.Id == 0)
            {
                // Part will be saved by caller after DialogResult = true
                // Set temporary flag that images need upload
                if (_temporaryImages.Count > 0)
                {
                    HasPendingImages = true;
                }
            }
            else if (_temporaryImages.Count > 0)
            {
                // Upload images for existing part
                await UploadImagesAsync(_part.Id);
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
            if (_temporaryImages.Count >= MaxImages)
            {
                MessageBox.Show($"Maximum {MaxImages} images allowed per part.", "Limit Reached",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Images",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files (*.*)|*.*",
                Multiselect = true  // Allow multiple file selection
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
<<<<<<< Updated upstream
                    await UploadImageToCloudinaryAsync(sourceFileName);
=======
<<<<<<< Updated upstream
                    SaveImageToAppDirectory(sourceFileName);
>>>>>>> Stashed changes
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
=======
                    if (_temporaryImages.Count >= MaxImages) break;

                    try
                    {
                        var processedData = ImageProcessingHelper.ProcessImage(fileName);
                        var tempImage = new TemporaryImage(fileName, processedData);
                        _temporaryImages.Add(tempImage);
                        TemporaryImages.Add(tempImage);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing image: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                OnPropertyChanged(nameof(TemporaryImages));
                OnPropertyChanged(nameof(HasImages));
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not TemporaryImage tempImage) return;

            _temporaryImages.Remove(tempImage);
            TemporaryImages.Remove(tempImage);

            OnPropertyChanged(nameof(TemporaryImages));
            OnPropertyChanged(nameof(HasImages));
        }

        /// <summary>
        /// Removes an existing image from the part's image paths
        /// </summary>
        private async void RemoveExistingImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string imagePath) return;

            // Check if it's a Cloudinary URL
            var imageService = ServiceContainer.GetService<IImageStorageService>();

            if (imageService != null && imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Delete from Cloudinary
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    await imageService.DeleteImageAsync(imagePath);
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
            else if (File.Exists(imagePath))
            {
                // Delete local file
                try
                {
                    File.Delete(imagePath);
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

            // Remove from the part's image paths
            _part.ImagePaths.Remove(imagePath);

            // Explicitly notify that the image source has changed to ensure UI updates
            OnPropertyChanged(nameof(PartImageSource));
            OnPropertyChanged(nameof(HasImages));
            OnPropertyChanged(nameof(HasExistingImages));
        }

        /// <summary>
        /// Uploads all temporary images to storage (Cloudinary or local)
        /// </summary>
        private async Task UploadImagesAsync(int partId)
        {
            var imageService = ServiceContainer.GetService<IImageStorageService>();
            var uploadedPaths = new List<string>();

            // Keep existing images if any
            if (_part.ImagePaths.Count > 0)
            {
                uploadedPaths.AddRange(_part.ImagePaths);
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                for (int i = 0; i < _temporaryImages.Count; i++)
                {
                    string imageUrl;

                    if (imageService != null)
>>>>>>> Stashed changes
                    {
                        // Upload to Cloudinary
                        var publicId = $"{partId}_{uploadedPaths.Count}";
                        imageUrl = await imageService.UploadImageBytesAsync(_temporaryImages[i].ProcessedData, publicId);
                    }
                    else
                    {
<<<<<<< Updated upstream
                        MessageBox.Show(
                            $"Error deleting image: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
<<<<<<< Updated upstream
                        return;
=======
=======
                        // Save locally
                        var appDataPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "CarPartStoreApp",
                            "Images");

                        if (!Directory.Exists(appDataPath))
                            Directory.CreateDirectory(appDataPath);

                        var fileName = $"{partId}_{uploadedPaths.Count}.jpg";
                        var destFilePath = Path.Combine(appDataPath, fileName);
                        await File.WriteAllBytesAsync(destFilePath, _temporaryImages[i].ProcessedData);
                        imageUrl = destFilePath;
>>>>>>> Stashed changes
>>>>>>> Stashed changes
                    }

                    uploadedPaths.Add(imageUrl);
                }

                // Update part with all image paths
                _part.ImagePaths = uploadedPaths;

<<<<<<< Updated upstream
                // Explicitly notify that the image source has changed to ensure UI updates
                OnPropertyChanged(nameof(PartImageSource));
            }
        }

=======
<<<<<<< Updated upstream
                // Refresh UI by resetting DataContext
                var temp = DataContext;
                DataContext = null;
                DataContext = temp;
            }
        }

=======
                // Clear temporary images
                _temporaryImages.Clear();
                TemporaryImages.Clear();
                HasPendingImages = false;

                OnPropertyChanged(nameof(TemporaryImages));
                OnPropertyChanged(nameof(HasImages));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading images: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
                // Validate part number before upload
                if (string.IsNullOrWhiteSpace(_part.PartNumber))
                {
                    MessageBox.Show(
                        "Please enter a Part Number before uploading an image.",
                        "Part Number Required",
=======
                // Validate part ID before upload
                if (_part.Id == 0)
                {
                    MessageBox.Show(
                        "Please save the part first before uploading an image.",
                        "Part ID Required",
>>>>>>> Stashed changes
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show loading cursor during upload
                Mouse.OverrideCursor = Cursors.Wait;

<<<<<<< Updated upstream
                // Upload to Cloudinary
                var imageUrl = await imageService.UploadImageAsync(sourceFilePath, _part.PartNumber);
=======
                // Upload to Cloudinary using part ID
                var imageUrl = await imageService.UploadImageAsync(sourceFilePath, _part.Id.ToString());
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes
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

                // Validate part ID before saving
                if (_part.Id == 0)
                {
                    MessageBox.Show(
                        "Please save the part first before uploading an image.",
                        "Part ID Required",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Generate a unique filename based on part ID
                var extension = Path.GetExtension(sourceFilePath);
                var fileName = $"part_{_part.Id}{extension}";
                var destFilePath = Path.Combine(appDataPath, fileName);

                // If file already exists, append a number
                int counter = 1;
                while (File.Exists(destFilePath))
                {
                    fileName = $"part_{_part.Id}_{counter}{extension}";
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
