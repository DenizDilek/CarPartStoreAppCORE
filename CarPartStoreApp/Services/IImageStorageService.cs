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
<<<<<<< Updated upstream
=======
        /// Uploads an image from byte array to cloud storage
        /// </summary>
        /// <param name="imageData">Image data as byte array</param>
        /// <param name="publicId">Public ID for the image</param>
        /// <returns>The URL of the uploaded image</returns>
        Task<string> UploadImageBytesAsync(byte[] imageData, string publicId);

        /// <summary>
>>>>>>> Stashed changes
        /// Deletes an image from cloud storage
        /// </summary>
        /// <param name="imagePath">The image URL or public ID to delete</param>
        Task DeleteImageAsync(string imagePath);
    }
}
