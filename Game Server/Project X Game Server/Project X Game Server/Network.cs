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
    public enum CommunicationType
    {
        Receive,
        Send
    }
    class Network
    {
        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        public static Network instance;

        private Thread MonitorThread;
        private int MonitorTickRate = 5000;
        
        public static bool Running = false;

        #region TCP
        private const int MaxConnections = 100;
        public const int Port = 5601;
        public const int UDPPort = 5604;
        private TcpListener Listener = new TcpListener(IPAddress.Any, Port);

        private const int LoginServerPort = 5600;
        private const int SyncServerPort = 5602;
        public const float SecondsBetweenPackets = 10.0f;

        public const int BufferSize = 4096;
        #endregion

        #region Connections
        public Dictionary<ConnectionType, Server> Servers = new Dictionary<ConnectionType, Server>();

        public const double SecondsToAuthenticateBeforeDisconnect = 5.0;
        public string AuthenticationCode = "";
        public bool SyncServerAuthenticated = false;
        private int ServerNumber = 10;
        public Client[] Clients = new Client[MaxConnections];

        public List<string> WhiteList = new List<string>();
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
                LineNumber = Log.log("Starting Login Server connection..", Log.LogType.SYSTEM);
                Servers.Add(ConnectionType.LOGINSERVER, new Server(ConnectionType.LOGINSERVER, 0, LoginServerPort, "127.0.0.1", CommunicationType.Send));
                Servers[ConnectionType.LOGINSERVER].Start();
                Log.log(LineNumber, "Starting Login Server connector started.", Log.LogType.SUCCESS);

                LineNumber = Log.log("Starting Client/Synchronization Listener..", Log.LogType.SYSTEM);
                for (int i = 0; i < MaxConnections; i++)
                {
                    Clients[i] = new Client(ConnectionType.CLIENT, i);
                }
                Listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                Listener.Start();
                StartAccept();
                Log.log(LineNumber, "Client/Synchronization Listener started.", Log.LogType.SUCCESS);

                Running = true;

                LineNumber = Log.log("Starting Connectivity Monitor..", Log.LogType.SYSTEM);
                MonitorThread = new Thread(new ThreadStart(SendConnectivityData));
                MonitorThread.Start();
                Log.log(LineNumber, "Connectivity Monitor Thread Started.", Log.LogType.SUCCESS);
            }
            catch (Exception e)
            {
                Log.log("An error occurred when attempting to start the server. > " + e.Message, Log.LogType.ERROR);
                return false;
            }
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
            lock (lockObj)
            {
                TcpClient socket = Listener.EndAcceptTcpClient(result);
                socket.NoDelay = false;
                if (!SyncServerAuthenticated)
                {
                    if (Servers.ContainsKey(ConnectionType.SYNCSERVER))
                    {
                        Servers.Remove(ConnectionType.SYNCSERVER);
                    }
                    Servers.Add((ConnectionType)ServerNumber, new Server(ConnectionType.SYNCSERVER, ServerNumber, SyncServerPort, socket.Client.RemoteEndPoint.ToString(), CommunicationType.Receive));
                    Servers[(ConnectionType)ServerNumber].Connected = true;
                    Servers[(ConnectionType)ServerNumber].Authenticated = false;
                    Servers[(ConnectionType)ServerNumber].Socket = socket;
                    Servers[(ConnectionType)ServerNumber].Username = "System";
                    Servers[(ConnectionType)ServerNumber].SessionID = "System";
                    Servers[(ConnectionType)ServerNumber].Start();
                    Log.log("Contact from potential server made: ", Log.LogType.CONNECTION);
                    if (Servers.ContainsKey((ConnectionType)ServerNumber))
                    {
                        Log.log("IP: " + Servers[(ConnectionType)ServerNumber].IP, Log.LogType.CONNECTION);
                    }
                    else
                    {
                        Log.log("IP: " + Servers[ConnectionType.SYNCSERVER].IP, Log.LogType.CONNECTION);
                    }
                    Log.log("Waiting for authentication packet..", Log.LogType.CONNECTION);
                    ++ServerNumber;
                }
                else
                {
                    if (CheckWhiteList(socket.Client.RemoteEndPoint.ToString().Substring(0, socket.Client.RemoteEndPoint.ToString().IndexOf(':'))))
                    {
                        Log.log("Client found in white-list, proceeding with accepting incoming connection..", Log.LogType.SUCCESS);
                        for (int i = 0; i < MaxConnections; i++)
                        {
                            if (Clients[i].Socket == null)
                            {
                                Clients[i].Connected = true;
                                Clients[i].Socket = socket;
                                Clients[i].IP = socket.Client.RemoteEndPoint.ToString();
                                Clients[i].Start();
                                Log.log("The white listed client has successfully connected to the server:", Log.LogType.CONNECTION);
                                Log.log("IP: " + Clients[i].IP, Log.LogType.CONNECTION);
                                Log.log("Sending initial world status to client..", Log.LogType.CONNECTION);
                                break;
                            }
                        }
                    }
                    else
                    {
                        Log.log("Client not found in white-list, denying connection.", Log.LogType.WARNING);
                        socket.Close();
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
        public bool CheckWhiteList(string ip)
        {
            for (int i = 0; i < WhiteList.Count; i++)
            {
                if (ip == WhiteList[i])
                {
                    return true;
                }
            }
            return false;
        }
        public void RemoveWhiteList(string ip)
        {
            WhiteList.Remove(ip);
        }
        public int GetIndex(int Character_ID)
        {
            for (int i = 0; i < Clients.Length; i++)
            {
                if (Clients[i].Character_ID == Character_ID)
                {
                    return i;
                }
            }
            return -1;
        }
        
        private void SendConnectivityData()
        {
            int LineN = -1;
            Dictionary<int, Connectivity> data = new Dictionary<int, Connectivity>();
            while (Running)
            {
                if (SyncServerAuthenticated)
                {
                    LineN = Log.log("Sending Monitor data to Synchonization Server..", Log.LogType.SYNC);
                    data.Clear();
                    for (int i = 0; i < Clients.Length; i++)
                    {
                        if (Clients[i].Character_ID > -1)
                        {
                            data.Add(Clients[i].Character_ID, new Connectivity(Clients[i].Character_ID, Clients[i].TCP_Throughput, Clients[i].TCP_PacketsReceived, Clients[i].TCP_PacketsSent, Clients[i].TCP_Latency,
                                Clients[i].UDP_Throughput, Clients[i].UDP_PacketsReceived, Clients[i].UDP_PacketsSent, Clients[i].UDP_Latency, Clients[i].LogStart));
                            Clients[i].LogStart = default(DateTime);
                            Log.log(LineN, "Sending Monitor data to Synchronization Server.. Client #" + i.ToString());
                        }
                    }
                    SendData.ConnectivityData(data);
                    Log.log(LineN, "Sent Monitor data to Synchronization Server.", Log.LogType.SUCCESS);
                }
                Thread.Sleep(MonitorTickRate);
            }
            MonitorThread.Join();
        }
    }
}
