// Copyright (c) Actuator Digital Pty Ltd. All rights reserved.

using UnityEngine;

namespace Actuator.Flume
{
    public abstract class DependentBehaviour : MonoBehaviour, IDependent
    {
        protected virtual void Awake() =>
            FlumeServiceContainer.InjectThis(this);
    }
}