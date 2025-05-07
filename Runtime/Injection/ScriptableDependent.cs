// Copyright (c) Actuator Digital Pty Ltd. All rights reserved.

using UnityEngine;

namespace Actuator.Flume
{
    public abstract class ScriptableDependent : ScriptableObject, IDependent
    {
        protected void Awake()
            => FlumeServiceContainer.InjectThis(this);
    }
}