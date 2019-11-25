using SimultaneousCore.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousLiteNetLib
{
    public sealed class RequestIntro
    {
    }

    public sealed class ProvideIntro
    {
        public InstanceRole Role;
        public Guid Id;
    }

    public sealed class SimulationMessages
    {
        public object[] Messages;
    }
}
