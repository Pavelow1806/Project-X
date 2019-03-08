using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Project_X_Login_Server
{
    class Server : Connection
    {
        private DateTime TimeUntilRelease = default(DateTime);
        private Thread AuthenticationThread;

        public bool Authenticated = false;

        public Server(ConnectionType type, int id) :
            base(type, id)
        {

        }

        public override void Start()
        {
            base.Start();
            TimeUntilRelease = ConnectedTime.AddSeconds(Network.SecondsToAuthenticateBeforeDisconnect);
            AuthenticationThread = new Thread(new ThreadStart(CheckAuthentication));
            AuthenticationThread.Start();
        }

        public override void Close()
        {
            Network.instance.GameServerAuthenticated = false;
            base.Close();
        }
        public void CheckAuthentication()
        {
            while (DateTime.Now < TimeUntilRelease || !Authenticated)
            {

            }
            if (Authenticated)
            {
                string msg = "";
                if (Network.instance.SyncServerAuthenticated && Network.instance.GameServerAuthenticated)
                {
                    msg = "ready for client connections.";
                }
                else if (Network.instance.SyncServerAuthenticated && !Network.instance.GameServerAuthenticated)
                {
                    msg = "waiting for game server.";
                }
                else if (!Network.instance.SyncServerAuthenticated && Network.instance.GameServerAuthenticated)
                {
                    msg = "waiting for synchronization server.";
                }
                Log.log("Authentication of " + Type.ToString() + " successful, " + msg, Log.LogType.SUCCESS);
            }
            else
            {
                Log.log("Authentication of Server failed, releasing socket.", Log.LogType.ERROR);
                Close();
            }
            AuthenticationThread.Join();
        }
    }
}
