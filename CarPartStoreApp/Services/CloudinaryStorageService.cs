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
        /// Uploads an image file to Cloudinary
        /// The image is stored in the "car-parts" folder with the part number as the public ID
        /// </summary>
        /// <param name="filePath">Local path to the image file</param>
        /// <param name="partNumber">Part number used as public ID for organizing the image</param>
        /// <returns>The URL of the uploaded image</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
        /// <exception cref="InvalidOperationException">Thrown when upload fails</exception>
        public async Task<string> UploadImageAsync(string filePath, string partNumber)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Image file not found: {filePath}", filePath);

            if (string.IsNullOrEmpty(partNumber))
                throw new ArgumentException("Part number is required", nameof(partNumber));

            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(filePath),
                    PublicId = $"{Folder}/{partNumber}",
                    Folder = Folder,
                    Overwrite = true, // Overwrite if image with same part number exists
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
                if (imagePath.StartsWith("http"))
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
