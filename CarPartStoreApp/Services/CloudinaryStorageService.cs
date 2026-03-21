using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// Cloudinary implementation of image storage service
    /// Uploads and stores car part images in Cloudinary cloud storage
    /// </summary>
    public class CloudinaryStorageService : IImageStorageService
    {
        private readonly Cloudinary _cloudinary;
        private const string Folder = "car-parts";

        /// <summary>
        /// Initializes a new instance of CloudinaryStorageService
        /// </summary>
        /// <param name="cloudName">Cloudinary cloud name</param>
        /// <param name="apiKey">Cloudinary API key</param>
        /// <param name="apiSecret">Cloudinary API secret</param>
        public CloudinaryStorageService(string cloudName, string apiKey, string apiSecret)
        {
            if (string.IsNullOrEmpty(cloudName))
                throw new ArgumentException("Cloud name is required", nameof(cloudName));
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("API key is required", nameof(apiKey));
            if (string.IsNullOrEmpty(apiSecret))
                throw new ArgumentException("API secret is required", nameof(apiSecret));

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        /// <summary>
        /// Uploads an image file to Cloudinary from a local file path
        /// The image is stored in the "car-parts" folder with the public ID provided
        /// </summary>
        /// <param name="filePath">Local path to the image file</param>
        /// <param name="publicId">Public ID used for organizing the image (e.g., "5_0" for part ID 5, image 0)</param>
        /// <returns>The URL of the uploaded image</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
        /// <exception cref="ArgumentException">Thrown when public ID is empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when upload fails</exception>
        public async Task<string> UploadImageAsync(string filePath, string publicId)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Image file not found: {filePath}", filePath);

            if (string.IsNullOrEmpty(publicId))
                throw new ArgumentException("Public ID is required", nameof(publicId));

            try
            {
                // Ensure public ID includes folder prefix
                string fullPublicId = EnsureFolderPrefix(publicId);

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(filePath),
                    PublicId = fullPublicId,
                    Folder = Folder,
                    Overwrite = true, // Overwrite if image with same public ID exists
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InvalidOperationException(
                        $"Cloudinary upload failed with status {uploadResult.StatusCode}: {uploadResult.Error?.Message}");
                }

                if (uploadResult.SecureUrl == null)
                {
                    throw new InvalidOperationException("Cloudinary upload succeeded but no URL returned");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex) when (!(ex is FileNotFoundException || ex is ArgumentException || ex is InvalidOperationException))
            {
                // Wrap any Cloudinary-specific exceptions
                throw new InvalidOperationException($"Failed to upload image to Cloudinary: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Uploads an image to Cloudinary from a byte array
        /// The image is stored in the "car-parts" folder with the public ID provided
        /// </summary>
        /// <param name="imageBytes">The image data as a byte array</param>
        /// <param name="publicId">Public ID used for organizing the image (e.g., "5_0" for part ID 5, image 0)</param>
        /// <param name="fileName">Original file name for content type detection (optional)</param>
        /// <returns>The URL of the uploaded image</returns>
        /// <exception cref="ArgumentNullException">Thrown when imageBytes is null</exception>
        /// <exception cref="ArgumentException">Thrown when public ID is empty or image bytes is empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when upload fails</exception>
        public async Task<string> UploadImageAsync(byte[] imageBytes, string publicId, string? fileName = null)
        {
            if (imageBytes == null)
                throw new ArgumentNullException(nameof(imageBytes), "Image bytes cannot be null");

            if (imageBytes.Length == 0)
                throw new ArgumentException("Image bytes cannot be empty", nameof(imageBytes));

            if (string.IsNullOrEmpty(publicId))
                throw new ArgumentException("Public ID is required", nameof(publicId));

            try
            {
                // Ensure public ID includes folder prefix
                string fullPublicId = EnsureFolderPrefix(publicId);

                // Create a memory stream from the byte array
                using var memoryStream = new MemoryStream(imageBytes);

                // Determine file extension for content type
                string extension = DetermineFileExtension(imageBytes, fileName);

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName ?? $"{publicId}{extension}", memoryStream),
                    PublicId = fullPublicId,
                    Folder = Folder,
                    Overwrite = true, // Overwrite if image with same public ID exists
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InvalidOperationException(
                        $"Cloudinary upload failed with status {uploadResult.StatusCode}: {uploadResult.Error?.Message}");
                }

                if (uploadResult.SecureUrl == null)
                {
                    throw new InvalidOperationException("Cloudinary upload succeeded but no URL returned");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is ArgumentException || ex is InvalidOperationException))
            {
                // Wrap any Cloudinary-specific exceptions
                throw new InvalidOperationException($"Failed to upload image to Cloudinary: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes an image from Cloudinary
        /// Can accept either a full URL or a public ID
        /// </summary>
        /// <param name="imagePath">The image URL or public ID to delete</param>
        /// <exception cref="ArgumentException">Thrown when image path is empty</exception>
        public async Task DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                throw new ArgumentException("Image path is required", nameof(imagePath));

            try
            {
                // Extract public ID from URL if necessary
                string publicId = imagePath;
                if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    publicId = ExtractPublicIdFromUrl(imagePath);
                }

                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

                if (deleteResult.StatusCode != System.Net.HttpStatusCode.OK && deleteResult.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    // Log warning but don't throw - the part might still be deleted from DB
                    // This prevents orphaned parts when Cloudinary deletion fails
                    System.Diagnostics.Debug.WriteLine(
                        $"Cloudinary delete warning: {deleteResult.StatusCode} - {deleteResult.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log warning but don't throw - allow DB deletion to proceed
                System.Diagnostics.Debug.WriteLine($"Cloudinary delete failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Formats a public ID for a part image
        /// </summary>
        /// <param name="partId">The part ID</param>
        /// <param name="imageIndex">The image index (0-based)</param>
        /// <returns>Formatted public ID (e.g., "car-parts/5_0")</returns>
        public string FormatPublicId(int partId, int imageIndex)
        {
            return $"{Folder}/{partId}_{imageIndex}";
        }

        /// <summary>
        /// Ensures the public ID includes the folder prefix
        /// </summary>
        /// <param name="publicId">The public ID (with or without folder prefix)</param>
        /// <returns>Public ID with folder prefix (e.g., "car-parts/5_0")</returns>
        private string EnsureFolderPrefix(string publicId)
        {
            // If the public ID already starts with the folder, return as-is
            if (publicId.StartsWith($"{Folder}/", StringComparison.OrdinalIgnoreCase))
            {
                return publicId;
            }

            // Otherwise, prepend the folder
            return $"{Folder}/{publicId}";
        }

        /// <summary>
        /// Determines the file extension based on image bytes or file name
        /// </summary>
        /// <param name="imageBytes">The image data</param>
        /// <param name="fileName">Optional file name</param>
        /// <returns>File extension including dot (e.g., ".jpg")</returns>
        private string DetermineFileExtension(byte[] imageBytes, string? fileName)
        {
            // First try to detect from image header (magic bytes)
            string? detectedExtension = DetectImageTypeFromBytes(imageBytes);
            if (!string.IsNullOrEmpty(detectedExtension))
            {
                return detectedExtension;
            }

            // Fall back to file name extension
            if (!string.IsNullOrEmpty(fileName))
            {
                string extension = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(extension))
                {
                    return extension.ToLowerInvariant();
                }
            }

            // Default to .jpg
            return ".jpg";
        }

        /// <summary>
        /// Detects image type from magic bytes in the header
        /// </summary>
        /// <param name="imageBytes">The image data</param>
        /// <returns>File extension if detected, null otherwise</returns>
        private string? DetectImageTypeFromBytes(byte[] imageBytes)
        {
            if (imageBytes.Length < 8)
                return null;

            // Check for common image format magic bytes
            // JPEG: FF D8 FF
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
            {
                return ".jpg";
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E &&
                imageBytes[3] == 0x47 && imageBytes[4] == 0x0D && imageBytes[5] == 0x0A &&
                imageBytes[6] == 0x1A && imageBytes[7] == 0x0A)
            {
                return ".png";
            }

            // GIF: 47 49 46 38 (GIF8)
            if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 &&
                imageBytes[3] == 0x38)
            {
                return ".gif";
            }

            // BMP: 42 4D (BM)
            if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
            {
                return ".bmp";
            }

            // WebP: 52 49 46 46 ... 57 45 42 50 (RIFF...WEBP)
            if (imageBytes.Length >= 12 &&
                imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 &&
                imageBytes[3] == 0x46 && imageBytes[8] == 0x57 && imageBytes[9] == 0x45 &&
                imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
            {
                return ".webp";
            }

            return null;
        }

        /// <summary>
        /// Extracts the public ID from a Cloudinary URL
        /// </summary>
        /// <param name="url">The Cloudinary URL</param>
        /// <returns>The public ID without file extension</returns>
        private string ExtractPublicIdFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;

                // Remove leading slash
                path = path.TrimStart('/');

                // Remove image version if present (e.g., /v1234567890/)
                var versionIndex = path.IndexOf("/v", StringComparison.Ordinal);
                if (versionIndex > 0)
                {
                    var nextSlash = path.IndexOf('/', versionIndex + 1);
                    if (nextSlash > 0)
                    {
                        path = path.Substring(nextSlash + 1);
                    }
                }

                // Remove file extension
                var lastDot = path.LastIndexOf('.');
                if (lastDot > 0)
                {
                    path = path.Substring(0, lastDot);
                }

                return path;
            }
            catch
            {
                // If URL parsing fails, return the original value
                return url;
            }
        }
    }
}
