using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CarPartStoreApp.Converters
{
    /// <summary>
    /// Converts a space-separated list of image URLs to the first URL
    /// </summary>
    public class FirstImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrWhiteSpace(imagePath))
            {
                // Split by spaces and get the first URL
                var firstImage = imagePath.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                return new BitmapImage(new Uri(firstImage));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
