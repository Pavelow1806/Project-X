using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class Network
    {
        public static Network instance;

        public static bool Running = false;

        #region TCP
        private const int MaxConnections = 100;
        public const int Port = 5601;
        private TcpListener Listener = new TcpListener(IPAddress.Any, Port);

        private const int LoginServerPort = 5600;
        private const int SyncServerPort = 5602;

        public const int BufferSize = 4096;
        #endregion

        #region Connections
        public Dictionary<ConnectionType, Server> Servers = new Dictionary<ConnectionType, Server>();

        public const double SecondsToAuthenticateBeforeDisconnect = 5.0;
        public string AuthenticationCode = "";
        public bool SyncServerAuthenticated = false;
        private int ServerNumber = 0;
        public Client[] Clients = new Client[MaxConnections];
        #endregion

        public Network()
        {
            instance = this;
        }

        public bool LaunchServer()
        {
            AuthenticationCode = Database.instance.RequestAuthenticationCode();
            int LineNumber = Log.log("Loading Authentication code..", Log.LogType.SYSTEM);
            if (AuthenticationCode == "")
            {
                Log.log(LineNumber, "Critical Error! Authentication code could not be loaded.", Log.LogType.ERROR);
            }
            else
            {
                Log.log(LineNumber, "Authentication code loaded.", Log.LogType.SUCCESS);
            }
            try
            {
                Servers.Add(ConnectionType.LOGINSERVER, new Server(ConnectionType.LOGINSERVER, 0, LoginServerPort, "192.168.0.200"));
                Servers[ConnectionType.LOGINSERVER].Start();
                for (int i = 0; i < MaxConnections; i++)
                {
                    Clients[i] = new Client(ConnectionType.CLIENT, i);
                }
                Listener.Start();
                StartAccept();
            }
            catch (Exception e)
            {
                Log.log("An error occurred when attempting to start the server. > " + e.Message, Log.LogType.ERROR);
                return false;
            }
            Running = true;
            return true;
        }
        public void StartAccept()
        {
            Listener.BeginAcceptTcpClient(HandleAsyncConnection, Listener);
        }
        public void HandleAsyncConnection(IAsyncResult result)
        {
            StartAccept();
            OnConnect(result);
        }
        public void OnConnect(IAsyncResult result)
        {
            TcpClient socket = Listener.EndAcceptTcpClient(result);
            socket.NoDelay = false;
            if (!SyncServerAuthenticated)
            {
                Servers.Add((ConnectionType)ServerNumber, new Server(ConnectionType.SYNCSERVER, ServerNumber, SyncServerPort, "192.168.0.200"));
                Servers[(ConnectionType)ServerNumber].Connected = true;
                Servers[(ConnectionType)ServerNumber].Socket = socket;
                Servers[(ConnectionType)ServerNumber].IP = socket.Client.RemoteEndPoint.ToString();
                Servers[(ConnectionType)ServerNumber].Username = "System";
                Servers[(ConnectionType)ServerNumber].SessionID = "System";
                Servers[(ConnectionType)ServerNumber].Start();
                Console.WriteLine("Contact from potential server made: ");
                Console.WriteLine("IP: " + Servers[(ConnectionType)ServerNumber].IP);
                Console.WriteLine("Waiting for authentication packet..");
                ++ServerNumber;
            }
            else
            {
                for (int i = 0; i < MaxConnections; i++)
                {
                    if (Clients[i].Socket == null)
                    {
                        Clients[i].Connected = true;
                        Clients[i].Socket = socket;
                        Clients[i].IP = socket.Client.RemoteEndPoint.ToString();
                        Clients[i].Start();
                        Console.WriteLine("A client has connected to the server:");
                        Console.WriteLine("IP: " + Clients[i].IP);
                    }
                }
            }
        }
        public void CheckAuthentication()
        {
            foreach (KeyValuePair<ConnectionType, Server> server in Servers)
            {
                if (!(server.Key == ConnectionType.GAMESERVER || server.Key == ConnectionType.SYNCSERVER) && !server.Value.Authenticated)
                {
                    Servers.Remove(server.Key);
                }
            }
        }
    }
}
