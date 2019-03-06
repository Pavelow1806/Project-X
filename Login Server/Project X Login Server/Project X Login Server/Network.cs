using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Login_Server
{
    class Network
    {
        public static Network instance;

        #region TCP
        private const int MaxConnections = 100;
        private const int Port = 5600;
        private TcpListener Listener = new TcpListener(IPAddress.Any, Port);

        public const int BufferSize = 4096;
        #endregion

        #region Connections
        public Game_Server GameServer;
        public const double SecondsToAuthenticateBeforeDisconnect = 5.0;
        public string AuthenticationCode = "";
        public bool GameServerAuthenticated = false;
        public Client[] Clients = new Client[MaxConnections];
        #endregion

        public Network()
        {
            instance = this;
        }
        
        public bool LaunchServer()
        {
            AuthenticationCode = Database.instance.RequestAuthenticationCode();
            if (AuthenticationCode == "")
            {
                Console.WriteLine("Critical Error! Authentication code could not be loaded.");
            }
            else
            {
                Console.WriteLine("Authentication code loaded.");
            }
            try
            {
                GameServer = new Game_Server(ConnectionType.GAMESERVER, 0);
                for (int i = 0; i < MaxConnections; i++)
                {
                    Clients[i] = new Client(ConnectionType.CLIENT, i);
                }
                Listener.Start();
            }
            catch (Exception e)
            {
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
            TcpClient socket = Listener.EndAcceptTcpClient(result);
            socket.NoDelay = false;
            if (GameServer.Socket == null)
            {
                // Allow the game server to connect
                GameServer.Connected = true;
                GameServer.Socket = socket;
                GameServer.IP = socket.Client.RemoteEndPoint.ToString();
                GameServer.Start();
                Console.WriteLine("Contact from potential game server made: ");
                Console.WriteLine("IP: " + GameServer.IP);
                Console.WriteLine("Waiting for authentication packet..");
            }
            else if (GameServerAuthenticated)
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
    }
}
