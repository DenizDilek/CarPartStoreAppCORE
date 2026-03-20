using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CarPartStoreApp.Helpers
{
    /// <summary>
    /// Helper class for processing images to meet size and format requirements
    /// </summary>
    public static class ImageProcessingHelper
    {
        /// <summary>
        /// Processes an image to meet size and format requirements
        /// - Converts to JPG format
        /// - Resizes if needed to keep file under 500KB
        /// </summary>
        /// <param name="sourcePath">Full path to the source image file</param>
        /// <returns>Byte array of the processed image</returns>
        public static byte[] ProcessImage(string sourcePath)
        {
            using var fileStream = File.OpenRead(sourcePath);
            var decoder = BitmapDecoder.Create(fileStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames[0];

            // Try different quality levels to get under 500KB
            for (int quality = 90; quality >= 50; quality -= 10)
            {
                var jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.QualityLevel = quality;

                using var memoryStream = new MemoryStream();
                jpegEncoder.Frames.Add(BitmapFrame.Create(frame));
                jpegEncoder.Save(memoryStream);

                if (memoryStream.Length < 500 * 1024) // 500KB
                {
                    return memoryStream.ToArray();
                }
            }

            // If still too large, resize
            return ResizeImage(sourcePath);
        }

        /// <summary>
        /// Resizes an image to reduce file size
        /// </summary>
        /// <param name="sourcePath">Full path to the source image file</param>
        /// <returns>Byte array of the resized image</returns>
        private static byte[] ResizeImage(string sourcePath)
        {
            // Load and resize to max 1024x1024
            using var fileStream = File.OpenRead(sourcePath);
            var decoder = BitmapDecoder.Create(fileStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames[0];

            var scaled = new TransformedBitmap(frame, new ScaleTransform(0.5, 0.5));

            var jpegEncoder = new JpegBitmapEncoder();
            jpegEncoder.QualityLevel = 75;
            jpegEncoder.Frames.Add(BitmapFrame.Create(scaled));

            using var memoryStream = new MemoryStream();
            jpegEncoder.Save(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
