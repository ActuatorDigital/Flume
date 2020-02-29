using UnityEngine;

namespace AIR.Flume {
    
    public abstract class DependentBehaviour : MonoBehaviour {
        
        protected virtual void Awake() => 
            FlumeServiceContainer.QueueInjection(this);

    }
    
}
