using SimultaneousCore.Entity;
using SimultaneousCore.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousCore
{
    public interface ISimultaneousInterface
    {
        double GetDeltaTime();
        IEntityLogic CreateEntity(SimultaneousSim sim, object info);
    }
}
