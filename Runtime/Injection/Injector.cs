// Copyright (c)Actuator Digital Pty Ltd. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Actuator.Flume
{
    internal class Injector
    {
        private const string INJECT = "Inject";
        private readonly FlumeServiceContainer _container;

        private readonly Dictionary<Type, InjectMethodAndArgumentsSet> _cachedInjectSets =
            new Dictionary<Type, InjectMethodAndArgumentsSet>();

        public Injector(FlumeServiceContainer container)
        {
            _container = container;
        }

        internal void InjectDependencies(IDependent dependent)
        {
            if (_container == null)
            {
                Debug.LogWarning(
                    "Skipping Injection. " +
                    "No UnityServiceContainer container found in scene.");
                return;
            }

            var dependentType = dependent.GetType();

            if (!_cachedInjectSets.TryGetValue(dependentType, out var injectMethodAndArguments))
            {
                CacheAndInjectDependencies(dependent, dependentType);

                return;
            }

            foreach (var injectMethod in injectMethodAndArguments)
            {
                injectMethod.Method.Invoke(dependent, injectMethod.Args);
            }
        }

        private void CacheAndInjectDependencies(IDependent dependent, Type dependentType)
        {
            InjectMethodAndArgumentsSet injectMethodAndArguments;
            var methods = dependentType.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            injectMethodAndArguments = new InjectMethodAndArgumentsSet();

            foreach (var injectMethod in methods)
            {
                if (injectMethod.Name != INJECT) continue;
                var injectArgTypes = injectMethod
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                var dependentServices = new List<object>();
                foreach (var injectArgType in injectArgTypes)
                {
                    var service = _container.Resolve(injectArgType, dependent);
                    dependentServices.Add(service);
                }

                var depArrayArgs = dependentServices.ToArray();
                injectMethod.Invoke(dependent, depArrayArgs);

                injectMethodAndArguments.Add(new InjectMethodAndArguments(injectMethod, depArrayArgs));
            }

            _cachedInjectSets[dependentType] = injectMethodAndArguments;
        }

        internal class InjectMethodAndArgumentsSet : List<InjectMethodAndArguments> { }

        internal class InjectMethodAndArguments
        {
            public InjectMethodAndArguments(MethodInfo method, object[] args)
            {
                Method = method;
                Args = args;
            }

            public MethodInfo Method { get; private set; }
            public object[] Args { get; private set; }
        }
    }
}