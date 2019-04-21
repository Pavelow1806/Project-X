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
        public int Character_ID = -1;
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
            if (Connected)
            {
                Username = "";
                Email = "";
                if (World.instance.players.ContainsKey(Character_ID))
                    World.instance.players[Character_ID].InWorld = false;
                Player player = World.instance.FindPlayerInWorld(Character_ID);
                if (player != null)
                {
                    for (int i = 0; i < Network.instance.Clients.Length; i++)
                    {
                        if (Network.instance.Clients[i] != null && Network.instance.Clients[i].Connected && Network.instance.Clients[i].LoggedIn &&
                            Network.instance.Clients[i].InGame())
                        {
                            SendData.PlayerStateChange(i, player, PlayerState.Logout);
                        }
                    }
                    World.instance.playersInWorld.Remove(player);
                }
                Character_ID = -1;
                LoggedIn = false;
                LoggedInTime = default(DateTime);
                Network.instance.RemoveWhiteList(IP.Substring(0, IP.IndexOf(':')));
                Log.log("Removed IP " + IP.Substring(0, IP.IndexOf(':')) + " from whitelist.", Log.LogType.SYSTEM);
                base.Disconnect();
            }
        }
        public bool InGame()
        {
            if (Connected && Socket != null && Socket.Connected && Character_ID > -1)
            {
                return true;
            }
            return false;
        }
    }
}
