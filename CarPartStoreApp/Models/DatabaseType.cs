namespace CarPartStoreApp.Models
{
    /// <summary>
    /// Represents the available database types for the application
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Local SQLite database stored in %LOCALAPPDATA%
        /// </summary>
        Local = 0,

        /// <summary>
        /// Turso cloud database (LibSQL compatible)
        /// </summary>
        Cloud = 1
    }
}
