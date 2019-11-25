using System;
using System.Collections.Generic;
using System.Text;
using SimultaneousCore.Message;
using SimultaneousCore.Simulation;

namespace SimultaneousCore.Entity
{
    public class RemoteEntity : IEntity
    {
        public Guid Id { get; private set; }
        public EntityRole Role { get; private set; }

        private RemoteSim _sim;

        public RemoteEntity(Guid id, EntityRole role, RemoteSim sim)
        {
            Id = id;
            Role = role;
            _sim = sim;
        }
        
        public void RecieveDeltaEnvelope(DeltaEnvelope deltaEnv)
        {
            _sim.SendMessage(new EntityMessage<DeltaEnvelope>(Id, deltaEnv));
        }

        public void RecieveFrameRecord(FrameCommands envelope)
        {
            _sim.SendMessage(new EntityMessage<FrameCommands>(Id, envelope));
        }
    }
}
