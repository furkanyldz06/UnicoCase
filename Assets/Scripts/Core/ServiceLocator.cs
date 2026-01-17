using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoardDefence.Core
{
    /// <summary>
    /// Service Locator Pattern implementation
    /// Provides a centralized point to access game services
    /// Alternative to Singleton pattern with better testability
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();
        private static bool _isQuitting;

        /// <summary>
        /// Register a service
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (Services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type} is already registered. Overwriting.");
            }
            Services[type] = service;
        }

        /// <summary>
        /// Get a registered service
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            if (!_isQuitting)
            {
                Debug.LogError($"Service of type {type} not found. Make sure it's registered.");
            }
            return null;
        }

        /// <summary>
        /// Try to get a registered service
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var s))
            {
                service = s as T;
                return service != null;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Unregister a service
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            Services.Remove(type);
        }

        /// <summary>
        /// Clear all registered services
        /// </summary>
        public static void Clear()
        {
            Services.Clear();
        }

        /// <summary>
        /// Set application quitting state to suppress error logs
        /// </summary>
        public static void SetQuitting(bool quitting)
        {
            _isQuitting = quitting;
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return Services.ContainsKey(typeof(T));
        }
    }

    /// <summary>
    /// MonoBehaviour that registers itself to the ServiceLocator
    /// </summary>
    public abstract class RegisteredService<T> : MonoBehaviour where T : class
    {
        protected virtual void Awake()
        {
            ServiceLocator.Register(this as T);
        }

        protected virtual void OnDestroy()
        {
            ServiceLocator.Unregister<T>();
        }
    }
}

