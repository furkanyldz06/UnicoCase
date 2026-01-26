using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoardDefence.Core
{

    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();
        private static bool _isQuitting;


        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (Services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type} is already registered. Overwriting.");
            }
            Services[type] = service;
        }


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


        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            Services.Remove(type);
        }


        public static void Clear()
        {
            Services.Clear();
        }


        public static void SetQuitting(bool quitting)
        {
            _isQuitting = quitting;
        }


        public static bool IsRegistered<T>() where T : class
        {
            return Services.ContainsKey(typeof(T));
        }
    }


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

