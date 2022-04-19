// Copyright (c) AIR Pty Ltd. All rights reserved.

using UnityEngine;

namespace AIR.Flume
{
    [RequireComponent(typeof(FlumeServiceContainer))]
    public abstract class ServiceInstaller : MonoBehaviour
    {
        protected abstract void InstallServices(FlumeServiceContainer container);

        protected virtual void OnException(System.Exception ex)
        { }

        private void Awake() => gameObject
            .GetComponent<FlumeServiceContainer>()
            .OnContainerReady += TryInstallServices;

        private void TryInstallServices(FlumeServiceContainer container)
        {
            try
            {
                InstallServices(container);
            }
            catch (System.Exception ex)
            {
                OnException(ex);
                throw new DuringServiceInstallException(ex);
            }
        }
    }
}