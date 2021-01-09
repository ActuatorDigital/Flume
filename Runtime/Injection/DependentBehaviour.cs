// Copyright (c) AIR Pty Ltd. All rights reserved.

using UnityEngine;

namespace AIR.Flume
{
    public abstract class DependentBehaviour : MonoBehaviour, IDependent
    {
        protected virtual void Awake() =>
            FlumeServiceContainer.InjectThis(this);
    }
}