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
    public enum GameServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer,
        WorldRequest
    }
    class SendData
    {
        private static void sendData(ConnectionType destination, string PacketName, byte[] data)
        {
            try
            {
                switch (destination)
                {
                    case ConnectionType.GAMESERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    case ConnectionType.LOGINSERVER:
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

        private static void BuildBasePacket(int packetNumber, ref ByteBuffer.ByteBuffer buffer)
        {
            buffer.WriteInteger((int)ConnectionType.SYNCSERVER);
            buffer.WriteInteger(packetNumber);
        }
        #region Generic
        public static void Authenticate(Connection connection)
        {
            if (!connection.Authenticated)
            {
                try
                {
                    ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                    BuildBasePacket((int)ServerSendPacketNumbers.AuthenticateGameServer, ref buffer);
                    buffer.WriteString(Network.instance.AuthenticationCode);
                    sendData(connection.Type, ServerSendPacketNumbers.AuthenticateGameServer.ToString(), buffer.ToArray());
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
        #endregion

        #region Game Server
        public static void WorldRequest()
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                Log.log("Sending world request response.", Log.LogType.SENT);
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest, ref buffer);
                // tbl_Characters
                int LineNumber = Log.log("Sending tbl_Characters..", Log.LogType.SENT);

                buffer.WriteInteger(Data.tbl_Characters.Count);
                foreach (KeyValuePair<int, _Characters> character in Data.tbl_Characters)
                {
                    buffer.WriteInteger(character.Key);
                    buffer.WriteString(character.Value.Character_Name);
                    buffer.WriteInteger(character.Value.Character_Level);
                    buffer.WriteFloat(character.Value.Pos_X);
                    buffer.WriteFloat(character.Value.Pos_Y);
                    buffer.WriteFloat(character.Value.Pos_Z);
                    Log.log(LineNumber, "Sending tbl_Characters.. Character ID " + character.Key.ToString() + "/" + Data.tbl_Characters.Count.ToString(), Log.LogType.SENT);
                }
                
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString(), buffer.ToArray());

                // 
            }
            catch (Exception e)
            {
                Log.log("Building World update packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion
    }
}
