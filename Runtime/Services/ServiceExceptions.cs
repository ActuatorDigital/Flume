// Copyright (c) AIR Pty Ltd. All rights reserved.

using System;

namespace AIR.Flume
{
    public class MissingServiceException : Exception
    {
        private readonly Type _type;

        public MissingServiceException(Type type) => _type = type;

        public override string Message => $"No service was registered for {_type.Name}.";
    }

    public class ServiceCollisionException<T> : Exception
    {
        public override string Message => $"Service already registered for {typeof(T).Name}. " +
                                          $"Use ReplaceService collision was if intentional.";
    }

    public class DuringRegisterException : Exception
    {
        public DuringRegisterException(Type type, Exception ex)
            : base($"{type.Name} threw during register.", ex)
        {
        }
    }

    public class DuringServiceInstallException : Exception
    {
        public DuringServiceInstallException(Exception ex)
            : base($"An Exception was thrown during service installation.", ex)
        {
        }
    }

    public class DuringInjectException : Exception
    {
        public DuringInjectException(Type type, Exception ex)
            : base($"{type.Name} threw during inject.", ex)
        {
        }
    }

    internal class MissingDependencyException : Exception
    {
        private readonly Type _missingDependencyType;
        private IDependent _dependent;

        public MissingDependencyException(Type missingDependencyType, IDependent dependent)
        {
            _dependent = dependent;
            _missingDependencyType = missingDependencyType;
        }

        public override string Message =>
            $"{_missingDependencyType.Name} not registered, but Injected by {_dependent.GetType().Name}.";
    }
}