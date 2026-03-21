using System.Threading.Tasks;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// Interface for image storage operations
    /// Provides abstraction for uploading and deleting images from cloud storage
    /// </summary>
    public interface IImageStorageService
    {
        /// <summary>
        /// Uploads an image file to cloud storage from a local file path
        /// </summary>
        /// <param name="filePath">Local path to the image file</param>
        /// <param name="publicId">Public ID used for organizing the image (e.g., "5_0" for part ID 5, image 0)</param>
        /// <returns>The URL of the uploaded image</returns>
        Task<string> UploadImageAsync(string filePath, string publicId);

        /// <summary>
        /// Uploads an image to cloud storage from a byte array
        /// </summary>
        /// <param name="imageBytes">The image data as a byte array</param>
        /// <param name="publicId">Public ID used for organizing the image (e.g., "5_0" for part ID 5, image 0)</param>
        /// <param name="fileName">Original file name for content type detection (optional)</param>
        /// <returns>The URL of the uploaded image</returns>
        Task<string> UploadImageAsync(byte[] imageBytes, string publicId, string? fileName = null);

        /// <summary>
        /// Deletes an image from cloud storage
        /// </summary>
        /// <param name="imagePath">The image URL or public ID to delete</param>
        Task DeleteImageAsync(string imagePath);

        /// <summary>
        /// Formats a public ID for a part image
        /// </summary>
        /// <param name="partId">The part ID</param>
        /// <param name="imageIndex">The image index (0-based)</param>
        /// <returns>Formatted public ID (e.g., "car-parts/5_0")</returns>
        string FormatPublicId(int partId, int imageIndex);
    }
}
