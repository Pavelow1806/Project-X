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
        LoginRequest,
        RegistrationRequest,
        CharacterListRequest
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
        AuthenticateServer
    }
    class ProcessData : Data
    {
        public static void processData(byte[] Data)
        {
            buffer.WriteBytes(Data);

            ConnectionType Source = (ConnectionType)buffer.ReadByte();
            int PacketNumber = buffer.ReadInteger();

            Type thisType = Type.GetType("ProcessData");

            data = Data;
            object[] obj = new object[1];
            switch (Source)
            {
                case ConnectionType.GAMESERVER:
                    if (PacketNumber == 0 || !Enum.IsDefined(typeof(GameServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[ConnectionType.GAMESERVER].Socket == null)
                    {
                        return;
                    }
                    Log.log("Packet Received [#" + PacketNumber.ToString("000") + " N" + ((GameServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.LOGINSERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);
                    obj[0] = Source;
                    thisType.InvokeMember(((GameServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, obj);
                    break;
                case ConnectionType.LOGINSERVER:
                    if (PacketNumber == 0 || !Enum.IsDefined(typeof(LoginServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[ConnectionType.LOGINSERVER].Socket == null)
                    {
                        return;
                    }
                    Log.log("Packet Received [#" + PacketNumber.ToString("000") + " N" + ((LoginServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.GAMESERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);
                    obj[0] = Source;
                    thisType.InvokeMember(((LoginServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, obj);
                    break;
                default:
                    break;
            }

            Reset();
        }

        #region Server Communication
        private static void AuthenticateServer(ConnectionType type)
        {
            if (buffer.ReadString() == Network.instance.AuthenticationCode)
            {
                // Confirmed to be the correct server, proceed with unblocking the client communication channels
                Network.instance.Servers[type].Authenticated = true;
            }
        }
        #endregion
        
        #region Login Server Communication
        private static void WhiteList()
        {
            string ip = buffer.ReadString();
            int LineNumber = Log.log("Checking white list for IP: " + ip + "..", Log.LogType.RECEIVED);
            if (!Network.instance.CheckWhiteList(ip))
            {
                Network.instance.WhiteList.Add(ip);
                Log.log(LineNumber, "Client IP: " + ip + " added successfully, sending confirmation to login server.", Log.LogType.RECEIVED);
                SendData.ConfirmWhiteList(ip);
            }
            else
            {
                Log.log(LineNumber, "Client IP: " + ip + " was already white listed.", Log.LogType.WARNING);
            }
        }
        #endregion

        #region Synchronization Server Communication

        #endregion
    }
}
