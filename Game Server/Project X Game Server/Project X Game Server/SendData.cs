using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Project_X_Game_Server
{
    public enum ClientSendPacketNumbers
    {
        Invalid,
        LoginResponse,
        RegistrationResponse,
        CharacterList
    }
    public enum ServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer
    }
    public enum LoginServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer,
        ConfirmWhiteList
    }
    class SendData : Data
    {
        private static void sendData(ConnectionType destination, string PacketName)
        {
            try
            {
                buffer.WriteBytes(data);
                switch (destination)
                {
                    case ConnectionType.GAMESERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                        break;
                    case ConnectionType.LOGINSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                        break;
                    default:
                        break;
                }
                Log.log("Successfully sent packet (" + PacketName + ") to " + destination.ToString(), Log.LogType.SENT);
            }
            catch (Exception e)
            {
                // Output error message
                Log.log("An error occured when attempting to send data:", Log.LogType.ERROR);
                Log.log("     Destination   > " + destination.ToString(), Log.LogType.ERROR);
                Log.log("     Error Message > " + e.Message, Log.LogType.ERROR);
            }

            Reset();
        }

        private static void BuildBasePacket(int packetNumber)
        {
            buffer.WriteByte((byte)ConnectionType.GAMESERVER);
            buffer.WriteInteger(packetNumber);
        }
        #region Generic
        public static void Authenticate(Connection server)
        {
            if (!server.Authenticated)
            {
                try
                {
                    BuildBasePacket((int)ServerSendPacketNumbers.AuthenticateGameServer);
                    buffer.WriteString(Network.instance.AuthenticationCode);
                    data = buffer.ToArray();
                    sendData(server.Type, ServerSendPacketNumbers.AuthenticateGameServer.ToString());
                }
                catch (Exception e)
                {
                    Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                    return;
                }
            }
            else
            {
                Log.log("An attempt was made to send an authentication packet, the " + server.Type.ToString() + " is already authenticated.", Log.LogType.ERROR);
            }
        }
        #endregion

        #region Login Server
        public static void ConfirmWhiteList(string ip)
        {
            try
            {
                BuildBasePacket((int)LoginServerSendPacketNumbers.ConfirmWhiteList);
                buffer.WriteString(ip);
                data = buffer.ToArray();
                sendData(ConnectionType.LOGINSERVER, LoginServerSendPacketNumbers.ConfirmWhiteList.ToString());
                Log.log("Sent white list confirmation of IP: " + ip, Log.LogType.SENT);
            }
            catch (Exception e)
            {
                Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion

        #region Game Server

        #endregion
    }
}
