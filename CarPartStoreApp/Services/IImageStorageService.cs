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
        /// Uploads an image file to cloud storage
        /// </summary>
        /// <param name="filePath">Local path to the image file</param>
        /// <param name="partNumber">Part number used for organizing the image</param>
        /// <returns>The URL of the uploaded image</returns>
        Task<string> UploadImageAsync(string filePath, string partNumber);

        /// <summary>
        /// Deletes an image from cloud storage
        /// </summary>
        /// <param name="imagePath">The image URL or public ID to delete</param>
        Task DeleteImageAsync(string imagePath);
    }
}
