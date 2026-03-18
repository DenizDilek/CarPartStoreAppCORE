using System;
using System.Collections.Generic;

namespace CarPartStoreApp.Services
{
    public static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void RegisterInstance<T>(T instance) where T : class
        {
            _services[typeof(T)] = instance;
        }

        public static T Resolve<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        public static T? GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }
            return null;
        }

        public static TursoDataService? TursoDataService =>
            _services.TryGetValue(typeof(TursoDataService), out var service) ? service as TursoDataService : null;
    }
}