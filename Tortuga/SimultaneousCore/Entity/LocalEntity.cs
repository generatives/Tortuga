using SimultaneousCore.Message;
using SimultaneousCore.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimultaneousCore.Entity
{
    public abstract class LocalEntity : IEntity
    {
        public Guid Id { get; private set; }
        public EntityRole Role { get; private set; }

        protected SimultaneousSim Sim { get; private set; }
        protected IEntityLogic Logic { get; set; }

        
        private List<FrameSnapshot> _recordedFrameSnapshots;
        private object _lastValidSnapshot;

        public float UpdatePeriod { get; set; }
        private float _timeSinceLastUpdate;
        private object _lastSentSnapshot;

        public LocalEntity(Guid id, IEntityLogic logic, SimultaneousSim sim, EntityRole role, float updatePeriod = 100f)
        {
            Id = id;
            Logic = logic;
            Sim = sim;
            Role = role;

            _recordedFrameSnapshots = new List<FrameSnapshot>();

            UpdatePeriod = updatePeriod;

            NetworkEntityAdded(this);
        }

        public void RecieveEnvelope(FrameCommands envelope)
        {
            Logic.ProcessCommands(envelope.Commands);
            //_recievedEnvelopes.Add(new RecievedCmdEnvelope()
            //{
            //    Envelope = envelope,
            //    RecievedTimestamp = _sim.GetTimestamp()
            //});
        }

        public void SimulateTimespan(long startingTimestamp, long endTimestamp, float deltaTime)
        {
            for(var currentTimestamp = startingTimestamp; currentTimestamp < endTimestamp; currentTimestamp++)
            {
                //Console.WriteLine($"Simulate Authority up to {currentTimestamp} with {deltaTime}");
                Logic.Simulate(deltaTime);
                //Console.WriteLine($"Authority is at {_logic.TakeSnapshot()}");
                _recordedFrameSnapshots.Add(new FrameSnapshot() { Snapshot = Logic.TakeSnapshot(), RecordedTimestamp = currentTimestamp });
            }
        }
        
        public void GoToTimestamp(long timestamp)
        {
            var frameSnapshot = _recordedFrameSnapshots.FirstOrDefault(r => r.RecordedTimestamp == timestamp) ?? _recordedFrameSnapshots.LastOrDefault();
            if(frameSnapshot != null)
            {
                //Console.WriteLine($"Going to {frameSnapshot.Snapshot} at {frameSnapshot.RecordedTimestamp} for {timestamp}");
                Logic.ApplySnapshot(frameSnapshot.Snapshot);
                _recordedFrameSnapshots = _recordedFrameSnapshots.Where(r => r.RecordedTimestamp < timestamp).ToList();
            }
        }

        public abstract void RecieveFrameRecord(FrameCommands envelope);
        public abstract void RecieveDeltaEnvelope(DeltaEnvelope deltaEnv);
        public abstract void NetworkEntityAdded(IEntity entity);
    }
}
