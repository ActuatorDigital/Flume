// Copyright (c) Actuator Digital Pty Ltd. All rights reserved.

namespace Actuator.Flume
{
    public class Dependent : IDependent
    {
        protected Dependent() =>
            FlumeServiceContainer.InjectThis(this);
    }
}