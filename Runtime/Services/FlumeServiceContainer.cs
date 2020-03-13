using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIR.Flume {
    
    public class FlumeServiceContainer : MonoBehaviour {

        public event Action<FlumeServiceContainer> OnContainerReady;
        private static Injector _injector;

        private readonly ServiceRegister _register;
        private static Queue<IDependent> _earlyDependents;

        public FlumeServiceContainer() {
            _earlyDependents = new Queue<IDependent>();
            _register = new ServiceRegister();
        }

        internal object Resolve(Type dependentType) {
            try {
                return _register.Resolve(dependentType);
            } catch (MissingServiceException) {
                throw new MissingDependencyException(dependentType);
            }
        }

        public FlumeServiceContainer Register<TService>() where TService : class {
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
            where TImplementation : TService {
            
            if (typeof(TImplementation).IsSubclassOf(typeof(MonoBehaviour))) {
                var component = gameObject.AddComponent(typeof(TImplementation));
                _register.Register(component as TService);
            }else
                _register.Register<TService, TImplementation>();
            
            return this;
        }

        private void OnDestroy() {
            _injector = null;
            _register.Dispose();
            _earlyDependents.Clear();
        }

        public static void InjectThis(Dependent dependent) =>
            InjectThis(dependent as IDependent);

        public static void InjectThis(DependentBehaviour dependentBehaviour) => 
            InjectThis(dependentBehaviour as IDependent);
        
        private static void InjectThis(IDependent dependentBehaviour) {
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
