using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Project_X_Synchronization_Server
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
        AuthenticateGameServer
    }
    class SendData : Data
    {
        private static void sendData(ConnectionType destination, ConnectionType type, string PacketName)
        {
            try
            {
                buffer.WriteBytes(data);
                switch (destination)
                {
                    case ConnectionType.GAMESERVER:
                        Network.instance.Servers[type].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                        break;
                    case ConnectionType.LOGINSERVER:
                        Network.instance.Servers[type].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
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
            buffer.WriteByte((byte)ConnectionType.SYNCSERVER);
            buffer.WriteInteger(packetNumber);
        }

        public static void Authenticate(Connection connection)
        {
            if (!connection.Authenticated)
            {
                try
                {
                    BuildBasePacket((int)ServerSendPacketNumbers.AuthenticateGameServer);
                    buffer.WriteString(Network.instance.AuthenticationCode);
                    data = buffer.ToArray();
                    sendData(ConnectionType.LOGINSERVER, connection.Type, ServerSendPacketNumbers.AuthenticateGameServer.ToString());
                }
                catch (Exception e)
                {
                    Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                    return;
                }
            }
            else
            {
                Log.log("An attempt was made to send an authentication packet, the " + connection.Type.ToString() + " is already authenticated.", Log.LogType.ERROR);
            }
        }
    }
}
