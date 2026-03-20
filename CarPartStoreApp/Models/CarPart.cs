using System;
using System.Collections.Generic;
using CarPartStoreApp.ViewModels;

namespace CarPartStoreApp.Models
{
    /// <summary>
    /// Represents a car part in the inventory system
    /// </summary>
    public class CarPart : ObservableObject
    {
        private int _id;
        private string _partNumber = string.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private int _categoryId;
        private string _categoryName = string.Empty;
        private decimal _costPrice;
        private int _stockQuantity;
        private string _location = string.Empty;
        private string _imagePath = string.Empty;
<<<<<<< Updated upstream
        private string? _model = string.Empty;
        private int? _releaseYear;
=======
<<<<<<< Updated upstream
=======
        private List<string> _imagePaths = new List<string>();
        private string? _model = string.Empty;
        private string _brand = string.Empty;
        private int? _releaseYear;
>>>>>>> Stashed changes
>>>>>>> Stashed changes
        private DateTime _createdDate = DateTime.Now;
        private DateTime? _lastUpdated;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string PartNumber
        {
            get => _partNumber;
            set => SetProperty(ref _partNumber, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public int CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        /// <summary>
        /// Category name for display purposes
        /// This is set when loading from database
        /// </summary>
        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        public decimal CostPrice
        {
            get => _costPrice;
            set => SetProperty(ref _costPrice, value);
        }

        public int StockQuantity
        {
            get => _stockQuantity;
            set => SetProperty(ref _stockQuantity, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public string ImagePath
        {
            get => _imagePaths.Count > 0 ? _imagePaths[0] : string.Empty;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _imagePaths.Clear();
                }
                else if (_imagePaths.Count == 0)
                {
                    _imagePaths.Add(value);
                }
                else
                {
                    _imagePaths[0] = value;
                }
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
                OnPropertyChanged(nameof(ImagePaths));
            }
        }

<<<<<<< Updated upstream
=======
        /// <summary>
        /// Collection of image paths for this part.
        /// First image is the primary image.
        /// </summary>
        public List<string> ImagePaths
        {
            get => _imagePaths;
            set
            {
                _imagePaths = value ?? new List<string>();
                OnPropertyChanged(nameof(ImagePaths));
                // Also update the single ImagePath for backward compatibility
                _imagePath = _imagePaths.Count > 0 ? _imagePaths[0] : string.Empty;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
        public string? Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

<<<<<<< Updated upstream
=======
        public string Brand
        {
            get => _brand;
            set => SetProperty(ref _brand, value);
        }

>>>>>>> Stashed changes
        public int? ReleaseDate
        {
            get => _releaseYear;
            set => SetProperty(ref _releaseYear, value);
        }

<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }
    }
}