using SimultaneousCore.Message;
using SimultaneousCore.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimultaneousCore.Entity
{
    public class AuthorityEntity : LocalEntity
    {
        private IEntity _controllerEntity;
        private List<IEntity> _clientEntities;
        private object _lastSentSnapshot;
        private List<FrameSnapshot> _recordedFrameSnapshots;
        private float _timeSinceLastUpdate;

        public AuthorityEntity(Guid id, IEntityLogic logic, SimultaneousSim sim, EntityRole role, float updatePeriod = 100) : base(id, logic, sim, role, updatePeriod)
        {
            _recordedFrameSnapshots = new List<FrameSnapshot>();
        }

        public override void RecieveDeltaEnvelope(DeltaEnvelope deltaEnv)
        {
            throw new Exception("AuthorityEntities shouldn't recieve deltas");
        }

        public void Update()
        {
            var delta = Sim.GetDeltaTime();

            if (_lastSentSnapshot == null)
            {
                _lastSentSnapshot = Logic.TakeSnapshot();
            }

            //if (Role.IsInRole(EntityRole.AUTHORITY))
            //{
            //    _logic.ProcessCommands(_recievedEnvelopes.SelectMany(e => e.Envelope.Commands));
            //    _recievedEnvelopes.Clear();
            //}
            Logic.Simulate(delta);

            var timestamp = Sim.GetTimestamp();
            var newSnapshot = Logic.TakeSnapshot();
            _recordedFrameSnapshots.Add(new FrameSnapshot() { Snapshot = newSnapshot, RecordedTimestamp = timestamp });

            _timeSinceLastUpdate += delta;
            if (_timeSinceLastUpdate > UpdatePeriod)
            {
                _timeSinceLastUpdate = 0;
                var env = new DeltaEnvelope()
                {
                    Deltas = Logic.CalculateDeltas(_lastSentSnapshot, newSnapshot),
                    SentTimestamp = timestamp
                };
                foreach (var client in _clientEntities)
                {
                    client.RecieveDeltaEnvelope(env);
                }
                _controllerEntity?.RecieveDeltaEnvelope(env);
                _lastSentSnapshot = newSnapshot;
            }

            _recordedFrameSnapshots = _recordedFrameSnapshots.Where(r => r.RecordedTimestamp > timestamp - 1000).ToList();
        }

        public override void NetworkEntityAdded(IEntity entity)
        {
            if (entity.Role.IsInRole(EntityRole.CONTROLLER))
            {
                _controllerEntity = entity;
            }
            else if (entity.Role.IsInRole(EntityRole.OBSERVER))
            {
                _clientEntities.Add(entity);
            }
            else
            {
                throw new Exception("There shouldn't be multiple AuthorityEntities");
            }
        }
    }
}
