using UnityEngine;

namespace AIR.Flume
{
    [RequireComponent(typeof(FlumeServiceContainer))]
    public abstract class ServiceInstaller : MonoBehaviour
    {
        protected abstract void InstallServices(FlumeServiceContainer container);

        void Awake() => gameObject
            .GetComponent<FlumeServiceContainer>()
            .OnContainerReady += InstallServices;
    }
}