using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net.Sockets;

namespace Project_X_Game_Server
{
    class Server : Connection
    {
        private DateTime TimeUntilRelease = default(DateTime);
        private Thread AuthenticationThread;
        private Thread WorldThread;

        public Server(ConnectionType type, int id, int port, string ip, CommunicationType communication) :
            base(type, id, communication)
        {
            Port = port;
            IP = ip;
        }

        public override void Start()
        {
            base.Start();
            if (Communication == CommunicationType.Receive)
            {
                TimeUntilRelease = ConnectedTime.AddSeconds(Network.SecondsToAuthenticateBeforeDisconnect);
                AuthenticationThread = new Thread(new ThreadStart(CheckAuthentication));
                AuthenticationThread.Start();
            }
        }

        public override void Disconnect()
        {
            base.Disconnect();
            ConnectionAttemptCount = 0;
            Authenticated = false;
            Network.instance.SyncServerAuthenticated = false;
        }
        public void CheckAuthentication()
        {
            while (DateTime.Now < TimeUntilRelease || !Authenticated)
            {
                
            }
            if (Authenticated)
            {
                string msg = "";
                if (Network.instance.SyncServerAuthenticated)
                {
                    msg = "ready for client connections.";
                }
                int LineNumber = Log.log("Requesting initial world status from Synchronization Server..", Log.LogType.SYSTEM);
                SendData.WorldRequest(LineNumber);
                Log.log("Authentication of " + Type.ToString() + " successful, starting world data stream.. " + msg, Log.LogType.SUCCESS);
                WorldThread = new Thread(new ThreadStart(WorldStream));
                WorldThread.Start();
            }
            else
            {
                Log.log("Authentication of Server failed, releasing socket.", Log.LogType.ERROR);
                Disconnect();
            }
            AuthenticationThread.Join();
        }
        public void WorldStream()
        {
            Log.log("Starting world update thread..", Log.LogType.CACHE);
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            World.instance.BuildBuffer(ref buffer);
            while (Connected)
            {
                for (int i = 0; i < Network.instance.Clients.Length; i++)
                {
                    World.instance.BuildBuffer(ref buffer);
                    if (Network.instance.Clients[i].InGame())
                    {
                        SendData.SendUDP_Packet(Network.instance.Clients[i], buffer.ToArray());
                    }
                }
            }
            WorldThread.Join();
        }
    }
}
