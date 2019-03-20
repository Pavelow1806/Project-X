using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    public enum SyncServerSendPacketNumbers
    {
        Invalid,
        AuthenticateSyncServer,
        WorldRequest
    }
    class SendData : Data
    {
        public static bool PostUDPMessages = true;

        private static void sendData(ConnectionType destination, string PacketName)
        {
            try
            {
                switch (destination)
                {
                    case ConnectionType.CLIENT:
                        Network.instance.Clients[Index].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    case ConnectionType.LOGINSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    case ConnectionType.SYNCSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
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
        }

        private static void BuildBasePacket(int packetNumber)
        {
            Reset();
            buffer.WriteInteger((int)ConnectionType.GAMESERVER);
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
                Log.log("Sent white list confirmation of IP: " + ip, Log.LogType.SENT);
                sendData(ConnectionType.LOGINSERVER, LoginServerSendPacketNumbers.ConfirmWhiteList.ToString());
            }
            catch (Exception e)
            {
                Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion

        #region Sync Server
        public static void WorldRequest(int LineNumber)
        {
            try
            {
                BuildBasePacket((int)SyncServerSendPacketNumbers.WorldRequest);
                data = buffer.ToArray();
                Log.log(LineNumber, "Requesting world status update from synchronization server..", Log.LogType.SENT);
                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.WorldRequest.ToString());
            }
            catch (Exception e)
            {
                Log.log(LineNumber, "Building World Request packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion

        #region Client Communication
        public static void SendUDPPacket(Connection connection, byte[] data)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress Receiver = IPAddress.Parse(connection.IP.Substring(connection.IP.IndexOf(':')));
                IPEndPoint EndPoint = new IPEndPoint(Receiver, 5601);
                socket.SendTo(data, EndPoint);
                if (PostUDPMessages) Log.log("UDP Packet sent to IP: " + connection.IP.Substring(connection.IP.IndexOf(':')).ToString() + " Length: " + data.Length.ToString(), Log.LogType.SENT);
            }
            catch (Exception e)
            {
                Log.log("An error occurred when sending a UDP packet to IP: " + connection.IP.Substring(connection.IP.IndexOf(':')).ToString() + ". > " + e.Message, Log.LogType.ERROR);
            }
        }
        #endregion
    }
}
