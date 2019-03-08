using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_X_Synchronization_Server
{
    class Network
    {
        public static Network instance;

        public static bool Running = false;

        #region TCP
        public const int Port = 5602;

        private const int GameServerPort = 5601;
        private const int LoginServerPort = 5500;        

        public const int BufferSize = 4096;
        #endregion

        #region Connections
        public Dictionary<ConnectionType, Connection> Servers = new Dictionary<ConnectionType, Connection>();
        
        public string AuthenticationCode = "";
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
                Log.log("Critical Error! Authentication code could not be loaded.", Log.LogType.ERROR);
            }
            else
            {
                Log.log("Authentication code loaded.", Log.LogType.SUCCESS);
            }
            try
            {
                Servers.Add(ConnectionType.LOGINSERVER, new Connection(ConnectionType.LOGINSERVER, 0, LoginServerPort, "192.168.0.200"));
                Servers[ConnectionType.LOGINSERVER].Start();
                Servers.Add(ConnectionType.GAMESERVER, new Connection(ConnectionType.GAMESERVER, 1, GameServerPort, "192.168.0.200"));
                Servers[ConnectionType.GAMESERVER].Start();
            }
            catch (Exception e)
            {
                Log.log("An error occurred when attempting to start the server. > " + e.Message, Log.LogType.ERROR);
                return false;
            }
            Running = true;
            return true;
        }
    }
}
