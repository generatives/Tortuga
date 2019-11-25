using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimultaneousCore.Message;
using SimultaneousCore.Simulation;

namespace SimultaneousCore.Entity
{
    public class ControllerEntity : LocalEntity
    {
        private IEntity _authorityEntity;

        private List<FrameCommands> _recordedFrameCommands;
        private object _lastValidSnapshot;

        public ControllerEntity(Guid id, IEntityLogic logic, SimultaneousSim sim, EntityRole role, float updatePeriod = 100) : base(id, logic, sim, role, updatePeriod)
        {
            _recordedFrameCommands = new List<FrameCommands>();
        }

        public override void RecieveDeltaEnvelope(DeltaEnvelope deltaEnv)
        {
            if (_lastValidSnapshot != null)
            {
                Logic.ApplySnapshot(_lastValidSnapshot);
            }

            Logic.ApplyDeltas(deltaEnv.Deltas);

            _lastValidSnapshot = Logic.TakeSnapshot();

            _recordedFrameCommands = _recordedFrameCommands
                .Where(e => e.SentTimestamp > deltaEnv.SentTimestamp)
                .ToList();
            var processCommands = Role.IsInRole(EntityRole.CONTROLLER);
            foreach (var env in _recordedFrameCommands)
            {
                Logic.ProcessCommands(env.Commands);
                Logic.Simulate(env.SentDelta);
            }
        }

        public void Update()
        {
            var delta = Sim.GetDeltaTime();

            var currentFrameCommands = new FrameCommands()
            {
                Commands = new List<object>(),
                SentDelta = delta,
                SentTimestamp = Sim.GetTimestamp()
            };

            var commands = Logic.GenerateCommands();
            currentFrameCommands.Commands.Add(commands);
            Logic.ProcessCommands(currentFrameCommands.Commands);

            _authorityEntity.RecieveFrameRecord(currentFrameCommands);

            _recordedFrameCommands.Add(currentFrameCommands);

            Logic.Simulate(delta);
        }

        public override void NetworkEntityAdded(IEntity entity)
        {
            if (entity.Role.IsInRole(EntityRole.AUTHORITY))
            {
                _authorityEntity = entity;
            }
            else
            {
                throw new Exception("ControllerEntities only care about AuthorityEntities");
            }
        }
    }
}
