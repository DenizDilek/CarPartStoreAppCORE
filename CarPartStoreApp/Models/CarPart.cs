using System;
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
        private decimal _retailPrice;
        private int _stockQuantity;
        private string _location = string.Empty;
        private string _supplier = string.Empty;
        private string _imagePath = string.Empty;
        private string? _model = string.Empty;
        private int? _releaseYear;
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

        public decimal RetailPrice
        {
            get => _retailPrice;
            set => SetProperty(ref _retailPrice, value);
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

        public string Supplier
        {
            get => _supplier;
            set => SetProperty(ref _supplier, value);
        }

        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public string? Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        public int? ReleaseDate
        {
            get => _releaseYear;
            set => SetProperty(ref _releaseYear, value);
        }

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