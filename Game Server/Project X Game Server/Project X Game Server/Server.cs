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

        #region Additional TCP Requirements for Server Listening
        public StreamReader Reader = null;
        public StreamWriter Writer = null;
        #endregion

        #region Threads
        private Thread AuthenticationThread;
        #endregion


        public Server(ConnectionType type, int id, int port, string ip, CommunicationType communication) :
            base(type, id, communication)
        {
            Port = port;
            IP = ip;
        }

        public override void Start()
        {
            if (Communication == CommunicationType.Send)
            {
                TimeUntilRelease = ConnectedTime.AddSeconds(Network.SecondsToAuthenticateBeforeDisconnect);
                AuthenticationThread = new Thread(new ThreadStart(CheckAuthentication));
                AuthenticationThread.Start();
            }
            base.Start();
        }

        public override void Close()
        {
            base.Close();
            if (Reader != null)
            {
                Reader.Close();
                Reader = null;
            }
            if (Writer != null)
            {
                Writer.Close();
                Writer = null;
            }
            ConnectionAttemptCount = 0;
            Authenticated = false;
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
