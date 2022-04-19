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
            where TImplementation : class, TService
        {
            var service = SafeActivate<TService, TImplementation>();
            Replace<TService>(service);
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
            where TImplementation : class, TService
        {
            CheckServiceCollision<TService>();
            var service = SafeActivate<TService, TImplementation>();
            _services.Add(typeof(TService), service);
        }

        public void Register<TImplementation>()
            where TImplementation : class
        {
            CheckServiceCollision<TImplementation>();
            var service = SafeActivate<TImplementation>();
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

        private TImplementation SafeActivate<TImplementation>()
            where TImplementation : class
        {
            var service = default(object);
            try
            {
                service = Activator.CreateInstance(typeof(TImplementation));
            }
            catch (Exception e)
            {
                throw new DuringRegisterException(typeof(TImplementation), e);
            }

            return service as TImplementation;
        }

        private TService SafeActivate<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return SafeActivate<TImplementation>() as TService;
        }
    }
}