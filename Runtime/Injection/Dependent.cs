using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIR.Flume {
    public class Dependent : IDependent {

        public Dependent() => 
            FlumeServiceContainer.InjectThis(this);

    }    
}


