using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousCore.Simulation
{
    public interface ISimulationInstance
    {
        InstanceRole Role { get; }
    }

    public enum InstanceRole
    {
        OWNER, CLIENT
    }
}
