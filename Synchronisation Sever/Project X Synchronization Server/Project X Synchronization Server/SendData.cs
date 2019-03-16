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
    class SendData : Data
    {
        private static void sendData(ConnectionType destination, string PacketName)
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

        private static void BuildBasePacket(int packetNumber)
        {
            Reset();
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
                    BuildBasePacket((int)ServerSendPacketNumbers.AuthenticateGameServer);
                    buffer.WriteString(Network.instance.AuthenticationCode);
                    data = buffer.ToArray();
                    sendData(connection.Type, ServerSendPacketNumbers.AuthenticateGameServer.ToString());
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
                Log.log("Sending world request response.", Log.LogType.SENT);
                BuildBasePacket((int)GameServerSendPacketNumbers.WorldRequest);
                // tbl_Characters
                int LineNumber = Log.log("Sending tbl_Characters..", Log.LogType.SENT);

                buffer.WriteInteger(tbl_Characters.Count);
                foreach (_Characters character in tbl_Characters)
                {
                    buffer.WriteInteger(character.Character_ID);
                    buffer.WriteString(character.Character_Name);
                    buffer.WriteInteger(character.Character_Level);
                    buffer.WriteFloat(character.Pos_X);
                    buffer.WriteFloat(character.Pos_Y);
                    buffer.WriteFloat(character.Pos_Z);
                    Log.log(LineNumber, "Sending tbl_Characters.. Character ID " + character.Character_ID.ToString() + "/" + tbl_Characters.Count.ToString(), Log.LogType.SENT);
                }

                data = buffer.ToArray();
                sendData(ConnectionType.GAMESERVER, GameServerSendPacketNumbers.WorldRequest.ToString());
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
