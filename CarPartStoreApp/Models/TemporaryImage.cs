using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace CarPartStoreApp.Models
{
    /// <summary>
    /// Represents a temporary image waiting to be uploaded to cloud storage
    /// </summary>
    public class TemporaryImage
    {
        private BitmapImage? _bitmapSource;

        /// <summary>
        /// Original file path on the local filesystem
        /// </summary>
        public string SourcePath { get; }

        /// <summary>
        /// Original file name
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Processed image data ready for upload
        /// </summary>
        public byte[] ProcessedData { get; }

        /// <summary>
        /// BitmapImage source for XAML binding
        /// Created from ProcessedData to ensure image is available even if source file is deleted
        /// </summary>
        public BitmapImage BitmapSource
        {
            get
            {
                if (_bitmapSource == null)
                {
                    try
                    {
                        _bitmapSource = new BitmapImage();
                        using (var stream = new MemoryStream(ProcessedData))
                        {
                            _bitmapSource.BeginInit();
                            _bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
                            _bitmapSource.StreamSource = stream;
                            _bitmapSource.DecodePixelWidth = 300; // Small preview
                            _bitmapSource.EndInit();
                        }
                        _bitmapSource.Freeze(); // Make accessible from any thread
                    }
                    catch
                    {
                        // If loading from memory fails, try loading from source path
                        try
                        {
                            _bitmapSource = new BitmapImage();
                            _bitmapSource.BeginInit();
                            _bitmapSource.UriSource = new Uri(SourcePath, UriKind.Absolute);
                            _bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
                            _bitmapSource.DecodePixelWidth = 300;
                            _bitmapSource.EndInit();
                            _bitmapSource.Freeze();
                        }
                        catch
                        {
                            // If both fail, create a dummy image
                            _bitmapSource = new BitmapImage();
                        }
                    }
                }
                return _bitmapSource;
            }
        }

        public TemporaryImage(string sourcePath, byte[] processedData)
        {
            SourcePath = sourcePath;
            FileName = Path.GetFileName(sourcePath);
            ProcessedData = processedData;
        }
    }
}
