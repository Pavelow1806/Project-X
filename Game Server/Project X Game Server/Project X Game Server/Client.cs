using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class Client : Connection
    {
        public string Email = "";
        public bool LoggedIn = false;
        public DateTime LoggedInTime = default(DateTime);

        public Client(ConnectionType type, int id) :
            base(type, id, CommunicationType.Receive)
        {

        }

        public override void Start()
        {
            base.Start();
            SessionID = Index.ToString("000") + " - " + IP + " - " + ConnectedTime.ToString("yyyy/MM/dd hh:mm:ss");
        }

        public override void Disconnect()
        {
            Username = "";
            Email = "";
            LoggedIn = false;
            LoggedInTime = default(DateTime);
            base.Disconnect();
        }
    }
}
