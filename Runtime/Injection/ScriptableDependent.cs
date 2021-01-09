// Copyright (c) AIR Pty Ltd. All rights reserved.

using UnityEngine;

namespace AIR.Flume
{
    public abstract class ScriptableDependent : ScriptableObject, IDependent
    {
        protected void Awake()
            => FlumeServiceContainer.InjectThis(this);
    }
}