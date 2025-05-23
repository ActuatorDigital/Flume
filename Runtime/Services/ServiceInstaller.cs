﻿// Copyright (c) Actuator Digital Ltd. All rights reserved.

using UnityEngine;

namespace Actuator.Flume
{
    [RequireComponent(typeof(FlumeServiceContainer))]
    public abstract class ServiceInstaller : MonoBehaviour
    {
        protected abstract void InstallServices(FlumeServiceContainer container);

        private void Awake() => gameObject
            .GetComponent<FlumeServiceContainer>()
            .OnContainerReady += InstallServices;
    }
}