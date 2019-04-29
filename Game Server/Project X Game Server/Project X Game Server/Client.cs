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

        // Connectivity Data
        public DateTime LogStart = default(DateTime);

        #region TCP
        // Throughput = (Time Received - Time Sent) / Size (b)
        private float TCP_throughput = 0.0f;
        public float TCP_Throughput
        {
            get
            {
                return TCP_throughput;
            }
            set
            {
                TCP_throughput = value;
                if (LogStart == default(DateTime)) LogStart = DateTime.Now;
            }
        }
        // Latency = Average of (Time Received - Time Sent)
        private float TCP_latency = 0.0f;
        public float TCP_Latency
        {
            get
            {
                return TCP_latency;
            }
            set
            {
                TCP_latency = value;
                if (LogStart == default(DateTime)) LogStart = DateTime.Now;
            }
        }
        public int Something = 1;
        private List<float> TCP_LatencyPackets = new List<float>();
        public void TCP_SetLatency(float NewEntry)
        {
            TCP_LatencyPackets.Add(NewEntry);
            float Sum = 0.0f;
            for (int i = 0; i < TCP_LatencyPackets.Count; i++)
            {
                Sum += TCP_LatencyPackets[i];
            }
            TCP_latency = Sum / TCP_LatencyPackets.Count;
        }
        public void TCP_ResetLatency()
        {
            TCP_latency = 0.0f;
            TCP_LatencyPackets.Clear();
        }
        private int TCP_packetsReceived = 0;
        public int TCP_PacketsReceived
        {
            get
            {
                return TCP_packetsReceived;
            }
            set
            {
                TCP_packetsReceived = value;
                if (LogStart == default(DateTime)) LogStart = DateTime.Now;
            }
        }
        private int TCP_packetsSent = 0;
        public int TCP_PacketsSent
        {
            get
            {
                return TCP_packetsSent;
            }
            set
            {
                TCP_packetsSent = value;
                if (LogStart == default(DateTime)) LogStart = DateTime.Now;
            }
        }
        #endregion

        #region UDP
        // Throughput = (Time Received - Time Sent) / Size(b)
        private float UDP_throughput = 0.0f;
        public float UDP_Throughput
        {
            get
            {
                return UDP_throughput;
            }
            set
            {
                UDP_throughput = value;
                if (LogStart == default(DateTime)) LogStart = DateTime.Now;
            }
        }
        // Latency = Average of (Time Received - Time Sent)
        private float UDP_latency = 0.0f;
        public float UDP_Latency
        {
            get
            {
                return UDP_latency;
            }
            set
            {
                UDP_latency = value;
                if (LogStart == default(DateTime)) LogStart = DateTime.Now;
            }
        }
        private List<float> UDP_LatencyPackets = new List<float>();
        public void UDP_SetLatency(float NewEntry)
        {
            UDP_LatencyPackets.Add(NewEntry);
            float Sum = 0.0f;
            for (int i = 0; i < UDP_LatencyPackets.Count; i++)
            {
                Sum += UDP_LatencyPackets[i];
            }
            UDP_Latency = Sum / UDP_LatencyPackets.Count;
        }
        public void UDP_ResetLatency()
        {
            UDP_Latency = 0.0f;
            UDP_LatencyPackets.Clear();
        }
        private int UDP_packetsReceived = 0;
        public int UDP_PacketsReceived
        {
            get
            {
                return UDP_packetsReceived;
            }
            set
            {
                UDP_packetsReceived = value;
                if (LogStart == default(DateTime)) LogStart = DateTime.Now;
            }
        }
        private int UDP_packetsSent = 0;
        public int UDP_PacketsSent
        {
            get
            {
                return UDP_packetsSent;
            }
            set
            {
                UDP_packetsSent = value;
                if (LogStart == default(DateTime)) LogStart = DateTime.Now;
            }
        }
        #endregion

        public int LineNumber = -1;

        public void ResetStats()
        {
            // TCP
            TCP_throughput = 0.0f;
            TCP_ResetLatency();
            TCP_packetsReceived = 0;
            TCP_packetsSent = 0;
            // UDP
            UDP_throughput = 0.0f;
            UDP_ResetLatency();
            UDP_packetsReceived = 0;
            UDP_packetsSent = 0;
            // Measures
            LogStart = default(DateTime);
        }

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
                SendData.LogActivity(Character_ID, Activity.LOGIN, SessionID);
                Username = "";
                Email = "";
                SessionID = "";
                Player player = null;
                if (World.instance.players.ContainsKey(Character_ID))
                {
                    World.instance.players[Character_ID].InWorld = false;
                    player = World.instance.players[Character_ID];
                }
                if (player != null)
                {
                    for (int i = 0; i < Network.instance.Clients.Length; i++)
                    {
                        if (Network.instance.Clients[i] != null && Network.instance.Clients[i].Connected &&
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
    public class Connectivity
    {
        public int Character_ID;
        public float TCP_Latency;
        public float TCP_Throughput;
        public int TCP_PacketsReceived;
        public int TCP_PacketsSent;
        public float UDP_Latency;
        public float UDP_Throughput;
        public int UDP_PacketsReceived;
        public int UDP_PacketsSent;
        public DateTime LogStart;
        public DateTime LogFinish;
        public Connectivity(int character_ID, 
            float tcp_throughput, int tcp_packetsReceived, int tcp_packetsSent, float tcp_latency,
            float udp_throughput, int udp_packetsReceived, int udp_packetsSent, float udp_latency, 
            DateTime logStart)
        {
            Character_ID = character_ID;
            TCP_Throughput = tcp_throughput;
            TCP_PacketsReceived = tcp_packetsReceived;
            TCP_PacketsSent = tcp_packetsSent;
            TCP_Latency = tcp_latency;
            UDP_Throughput = udp_throughput;
            UDP_PacketsReceived = udp_packetsReceived;
            UDP_PacketsSent = udp_packetsSent;
            UDP_Latency = udp_latency;
            LogStart = logStart;
            LogFinish = DateTime.Now;
        }
    }
}
