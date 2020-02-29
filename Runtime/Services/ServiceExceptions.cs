using System;

namespace AIR.Flume {

    public class MissingServiceException : Exception {
        private readonly Type _type;
        public MissingServiceException(Type type) => _type = type;
        public override string Message => $"No service was registered for {_type.Name}."; 
    }

    public class ServiceCollisionException<T> : Exception {
        public override string Message => $"Service already registered for {typeof(T).Name}. " +
                                          $"Use ReplaceService collision was if intentional.";
    }

    public class MissingDependencyException : Exception {
        private readonly Type _missingDependencyType;
        public override string Message => $"No dependency of type {_missingDependencyType.Name} was registered";

        public MissingDependencyException(Type missingDependencyType) {
            _missingDependencyType = missingDependencyType;
        }
    }
}
