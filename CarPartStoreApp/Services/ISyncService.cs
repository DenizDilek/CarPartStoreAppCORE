using CarPartStoreApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// Interface for synchronization operations with web applications
    /// TODO: Implement sync logic when web API is available
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Synchronizes local changes to the web application
        /// </summary>
        Task<bool> SyncToWebAsync();

        /// <summary>
        /// Pulls changes from the web application to local database
        /// </summary>
        Task<bool> SyncFromWebAsync();

        /// <summary>
        /// Checks if there are pending changes to sync
        /// </summary>
        Task<bool> HasPendingChangesAsync();

        /// <summary>
        /// Gets the last sync timestamp
        /// </summary>
        Task<DateTime?> GetLastSyncTimeAsync();
    }
}