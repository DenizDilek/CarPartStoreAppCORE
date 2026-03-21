using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ObservableCollection<TemporaryImageData> _partImages = new ObservableCollection<TemporaryImageData>();
        private const int MaxImages = 6;

        public CarPart Part
        {
            get => _part;
            set => _part = value;
        }

        public ObservableCollection<TemporaryImageData> PartImages => _partImages;

        public System.Collections.ObjectModel.ObservableCollection<Category> Categories { get; set; } = new System.Collections.ObjectModel.ObservableCollection<Category>();

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
        public string LabelStockQuantity => this["PartDialog.LabelStockQuantity"];
        public string LabelLocation => this["PartDialog.LabelLocation"];
        public string LabelBrand => this["PartDialog.LabelBrand"];
        public string LabelModel => this["PartDialog.LabelModel"];
        public string LabelReleaseDate => this["PartDialog.LabelReleaseDate"];
        public string LabelImage => this["PartDialog.LabelImage"];
        public string ButtonSave => this["PartDialog.ButtonSave"];
        public string ButtonCancel => this["PartDialog.ButtonCancel"];
        public string ButtonUploadImage => this["PartDialog.ButtonUploadImage"];
        public string ButtonRemoveImage => this["PartDialog.ButtonRemoveImage"];
        public string ButtonRemoveAllImages => this["PartDialog.ButtonRemoveAllImages"];
        public string ToolTipPartNumber => this["PartDialog.ToolTipPartNumber"];
        public string ToolTipName => this["PartDialog.ToolTipName"];
        public string ToolTipDescription => this["PartDialog.ToolTipDescription"];
        public string ToolTipCostPrice => this["PartDialog.ToolTipCostPrice"];
        public string ToolTipStockQuantity => this["PartDialog.ToolTipStockQuantity"];
        public string ToolTipLocation => this["PartDialog.ToolTipLocation"];
        public string ToolTipBrand => this["PartDialog.ToolTipBrand"];
        public string ToolTipModel => this["PartDialog.ToolTipModel"];
        public string ToolTipReleaseDate => this["PartDialog.ToolTipReleaseDate"];
        public string ToolTipImage => this["PartDialog.ToolTipImage"];

        public Visibility RemoveImagesVisibility => _partImages.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

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

                // Load existing images into PartImages collection
                LoadExistingImages();
            }
            else
            {
                // Adding new part
                Part = new CarPart();
                Part.CategoryId = categories.FirstOrDefault()?.Id ?? 1;
                Title = DialogTitleAdd;
            }
        }

        /// <summary>
        /// Loads existing images from the part's ImagePath into the PartImages collection
        /// </summary>
        private void LoadExistingImages()
        {
            _partImages.Clear();

            if (string.IsNullOrWhiteSpace(_part.ImagePath))
                return;

            // Split the ImagePath by spaces to get individual URLs/paths
            var imagePaths = _part.ImagePath.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in imagePaths)
            {
                var imageData = new TemporaryImageData
                {
                    ImageSource = LoadImageSource(path),
                    ImagePath = path,
                    RemoveButtonVisibility = Visibility.Visible
                };
                _partImages.Add(imageData);
            }

            OnPropertyChanged(nameof(RemoveImagesVisibility));
        }

        /// <summary>
        /// Loads a BitmapImage from a file path or URL
        /// </summary>
        private static BitmapImage? LoadImageSource(string path)
        {
            try
            {
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    return new BitmapImage(new Uri(path));
                }
                else if (File.Exists(path))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch
            {
                // Return null if image can't be loaded
            }
            return null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields (PartNumber is now optional)
            if (string.IsNullOrWhiteSpace(_part.Name))
            {
                MessageBox.Show(
                    _localization.GetString(ResourceKeys.MessageValidationErrorName),
                    _localization.GetString(ResourceKeys.MessageValidationErrorTitle),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Combine all image paths into the Part's ImagePath field
            var imagePaths = _partImages
                .Where(img => !string.IsNullOrWhiteSpace(img.ImagePath))
                .Select(img => img.ImagePath);
            _part.ImagePath = string.Join(" ", imagePaths);

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
            // Check if we've reached the maximum number of images
            if (_partImages.Count >= MaxImages)
            {
                MessageBox.Show(
                    $"Maximum of {MaxImages} images allowed per part.",
                    "Image Limit Reached",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Images",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFiles = openFileDialog.FileNames;
                var availableSlots = MaxImages - _partImages.Count;
                var filesToAdd = selectedFiles.Take(availableSlots).ToList();

                if (selectedFiles.Length > availableSlots)
                {
                    MessageBox.Show(
                        $"Only {availableSlots} more image(s) can be added (max {MaxImages} total).\n{selectedFiles.Length - availableSlots} file(s) were not added.",
                        "Image Limit",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                foreach (var sourceFilePath in filesToAdd)
                {
                    if (!string.IsNullOrWhiteSpace(sourceFilePath))
                    {
                        await AddImageToPartAsync(sourceFilePath);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an image to the PartImages collection (temporary until Save is clicked)
        /// Images are processed (resized/converted to JPG) to be under 500KB
        /// </summary>
        private async Task AddImageToPartAsync(string sourceFilePath)
        {
            try
            {
                // Process the image to meet size/format requirements (max 500KB, JPG format)
                byte[] processedImageBytes = await Task.Run(() => ImageProcessingHelper.ProcessImage(sourceFilePath));

                // Save processed image to a temporary file for later upload
                string tempDirectory = Path.Combine(Path.GetTempPath(), "CarPartStoreApp", "Images");
                Directory.CreateDirectory(tempDirectory);
                string tempFileName = $"temp_{Guid.NewGuid():N}.jpg";
                string tempFilePath = Path.Combine(tempDirectory, tempFileName);

                await File.WriteAllBytesAsync(tempFilePath, processedImageBytes);

                // Load the processed image for display
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(tempFilePath);
                bitmap.EndInit();
                bitmap.Freeze(); // Freeze for better performance

                var imageData = new TemporaryImageData
                {
                    ImageSource = bitmap,
                    LocalFilePath = tempFilePath, // Store processed temp file path for later upload
                    OriginalFilePath = sourceFilePath, // Keep track of original file
                    ImagePath = tempFilePath, // Temporary display path
                    ProcessedImageBytes = processedImageBytes, // Store bytes for potential direct upload
                    RemoveButtonVisibility = Visibility.Visible
                };

                _partImages.Add(imageData);
                OnPropertyChanged(nameof(RemoveImagesVisibility));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error processing image: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            // Get the image data from the button's CommandParameter
            if (sender is System.Windows.Controls.Button button && button.CommandParameter is TemporaryImageData imageData)
            {
                await RemoveImageAsync(imageData);
            }
        }

        private async void RemoveAllImages_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to remove all images?",
                "Confirm Remove All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            // Delete all images from Cloudinary if they're URLs
            var imageService = ServiceContainer.GetService<IImageStorageService>();
            var errors = new System.Collections.Generic.List<string>();

            foreach (var imageData in _partImages.ToList())
            {
                if (imageService != null && !string.IsNullOrWhiteSpace(imageData.ImagePath) &&
                    imageData.ImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        await imageService.DeleteImageAsync(imageData.ImagePath);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to delete {imageData.ImagePath}: {ex.Message}");
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(imageData.LocalFilePath) && File.Exists(imageData.LocalFilePath))
                {
                    try
                    {
                        File.Delete(imageData.LocalFilePath);
                    }
                    catch
                    {
                        // Silently ignore errors deleting local temp files
                    }
                }
            }

            _partImages.Clear();
            OnPropertyChanged(nameof(RemoveImagesVisibility));

            if (errors.Count > 0)
            {
                MessageBox.Show(
                    $"Some images could not be deleted:\n{string.Join("\n", errors)}",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Removes a single image from the PartImages collection
        /// </summary>
        private async Task RemoveImageAsync(TemporaryImageData imageData)
        {
            // Check if it's a Cloudinary URL
            var imageService = ServiceContainer.GetService<IImageStorageService>();

            if (imageService != null && !string.IsNullOrWhiteSpace(imageData.ImagePath) &&
                imageData.ImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Delete from Cloudinary
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    await imageService.DeleteImageAsync(imageData.ImagePath);
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
            else if (!string.IsNullOrWhiteSpace(imageData.LocalFilePath) && File.Exists(imageData.LocalFilePath))
            {
                // Delete local file
                try
                {
                    File.Delete(imageData.LocalFilePath);
                }
                catch
                {
                    // Silently ignore errors deleting local files
                }
            }

            // Remove from collection
            _partImages.Remove(imageData);
            OnPropertyChanged(nameof(RemoveImagesVisibility));
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
    }

    /// <summary>
    /// Represents temporary image data before it's uploaded to Cloudinary
    /// </summary>
    public class TemporaryImageData
    {
        public BitmapImage? ImageSource { get; set; }
        public string? ImagePath { get; set; }
        public string? LocalFilePath { get; set; }
        public string? OriginalFilePath { get; set; }
        public byte[]? ProcessedImageBytes { get; set; }
        public Visibility RemoveButtonVisibility { get; set; } = Visibility.Visible;
    }
}
