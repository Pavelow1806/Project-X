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
            while (DateTime.Now < TimeUntilRelease || !Network.instance.GameServerAuthenticated)
            {

            }
            if (Authenticated)
            {
                Log.log("Authentication of Game Server successful, " + ((Network.instance.SyncServerAuthenticated) ? "ready for client connections." : "waiting for synchronization server."), Log.LogType.CONNECTION);
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
