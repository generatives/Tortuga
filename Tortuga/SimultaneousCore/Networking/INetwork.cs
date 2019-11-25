using SimultaneousCore.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousCore.Networking
{
    public delegate void PeerConnected(RemoteSim sim);
    public delegate void PeerDisconnected(RemoteSim sim);
    public delegate void MessageRecieved(RemoteSim sim, object[] messages);
    public delegate void SimulationIdAssigned(Guid id);

    public interface INetwork
    {
        event PeerConnected PeerConnected;
        event PeerDisconnected PeerDisconnected;
        event MessageRecieved MessageRecieved;
        event SimulationIdAssigned SimulationIdAssigned;

        void ConnectToHost(string address, int port);
        void ListenToPort(int port);

        void Update();
    }
}
