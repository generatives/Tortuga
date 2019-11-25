using SimultaneousCore.Message;
using SimultaneousCore.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimultaneousCore.Entity
{
    public class LocalEntity : IEntity
    {
        public Guid Id { get; private set; }
        public EntityRole Role { get; private set; }

        private SimultaneousSim _sim;

        private IEntity _authorityEntity;
        private IEntity _controllerEntity;
        private List<IEntity> _clientEntities;

        private IEntityLogic _logic;
        
        private List<FrameSnapshot> _recordedFrameSnapshots;

        private List<FrameCommands> _recordedFrameCommands;
        private object _lastValidSnapshot;

        public float UpdatePeriod { get; set; }
        private float _timeSinceLastUpdate;
        private object _lastSentSnapshot;

        public LocalEntity(Guid id, IEntityLogic logic, SimultaneousSim sim, EntityRole role, float updatePeriod = 100f)
        {
            Id = id;
            _logic = logic;
            _sim = sim;
            Role = role;

            _recordedFrameSnapshots = new List<FrameSnapshot>();
            _recordedFrameCommands = new List<FrameCommands>();

            _clientEntities = new List<IEntity>();

            UpdatePeriod = updatePeriod;

            NetworkEntityAdded(this);
        }

        public void RecieveEnvelope(FrameCommands envelope)
        {
            _logic.ProcessCommands(envelope.Commands);
            //_recievedEnvelopes.Add(new RecievedCmdEnvelope()
            //{
            //    Envelope = envelope,
            //    RecievedTimestamp = _sim.GetTimestamp()
            //});
        }

        public void RecieveDeltas(DeltaEnvelope deltaEnv)
        {
            if(!Role.IsInRole(EntityRole.AUTHORITY))
            {
                //if (Role.IsInRole(EntityRole.CONTROLLER))
                //{
                //    Console.WriteLine($"State Before Delta: {_logic.TakeSnapshot()}");
                //}
                if (_lastValidSnapshot != null)
                {
                    _logic.ApplySnapshot(_lastValidSnapshot);
                }
                //if (Role.IsInRole(EntityRole.CONTROLLER))
                //{
                //    Console.WriteLine($"Last Valid State: {_logic.TakeSnapshot()}");
                //    Console.WriteLine($"Applying Delta: {deltaEnv.Deltas} From: {deltaEnv.SentTimestamp} It Is: {_sim.GetTimestamp()}");
                //}
                _logic.ApplyDeltas(deltaEnv.Deltas);

                _lastValidSnapshot = _logic.TakeSnapshot();

                _recordedFrameCommands = _recordedFrameCommands
                    .Where(e => e.SentTimestamp > deltaEnv.SentTimestamp)
                    .ToList();
                var processCommands = Role.IsInRole(EntityRole.CONTROLLER);
                foreach (var env in _recordedFrameCommands)
                {
                    if(processCommands)
                    {
                        _logic.ProcessCommands(env.Commands);
                        //Console.WriteLine($"ReProcessing Commands:");
                        //Console.WriteLine($"Timestamp: {env.SentTimestamp}, Delta: {env.SentDelta}");
                        //foreach (var command in env.Commands)
                        //{
                        //    Console.WriteLine($"{command}");
                        //}
                    }
                    _logic.Simulate(env.SentDelta);
                    //if (processCommands)
                    //{
                    //    Console.WriteLine($"State After Reprocess: {_logic.TakeSnapshot()}");
                    //}
                }

                //if (Role.IsInRole(EntityRole.CONTROLLER))
                //{
                //    Console.WriteLine($"State After Simulation: {_logic.TakeSnapshot()}");
                //}
            }
        }

        public void NetworkEntityAdded(IEntity entity)
        {
            if(entity.Role.IsInRole(EntityRole.AUTHORITY))
            {
                _authorityEntity = entity;
            }
            if (entity.Role.IsInRole(EntityRole.CONTROLLER))
            {
                _controllerEntity = entity;
            }
            if (entity.Role.IsInRole(EntityRole.OBSERVER))
            {
                _clientEntities.Add(entity);
            }
        }

        public void Update()
        {
            var isAuthority = Role.IsInRole(EntityRole.AUTHORITY);
            var delta = _sim.GetDeltaTime();

            if (_lastSentSnapshot == null && Role.IsInRole(EntityRole.AUTHORITY))
            {
                _lastSentSnapshot = _logic.TakeSnapshot();
            }

            if(!isAuthority)
            {
                var currentFrameCommands = new FrameCommands()
                {
                    Commands = new List<object>(),
                    SentDelta = delta,
                    SentTimestamp = _sim.GetTimestamp()
                };
                
                if (Role.IsInRole(EntityRole.CONTROLLER))
                {
                    //Console.WriteLine($"Delta is: {delta}");
                    var commands = _logic.GenerateCommands();
                    currentFrameCommands.Commands.Add(commands);
                    if (!Role.IsInRole(EntityRole.AUTHORITY))
                    {
                        _logic.ProcessCommands(currentFrameCommands.Commands);
                        //Console.WriteLine($"Processing Commands:");
                        //foreach (var command in currentFrameCommands.Commands)
                        //{
                        //    Console.WriteLine($"{command}");
                        //}
                    }

                    _authorityEntity.SendFrameRecord(currentFrameCommands);
                }

                _recordedFrameCommands.Add(currentFrameCommands);
            }

            //if (Role.IsInRole(EntityRole.AUTHORITY))
            //{
            //    _logic.ProcessCommands(_recievedEnvelopes.SelectMany(e => e.Envelope.Commands));
            //    _recievedEnvelopes.Clear();
            //}
            if(Role.IsInRole(EntityRole.AUTHORITY | EntityRole.CONTROLLER))
            {
                _logic.Simulate(delta);
            }

            if(isAuthority)
            {
                var timestamp = _sim.GetTimestamp();
                var newSnapshot = _logic.TakeSnapshot();
                _recordedFrameSnapshots.Add(new FrameSnapshot() { Snapshot = newSnapshot, RecordedTimestamp = timestamp });

                _timeSinceLastUpdate += delta;
                if (_timeSinceLastUpdate > UpdatePeriod)
                {
                    _timeSinceLastUpdate = 0;
                    var env = new DeltaEnvelope()
                    {
                        Deltas = _logic.CalculateDeltas(_lastSentSnapshot, newSnapshot),
                        SentTimestamp = timestamp
                    };
                    foreach (var client in _clientEntities)
                    {
                        client.SendDeltaEnvelope(env);
                    }
                    _controllerEntity?.SendDeltaEnvelope(env);
                    _lastSentSnapshot = newSnapshot;
                }

                _recordedFrameSnapshots = _recordedFrameSnapshots.Where(r => r.RecordedTimestamp > timestamp - 1000).ToList();
            }
        }

        public void SimulateTimespan(long startingTimestamp, long endTimestamp, float deltaTime)
        {
            for(var currentTimestamp = startingTimestamp; currentTimestamp < endTimestamp; currentTimestamp++)
            {
                //Console.WriteLine($"Simulate Authority up to {currentTimestamp} with {deltaTime}");
                _logic.Simulate(deltaTime);
                //Console.WriteLine($"Authority is at {_logic.TakeSnapshot()}");
                _recordedFrameSnapshots.Add(new FrameSnapshot() { Snapshot = _logic.TakeSnapshot(), RecordedTimestamp = currentTimestamp });
            }
        }
        
        public void GoToTimestamp(long timestamp)
        {
            var frameSnapshot = _recordedFrameSnapshots.FirstOrDefault(r => r.RecordedTimestamp == timestamp) ?? _recordedFrameSnapshots.LastOrDefault();
            if(frameSnapshot != null)
            {
                //Console.WriteLine($"Going to {frameSnapshot.Snapshot} at {frameSnapshot.RecordedTimestamp} for {timestamp}");
                _logic.ApplySnapshot(frameSnapshot.Snapshot);
                _recordedFrameSnapshots = _recordedFrameSnapshots.Where(r => r.RecordedTimestamp < timestamp).ToList();
            }
        }

        public void SendFrameRecord(FrameCommands envelope)
        {
            RecieveEnvelope(envelope);
        }

        public void SendDeltaEnvelope(DeltaEnvelope deltaEnv)
        {
            RecieveDeltas(deltaEnv);
        }
    }
}
