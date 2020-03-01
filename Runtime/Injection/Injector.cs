using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AIR.Flume {
    internal class Injector
    {
        private const string INJECT = "Inject";
        private readonly FlumeServiceContainer _container;

        public Injector(FlumeServiceContainer container) {
            _container = container;
        }
        
        internal void InjectDependencies( IDependent dependent ) {
            if (_container == null) {
                Debug.LogWarning(
                    "Skipping Injection. " +
                    "No UnityServiceContainer container found in scene.");
                return;
            }
            
            var injectMethod = dependent.GetType().GetMethod(INJECT);
            if(injectMethod == null)
                injectMethod = dependent.GetType().GetMethod(INJECT,
                    BindingFlags.NonPublic | BindingFlags.Instance);
            if(injectMethod == null) return;
        
            var typeDependencies = injectMethod
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
                
            var dependentServices = new List<object>();
            foreach (var dependentType in typeDependencies) {
                var service = _container.Resolve(dependentType);
                dependentServices.Add(service);
            }
        
            injectMethod.Invoke(dependent, dependentServices.ToArray());
            
        }

    }
    
}

