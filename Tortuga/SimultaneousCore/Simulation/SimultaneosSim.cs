using SimultaneousCore.Entity;
using SimultaneousCore.Message;
using SimultaneousCore.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimultaneousCore.Simulation
{
    public class SimultaneousSim
    {
        public event Action<RemoteSim> OwnerSimConnected;
        public event Action<RemoteSim> OwnerSimDisconnecting;
        public event Action<RemoteSim> ClientSimConnected;
        public event Action<RemoteSim> ClientSimDisconnecting;

        private ISimultaneousInterface _interface;
        private INetwork _network;

        public InstanceRole Role { get; private set; }

        public Guid Id { get; private set; }
        private RemoteSim _ownerSim;
        private List<RemoteSim> _clientSims;

        private Dictionary<Guid, LocalEntity> _entities;

        private List<EntityAdded> _addedEntites;
        private List<EntityRemoved> _removedEntities;

        private long _currentTimeStamp;

        private SortedList<long, List<EntityMessage<FrameCommands>>> _recievedFrameRecords;

        public SimultaneousSim(ISimultaneousInterface @interface, INetwork network)
        {
            _interface = @interface;
            _network = network;
            _clientSims = new List<RemoteSim>();
            _entities = new Dictionary<Guid, LocalEntity>();
            _addedEntites = new List<EntityAdded>();
            _removedEntities = new List<EntityRemoved>();
            _network.PeerConnected += _network_PeerConnected;
            _network.PeerDisconnected += _network_PeerDisconnected;
            _network.MessageRecieved += _network_MessageRecieved;
            _network.SimulationIdAssigned += _network_SimulationIdAssigned;
            _recievedFrameRecords = new SortedList<long, List<EntityMessage<FrameCommands>>>();
        }

        private void _network_SimulationIdAssigned(Guid id)
        {
            Id = id;
        }

        private void _network_MessageRecieved(RemoteSim sim, object[] messages)
        {
            foreach(var message in messages)
            {
                switch(message)
                {
                    case EntityAdded added:
                        ProcessEntityAdded(added);
                        break;
                    case EntityRemoved removed:
                        RemoveEntityInternal(removed.EntityId);
                        break;
                    case EntityMessage<DeltaEnvelope> delta:
                        if(_entities.ContainsKey(delta.EntityId))
                        {
                            var entity = _entities[delta.EntityId];
                            entity.RecieveDeltas(delta.Message);
                        }
                        break;
                    case EntityMessage<FrameCommands> cmd:
                        if(!_recievedFrameRecords.ContainsKey(cmd.Message.SentTimestamp))
                        {
                            _recievedFrameRecords.Add(cmd.Message.SentTimestamp, new List<EntityMessage<FrameCommands>>());
                        }
                        _recievedFrameRecords[cmd.Message.SentTimestamp].Add(cmd);
                        break;
                }
            }
        }

        private void _network_PeerDisconnected(RemoteSim sim)
        {
            if (sim.Role == InstanceRole.OWNER)
            {
                OwnerSimDisconnecting?.Invoke(_ownerSim);
                _ownerSim = null;
            }
            else
            {
                ClientSimDisconnecting?.Invoke(sim);
                _clientSims.Remove(sim);
            }
        }

        private void _network_PeerConnected(RemoteSim sim)
        {
            if(sim.Role == InstanceRole.OWNER)
            {
                _ownerSim = sim;
                OwnerSimConnected?.Invoke(_ownerSim);
            }
            else
            {
                _clientSims.Add(sim);
                ClientSimConnected?.Invoke(sim);
            }
        }

        public Guid NewEntity(IEntityLogic logic, Guid controllerSim, float updatePeriod = 100)
        {
            if(Role == InstanceRole.OWNER)
            {
                var id = Guid.NewGuid();

                var entityAdded = new EntityAdded()
                {
                    EntityId = id,
                    UpdatePeriod = updatePeriod,
                    CreationInfo = logic.GetCreationInfo(),
                    InitialSnapshot = logic.TakeSnapshot(),
                    ControllerSimId = controllerSim,
                    AuthoritySimId = Id
                };
                _addedEntites.Add(entityAdded);

                var entity = CreateLocalEntity(logic, entityAdded);
                AddRemoteEntites(entity, entityAdded);

                return id;
            }
            else
            {
                throw new Exception("Can only create entities on OWNER instance");
            }
        }

        private void AddRemoteEntites(LocalEntity entity, EntityAdded added)
        {
            if (Role == InstanceRole.OWNER)
            {
                foreach (var client in _clientSims)
                {
                    var remote = new RemoteEntity(entity.Id, GetRoleForSim(client.Id, added), client);
                    entity.NetworkEntityAdded(remote);
                }
            }
            else
            {
                var remote = new RemoteEntity(entity.Id, GetRoleForSim(_ownerSim.Id, added), _ownerSim);
                entity.NetworkEntityAdded(remote);
            }
        }

        private LocalEntity CreateLocalEntity(IEntityLogic logic, EntityAdded added)
        {
            var entity = new LocalEntity(added.EntityId, logic, this, GetRoleForSim(Id, added), added.UpdatePeriod);
            _entities[entity.Id] = entity;
            logic.ApplySnapshot(added.InitialSnapshot);
            return entity;
        }

        private EntityRole GetRoleForSim(Guid simId, EntityAdded added)
        {
            var role = EntityRole.EMPTY;

            if (simId == added.ControllerSimId) role |= EntityRole.CONTROLLER;

            if (simId == added.AuthoritySimId) role |= EntityRole.AUTHORITY;

            if(!(simId == added.ControllerSimId || simId == added.AuthoritySimId))
            {
                role |= EntityRole.OBSERVER;
            }

            return role;
        }

        private void ProcessEntityAdded(EntityAdded added)
        {
            var logic = _interface.CreateEntity(this, added.CreationInfo);
            var entity = CreateLocalEntity(logic, added);
            AddRemoteEntites(entity, added);
        }

        public void RemoveEntity(Guid id)
        {
            if (Role == InstanceRole.OWNER)
            {
                RemoveEntityInternal(id);
                _removedEntities.Add(new EntityRemoved() { EntityId = id });
            }
            else
            {
                throw new Exception("Can only remove entities on OWNER instance");
            }
        }

        private void RemoveEntityInternal(Guid id)
        {
            if(_entities.ContainsKey(id))
            {
                _entities.Remove(id);
            }
        }

        public void ConnectToHost(string address, int port)
        {
            Role = InstanceRole.CLIENT;
            _network.ConnectToHost(address, port);
        }

        public void ListenToPort(int port)
        {
            Role = InstanceRole.OWNER;
            _network.ListenToPort(port);
        }

        public void Update()
        {
            var lastFrameStamp = GetTimestamp();
            _currentTimeStamp += 1;

            _network.Update();

            if(_recievedFrameRecords.Any())
            {
                long previousTimestamp = 0;
                var isFirst = true;
                foreach(var kvp in _recievedFrameRecords)
                {
                    foreach (var entity in _entities.Values)
                    {
                        if (isFirst)
                        {
                            entity.GoToTimestamp(kvp.Key);
                        }
                        else
                        {
                            entity.SimulateTimespan(previousTimestamp, kvp.Key, 16);
                        }
                    }

                    isFirst = false;

                    var cmds = kvp.Value;
                    foreach(var cmd in cmds)
                    {
                        if (_entities.ContainsKey(cmd.EntityId))
                        {
                            var entity = _entities[cmd.EntityId];
                            entity.RecieveEnvelope(cmd.Message);
                        }
                    }
                    previousTimestamp = kvp.Key + 1;
                }
                foreach (var entity in _entities.Values)
                {
                    entity.SimulateTimespan(previousTimestamp, GetTimestamp(), 16.6f);
                }

                _recievedFrameRecords.Clear();
            }

            foreach (var entity in _entities.Values)
            {
                entity.Update();
            }

            if (Role == InstanceRole.OWNER)
            {
                foreach(var client in _clientSims)
                {
                    client.SendMessages(_addedEntites);
                    client.SendMessages(_removedEntities);
                }
                _addedEntites.Clear();
                _removedEntities.Clear();
            }
        }

        public float GetDeltaTime()
        {
            return (float)_interface.GetDeltaTime();
        }

        public long GetTimestamp()
        {
            return _currentTimeStamp;
        }
    }
}
