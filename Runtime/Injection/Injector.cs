// Copyright (c) AIR Pty Ltd. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AIR.Flume
{
    internal class Injector
    {
        private const string INJECT = "Inject";
        private readonly FlumeServiceContainer _container;

        public Injector(FlumeServiceContainer container)
        {
            _container = container;
        }

        internal void InjectDependencies(IDependent dependent)
        {
            if (_container == null) {
                Debug.LogWarning(
                    "Skipping Injection. " +
                    "No UnityServiceContainer container found in scene.");
                return;
            }

            var publicMethods = dependent.GetType().GetMethods();
            var privateMethods = dependent.GetType().GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance);
            var methods = publicMethods
                .Concat(privateMethods)
                .Distinct();
            foreach (var injectMethod in methods) {
                if (injectMethod.Name != INJECT) continue;
                var typeDependencies = injectMethod
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                var dependentServices = new List<object>();
                foreach (var dependentType in typeDependencies) {
                    var service = _container.Resolve(dependentType, dependent);
                    dependentServices.Add(service);
                }

                injectMethod.Invoke(dependent, dependentServices.ToArray());
            }
        }
    }
}