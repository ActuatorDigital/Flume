using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIR.Flume {
    
    public class FlumeServiceContainer : MonoBehaviour {

        public event Action<FlumeServiceContainer> OnContainerReady;
        private static Injector _injector;

        private readonly ServiceRegister _register;
        private static Queue<DependentBehaviour> _earlyDependents;

        public FlumeServiceContainer() {
            _earlyDependents = new Queue<DependentBehaviour>();
            _register = new ServiceRegister();
        }

        internal object Resolve(Type dependentType) {
            try {
                return _register.Resolve(dependentType);
            } catch (MissingServiceException) {
                throw new MissingDependencyException(dependentType);
            }
        }

        public void Register<TService>() where TService : class {
            _register.Register<TService>();
        }

        public void Register<TService>(TService instance) 
            where TService : class 
        {
            _register.Register(instance);
        }

        public void Register<TService, TImplementation>() 
            where TService : class 
            where TImplementation : TService 
        {
            _register.Register<TService, TImplementation>();    
        }

        private void OnDestroy() {
            _injector = null;
            _register.Dispose();
            _earlyDependents.Clear();
        }

        public static void QueueInjection(DependentBehaviour dependentBehaviour) {
            if(_injector == null && _earlyDependents != null)
                _earlyDependents.Enqueue(dependentBehaviour);
            else 
                _injector?.InjectDependencies(dependentBehaviour);
        }

        private void Awake() {
            OnContainerReady?.Invoke(this);
            
            _injector = new Injector(this);
            while (_earlyDependents.Count > 0) {
                var dependent = _earlyDependents.Dequeue();
                _injector.InjectDependencies(dependent);
            }
            
        }
    }
    
}
