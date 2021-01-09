// Copyright (c) AIR Pty Ltd. All rights reserved.

namespace AIR.Flume
{
    public class Dependent : IDependent
    {
        protected Dependent() =>
            FlumeServiceContainer.InjectThis(this);
    }
}