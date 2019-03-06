using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Project_X_Login_Server
{
    class Game_Server : Connection
    {
        private DateTime TimeUntilRelease = default(DateTime);
        private Thread AuthenticationThread;

        public Game_Server(ConnectionType type, int id) :
            base (type, id)
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
            if (Network.instance.GameServerAuthenticated)
            {
                Console.WriteLine("Authentication of Game Server successful, ready for client connections.");
            }
            else
            {
                Console.WriteLine("Authentication of Game Server failed, releasing socket.");
                Close();
            }
            AuthenticationThread.Join();
        }
    }
}
