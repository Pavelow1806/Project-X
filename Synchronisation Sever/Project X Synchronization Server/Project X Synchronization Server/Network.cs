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
        private SynchronizationScheduler scheduler = new SynchronizationScheduler();

        public static bool Running = false;

        #region TCP
        public const int Port = 5602;

        private const int GameServerPort = 5601;
        private const int LoginServerPort = 5600;

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
            int LineNumber = Log.log("Loading Authentication code..", Log.LogType.SYSTEM);
            if (AuthenticationCode == "")
            {
                Log.log(LineNumber, "Critical Error! Authentication code could not be loaded.", Log.LogType.ERROR);
            }
            else
            {
                Log.log(LineNumber, "Authentication code loaded.", Log.LogType.SUCCESS);
            }
            SynchronizationScheduler.instance.LoadSynchronizationSettings();
            Data.Initialise();
            try
            {
                Servers.Add(ConnectionType.LOGINSERVER, new Connection(ConnectionType.LOGINSERVER, 0, LoginServerPort, "127.0.0.1"));
                Servers[ConnectionType.LOGINSERVER].Start();
                Servers.Add(ConnectionType.GAMESERVER, new Connection(ConnectionType.GAMESERVER, 1, GameServerPort, "127.0.0.1"));
                Servers[ConnectionType.GAMESERVER].Start();
            }
            catch (Exception e)
            {
                Log.log("An error occurred when attempting to start the server. > " + e.Message, Log.LogType.ERROR);
                return false;
            }
            SynchronizationScheduler.instance.Start();
            Running = true;
            return true;
        }
    }
}

