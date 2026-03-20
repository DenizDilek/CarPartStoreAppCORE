using CarPartStoreApp.Localization;
using CarPartStoreApp.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CarPartStoreApp.ViewModels
{
    /// <summary>
    /// ViewModel for PartDetailWindow - displays part details in read-only mode
    /// </summary>
    public class PartDetailViewModel : ObservableObject
    {
        private readonly ILocalizationService _localization;
        private PartImageItem? _selectedImage;

        public PartDetailViewModel(CarPart part, ILocalizationService localization)
        {
            _localization = localization;
            Part = part;

            // Load images
            LoadImages();

            // Set first image as selected
            if (PartImages.Count > 0)
            {
                PartImages[0].IsSelected = true;
                SelectedImage = PartImages[0];
            }

            // Initialize command
            SelectImageCommand = new RelayCommand(SelectImage);
        }

        public CarPart Part { get; }

        public ObservableCollection<PartImageItem> PartImages { get; set; }
            = new ObservableCollection<PartImageItem>();

        /// <summary>
        /// Gets or sets the currently selected image for display in the main viewer
        /// </summary>
        public PartImageItem? SelectedImage
        {
            get => _selectedImage;
            set
            {
                _selectedImage = value;
                OnPropertyChanged(nameof(SelectedImage));
            }
        }

        /// <summary>
        /// Command to select an image for display in the main viewer
        /// </summary>
        public ICommand SelectImageCommand { get; }

        private void SelectImage(object? parameter)
        {
            if (parameter is PartImageItem imageItem)
            {
                // Deselect all images
                foreach (var img in PartImages)
                {
                    img.IsSelected = false;
                }

                // Select the clicked image
                imageItem.IsSelected = true;
                SelectedImage = imageItem;
            }
        }

        private void LoadImages()
        {
            PartImages.Clear();

            if (Part.ImagePaths.Count > 0)
            {
                for (int i = 0; i < Part.ImagePaths.Count; i++)
                {
                    PartImages.Add(new PartImageItem(Part.ImagePaths[i], i, Part.ImagePaths.Count));
                }
            }
        }

        // Localized properties
        public string WindowTitle => Part?.Name ?? "Part Details";
        public string LabelPartNumber => _localization.GetString("PartDialog.LabelPartNumber");
        public string LabelName => _localization.GetString("PartDialog.LabelName");
        public string LabelDescription => _localization.GetString("PartDialog.LabelDescription");
        public string LabelCategory => _localization.GetString("PartDialog.LabelCategory");
        public string LabelCostPrice => _localization.GetString("PartDialog.LabelCostPrice");
        public string LabelStockQuantity => _localization.GetString("PartDialog.LabelStockQuantity");
        public string LabelLocation => _localization.GetString("PartDialog.LabelLocation");
        public string LabelBrand => _localization.GetString("PartDialog.LabelBrand");
        public string LabelModel => _localization.GetString("PartDialog.LabelModel");
        public string LabelReleaseDate => _localization.GetString("PartDialog.LabelReleaseDate");
        public string ImagesTitle => $"Images ({Part.ImagePaths.Count})";
        public string CloseButton => _localization.GetString("PartDialog.ButtonCancel");
    }

    /// <summary>
    /// Represents an image item for display in the gallery
    /// </summary>
    public class PartImageItem : ObservableObject
    {
        private bool _isSelected;

        public string ImagePath { get; set; }
        public int Index { get; set; }
        public int TotalCount { get; set; }
        public string Label => $"Image {Index + 1} of {TotalCount}";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public PartImageItem(string imagePath, int index, int totalCount)
        {
            ImagePath = imagePath;
            Index = index;
            TotalCount = totalCount;
            IsSelected = false;
        }
    }
}
