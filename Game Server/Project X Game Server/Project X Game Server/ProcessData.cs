using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Project_X_Game_Server
{
    public enum ClientProcessPacketNumbers
    {
        Invalid,
        EnterWorld
    }
    public enum LoginServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WhiteList
    }
    public enum GameServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer
    }
    public enum SyncServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WorldRequest
    }
    class ProcessData : Data
    {
        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        public static void processData(int index, byte[] Data)
        {
            lock (lockObj)
            {
                Reset();
                buffer.WriteBytes(Data);

                ConnectionType Source = (ConnectionType)buffer.ReadInteger();
                int PacketNumber = buffer.ReadInteger();

                data = Data;
                Index = index;
                object[] obj;
                switch (Source)
                {
                    case ConnectionType.SYNCSERVER:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(SyncServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[(ConnectionType)Index].Socket == null)
                        {
                            return;
                        }
                        Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((SyncServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.SYNCSERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);
                        obj = new object[1];
                        obj[0] = Source;
                        typeof(ProcessData).InvokeMember(((SyncServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    case ConnectionType.LOGINSERVER:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(LoginServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[ConnectionType.LOGINSERVER].Socket == null)
                        {
                            return;
                        }
                        Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((LoginServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.GAMESERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);
                        obj = new object[1];
                        obj[0] = Source;
                        typeof(ProcessData).InvokeMember(((LoginServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    case ConnectionType.CLIENT:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(ClientProcessPacketNumbers), PacketNumber) || Network.instance.Clients[Index].Socket == null)
                        {
                            return;
                        }
                        Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((ClientProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.CLIENT.ToString() + ", Processing response..", Log.LogType.RECEIVED);
                        obj = new object[1];
                        obj[0] = Source;
                        typeof(ProcessData).InvokeMember(((ClientProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    default:
                        break;
                }

                Reset();
            }
        }

        #region Server Communication
        private static void AuthenticateServer(ConnectionType type)
        {
            if (buffer.ReadString() == Network.instance.AuthenticationCode)
            {
                // Confirmed to be the correct server, proceed with unblocking the client communication channels
                Network.instance.Servers[(ConnectionType)Index].Authenticated = true;
                Network.instance.SyncServerAuthenticated = true;
                Network.instance.Servers.Add(type, Network.instance.Servers[(ConnectionType)Index]);
                Network.instance.Servers.Remove((ConnectionType)Index);
                Network.instance.Servers[type].Index = (int)type;
            }
        }
        #endregion
        
        #region Login Server Communication
        private static void WhiteList(ConnectionType type)
        {
            string ip = buffer.ReadString();
            int LineNumber = Log.log("Checking white list for IP: " + ip.Substring(0, ip.IndexOf(':')) + "..", Log.LogType.RECEIVED);
            if (!Network.instance.CheckWhiteList(ip.Substring(0, ip.IndexOf(':'))))
            {
                Network.instance.WhiteList.Add(ip.Substring(0, ip.IndexOf(':')));
                Log.log(LineNumber, "Client IP: " + ip.Substring(0, ip.IndexOf(':')) + " added successfully, sending confirmation to login server.", Log.LogType.RECEIVED);
                SendData.ConfirmWhiteList(ip.Substring(0, ip.IndexOf(':')));
            }
            else
            {
                Log.log(LineNumber, "Client IP: " + ip.Substring(0, ip.IndexOf(':')) + " was already white listed.", Log.LogType.WARNING);
            }
        }
        #endregion

        #region Synchronization Server Communication
        private static void WorldRequest(ConnectionType type)
        {
            int LineNumber = Log.log("Processing world request packet..", Log.LogType.RECEIVED);
            int Character_Count = buffer.ReadInteger();
            for (int i = 0; i < Character_Count; i++)
            {
                World.instance.players.Add(new Player(buffer.ReadInteger(), buffer.ReadString(), buffer.ReadInteger(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat()));
                Log.log(LineNumber, "Processing world request packet.. Added character " + i.ToString() + "/" + Character_Count.ToString(), Log.LogType.RECEIVED);
            }
            Log.log(LineNumber, "Processing world request packet.. Added characters (" + Character_Count.ToString() + ")", Log.LogType.SUCCESS);
        }
        #endregion

        #region Client Communication
        private static void EnterWorld(ConnectionType type)
        {
            string Character_Name = buffer.ReadString();
            Log.log("Account " + Network.instance.Clients[Index].Username + " is entering the world with " + Character_Name + "..", Log.LogType.SUCCESS);

        }
        #endregion
    }
}
