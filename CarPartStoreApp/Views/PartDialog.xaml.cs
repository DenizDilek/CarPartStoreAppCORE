using System.IO;
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
    public partial class PartDialog : Window
    {
        private readonly ILocalizationService _localization;
        private CarPart _part = new CarPart();

        public CarPart Part
        {
            get => _part;
            set => _part = value;
        }

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

        private void UploadImage_Click(object sender, RoutedEventArgs e)
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
                    SaveImageToAppDirectory(sourceFileName);
                }
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_part.ImagePath))
            {
                // Delete the existing image file
                if (File.Exists(_part.ImagePath))
                {
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
                    }
                }

                // Clear the image path
                _part.ImagePath = string.Empty;

                // Refresh UI by resetting DataContext
                var temp = DataContext;
                DataContext = null;
                DataContext = temp;
            }
        }

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

                // Refresh UI by resetting DataContext
                var temp = DataContext;
                DataContext = null;
                DataContext = temp;

                // Clear remaining OnPropertyChanged call below
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
    }
}
