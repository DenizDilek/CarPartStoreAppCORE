using System;
using System.Collections.Generic;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// Simple service container for dependency injection
    /// </summary>
    public static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="instance">Service instance</param>
        public static void RegisterInstance<T>(T instance) where T : class
        {
            _services[typeof(T)] = instance;
        }

        /// <summary>
        /// Resolves a registered service
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance</returns>
        public static T Resolve<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        /// <summary>
        /// Gets a registered service (alias for Resolve)
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance or null if not registered</returns>
        public static T? GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }
            return null;
        }
    }
}