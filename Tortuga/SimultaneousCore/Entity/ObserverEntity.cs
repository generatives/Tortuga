using SimultaneousCore.Message;
using SimultaneousCore.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimultaneousCore.Entity
{
    public class ObserverEntity : LocalEntity
    {
        public ObserverEntity(Guid id, IEntityLogic logic, SimultaneousSim sim, EntityRole role, float updatePeriod = 100) : base(id, logic, sim, role, updatePeriod)
        {

        }

        public void RecieveDeltaEnvelope(DeltaEnvelope deltaEnv)
        {
            Logic.ApplyDeltas(deltaEnv.Deltas);
        }

        public override void NetworkEntityAdded(IEntity entity)
        {

        }
    }
}
