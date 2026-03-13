using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CarPartStoreApp.Localization
{
    /// <summary>
    /// Observable language resources that provide indexer-based access
    /// Raises property change notifications when resources are loaded
    /// </summary>
    public class LanguageResources : INotifyPropertyChanged
    {
        private Dictionary<string, string> _currentResources;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets a resource value by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string or key if not found</returns>
        public string this[string key]
        {
            get
            {
                System.Diagnostics.Debug.WriteLine($"LanguageResources indexer called for key: {key}");

                if (_currentResources != null && _currentResources.TryGetValue(key, out var value))
                {
                    System.Diagnostics.Debug.WriteLine($"  Key found, returning: {value}");
                    return value;
                }

                System.Diagnostics.Debug.WriteLine($"  Key NOT found, returning: {key}");
                return key; // Fallback to key if translation is missing
            }
            set
            {
                if (_currentResources != null)
                {
                    _currentResources[key] = value;
                    System.Diagnostics.Debug.WriteLine($"  Resource set: {key} = {value}");
                }
            }
        }

        /// <summary>
        /// Sets the current resources dictionary
        /// </summary>
        /// <param name="resources">New resources dictionary</param>
        public void SetResources(Dictionary<string, string> resources)
        {
            _currentResources = resources;
            OnPropertyChanged(); // Notify that all bindings should refresh
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }
}
