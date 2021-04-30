// Copyright (c) AIR Pty Ltd. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AIR.Flume.Tests")]
namespace AIR.Flume
{
    public class ServiceRegister : IDisposable
    {
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();

#if UNITY_INCLUDE_TESTS
        public void Replace<TService, TImplementation>()
            where TService : class
            where TImplementation : TService
        {
            var service = Activator.CreateInstance(typeof(TImplementation));
            Replace<TService>((TImplementation)service);
        }

        public void Replace<TService>(TService service)
            where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out object existingService))
            {
                UnityEngine.Debug.LogWarning(
                    "Replacing " + existingService.GetType() +
                    " with service of type " + typeof(TService));
                _services.Remove(typeof(TService));
            }

            if (existingService == null)
            {
                UnityEngine.Debug.LogWarning(
                    "Trying to replace " + typeof(TService) +
                    " but the service was not registered. " +
                    "Unless this is a test, you might be doing something wrong.");
            }

            _services.Add(typeof(TService), service);
        }
#endif

        public void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : TService
        {
            CheckServiceCollision<TService>();
            var service = Activator.CreateInstance(typeof(TImplementation));
            _services.Add(typeof(TService), service);
        }

        public void Register<TImplementation>()
            where TImplementation : class
        {
            CheckServiceCollision<TImplementation>();
            var service = Activator.CreateInstance(typeof(TImplementation));
            _services.Add(typeof(TImplementation), service);
        }

        public void Register<TImplementation>(TImplementation implementation)
            where TImplementation : class
        {
            CheckServiceCollision<TImplementation>();
            _services.Add(typeof(TImplementation), implementation);
        }

        public void Dispose()
        {
            _services.Clear();
        }

        internal object Resolve(Type t)
        {
            if (_services.ContainsKey(t))
                return _services[t];

            throw new MissingServiceException(t);
        }

        internal T Resolve<T>()
            where T : class
        {
            var serviceType = typeof(T);
            return (T)Resolve(serviceType);
        }

        private void CheckServiceCollision<T>()
        {
            if (_services.ContainsKey(typeof(T)))
                throw new ServiceCollisionException<T>();
        }
    }
}