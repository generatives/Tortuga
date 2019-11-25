using Hyperion;
using LiteNetLib;
using SimultaneousCore.Networking;
using SimultaneousCore.Simulation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SimultaneousLiteNetLib
{
    public class LiteNetLibNetwork : INetwork
    {
        public static int MAX_CONNECTIONS = 100;
        public static string CONNECTION_KEY = "LiteNetLibNetwork";

        public event PeerConnected PeerConnected;
        public event PeerDisconnected PeerDisconnected;
        public event MessageRecieved MessageRecieved;
        public event SimulationIdAssigned SimulationIdAssigned;

        private EventBasedNetListener _listener;
        private NetManager _netManager;
        private Serializer _serializer;

        private List<(NetPeer, RemoteSim)> _peers;

        private byte[] _serializeBuffer;

        private InstanceRole _role;
        private Guid _id;

        public LiteNetLibNetwork(Serializer serializer)
        {
            _serializer = serializer;
            _serializeBuffer = new byte[4028];
            _peers = new List<(NetPeer, RemoteSim)>();
        }

        private void Init()
        {
            _listener = new EventBasedNetListener();
            _netManager = new NetManager(_listener);
            _netManager.SimulateLatency = true;
            _netManager.SimulationMinLatency = 20;
            _netManager.SimulationMaxLatency = 30;

            _listener.PeerConnectedEvent += _listener_PeerConnectedEvent;
            _listener.PeerDisconnectedEvent += _listener_PeerDisconnectedEvent;
            _listener.NetworkReceiveEvent += _listener_NetworkReceiveEvent;
        }

        public void ConnectToHost(string address, int port)
        {
            _role = InstanceRole.CLIENT;

            Init();
            _netManager.Start();
            _netManager.Connect(address, port, CONNECTION_KEY);

            _id = Guid.NewGuid();
            SimulationIdAssigned(_id);
        }

        public void ListenToPort(int port)
        {
            _role = InstanceRole.OWNER;

            Init();
            _netManager.Start(port);

            _listener.ConnectionRequestEvent += _listener_ConnectionRequestEvent;

            _id = Guid.NewGuid();
            SimulationIdAssigned(_id);
        }

        private void _listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            if (_netManager.PeersCount < MAX_CONNECTIONS /* max connections */)
                request.AcceptIfKey(CONNECTION_KEY);
            else
                request.Reject();
        }

        private void _listener_NetworkReceiveEvent(NetPeer peer, LiteNetLib.Utils.NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            var t = _peers.FirstOrDefault(p => p.Item1 == peer);
            using (var stream = new MemoryStream(reader.RawData, reader.UserDataOffset, reader.UserDataSize))
            {
                var message = _serializer.Deserialize(stream);
                switch(message)
                {
                    case RequestIntro reqIntro:
                        Send(peer, new ProvideIntro() { Role = _role, Id = _id }, DeliveryMethod.ReliableOrdered);
                        break;
                    case ProvideIntro provIntro:
                        if (t.Item2 == null)
                        {
                            RemoteSim sim = new RemoteSim(provIntro.Id, provIntro.Role);
                            _peers.Add((peer, sim));
                            PeerConnected?.Invoke(sim);
                        }
                        break;
                    case SimulationMessages simMessages:
                        MessageRecieved?.Invoke(t.Item2, simMessages.Messages);
                        break;
                }
            }
        }

        private void _listener_PeerConnectedEvent(NetPeer peer)
        {
            Send(peer, new RequestIntro(), DeliveryMethod.ReliableOrdered);
        }

        private void _listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var t = _peers.FirstOrDefault(p => p.Item1 == peer);
            if(t.Item1 != null)
            {
                _peers.Remove(t);
                PeerDisconnected?.Invoke(t.Item2);
            }
        }

        public void Update()
        {
            foreach (var t in _peers)
            {
                var peer = t.Item1;
                var sim = t.Item2;
                var messages = sim.TakeMessagesToSend().ToArray();
                if(messages.Length > 0)
                {
                    var simMessages = new SimulationMessages()
                    {
                        Messages = messages
                    };
                    Send(peer, simMessages, DeliveryMethod.ReliableOrdered);
                }
            }

            _netManager.PollEvents();
        }

        private void Send(NetPeer peer, object data, DeliveryMethod options)
        {
            var watch = Stopwatch.StartNew();
            var position = 0;
            using (var stream = new MemoryStream(_serializeBuffer))
            {
                SerializerSession session = new SerializerSession(_serializer);
                _serializer.Serialize(data, stream);
                position = (int)stream.Position;
            }
            watch.Stop();

            peer.Send(_serializeBuffer, 0, position, options);
        }
    }
}
