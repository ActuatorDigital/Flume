// Copyright (c) AIR Pty Ltd. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AIR.Flume
{
    public class FlumeServiceContainer : MonoBehaviour
    {
        public event Action<FlumeServiceContainer> OnContainerReady;
        private static Injector _injector;

        private ServiceRegister _register = new ServiceRegister();
        private static Queue<IDependent> _earlyDependents = new Queue<IDependent>();

        public FlumeServiceContainer Register<TService>()
            where TService : class
        {
            _register.Register<TService>();
            return this;
        }

        public FlumeServiceContainer Register<TService>(TService instance)
            where TService : class
        {
            _register.Register(instance);
            return this;
        }

        public FlumeServiceContainer Register<TService, TImplementation>()
            where TService : class
            where TImplementation : TService
        {
            if (typeof(TImplementation).IsSubclassOf(typeof(MonoBehaviour))) {
                Component monoBehaviour = FindObjectsOfType<MonoBehaviour>()
                    .FirstOrDefault(mb => mb is TImplementation);
                if (monoBehaviour == null)
                    monoBehaviour = gameObject.AddComponent(typeof(TImplementation));
                _register.Register(monoBehaviour as TService);
            } else {
                _register.Register<TService, TImplementation>();
            }

            return this;
        }

        public static void InjectThis(Dependent dependent) =>
            InjectThis(dependent as IDependent);

        public static void InjectThis(DependentBehaviour dependentBehaviour) =>
            InjectThis(dependentBehaviour as IDependent);

        public static void InjectThis(ScriptableDependent dependentBehaviour) =>
            InjectThis(dependentBehaviour as IDependent);

        internal object Resolve(Type dependentType, IDependent dependent)
        {
            try {
                return _register.Resolve(dependentType);
            } catch (MissingServiceException) {
                throw new MissingDependencyException(dependentType, dependent);
            }
        }

        private void OnDestroy()
        {
            _injector = null;
            _register.Dispose();
            _earlyDependents.Clear();
        }

        private static void InjectThis(IDependent dependentBehaviour)
        {
            if (_injector == null && _earlyDependents != null)
                _earlyDependents.Enqueue(dependentBehaviour);
            else
                _injector?.InjectDependencies(dependentBehaviour);
        }

        private void Awake()
        {
            OnContainerReady?.Invoke(this);

            _injector = new Injector(this);
            while (_earlyDependents.Count > 0) {
                var dependent = _earlyDependents.Dequeue();
                _injector.InjectDependencies(dependent);
            }
        }
    }
}