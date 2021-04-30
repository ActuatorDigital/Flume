// Copyright (c) AIR Pty Ltd. All rights reserved.

using UnityEngine;

namespace AIR.Flume
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