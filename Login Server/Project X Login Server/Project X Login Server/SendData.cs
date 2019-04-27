using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Login_Server
{
    public enum ClientSendPacketNumbers
    {
        Invalid,
        LoginResponse,
        RegistrationResponse,
        CharacterList,
        ConfirmWhiteList,
        CreateCharacterResponse
    }
    public enum GameServerSendPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WhiteList,
        CreateCharacterResponse
    }
    public enum SyncServerSendPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        RegistrationNotification,
        CreateCharacterResponse
    }
    class SendData
    {
        private static void sendData(ConnectionType destination, int PacketNumber, int index, byte[] data)
        {
            try
            {
                switch (destination)
                {
                    case ConnectionType.GAMESERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
                        Log.log("Packet Sent     [#" + PacketNumber.ToString("000") + " " + ((GameServerSendPacketNumbers)PacketNumber).ToString() + "] to Game Server.", Log.LogType.SENT);
                        break;
                    case ConnectionType.CLIENT:
                        Network.instance.Clients[index].Stream.BeginWrite(data, 0, data.ToArray().Length, null, null);
                        Log.log("Packet Sent     [#" + PacketNumber.ToString("000") + " " + ((ClientSendPacketNumbers)PacketNumber).ToString() + "] to Client Index " + index.ToString() + ".", Log.LogType.SENT);
                        break;
                    case ConnectionType.SYNCSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.ToArray().Length, null, null);
                        Log.log("Packet Sent     [#" + PacketNumber.ToString("000") + " " + ((SyncServerSendPacketNumbers)PacketNumber).ToString() + "] to Synchronization Server.", Log.LogType.SENT);
                        break;
                    default:
                        break;
                }
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
            buffer.WriteInteger((int)ConnectionType.LOGINSERVER);
            buffer.WriteInteger(packetNumber);
        }

        #region Client Send Packets
        public static void LoginResponse(int index, byte response)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.LoginResponse, ref buffer);
                buffer.WriteByte(response);
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.LoginResponse, index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Login Response packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void ConfirmWhiteList(int index)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.ConfirmWhiteList, ref buffer);
                
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.ConfirmWhiteList, index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building White List Confirmation packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void RegistrationResponse(int index, byte success, string response)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.RegistrationResponse, ref buffer);
                buffer.WriteByte(success);
                buffer.WriteString(response);
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.RegistrationResponse, index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Registration Response packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void CharacterList(int index, bool success)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.CharacterList, ref buffer);
                buffer.WriteByte((success) ? (byte)1 : (byte)0);
                buffer.WriteInteger(Network.instance.Clients[index].Characters.Count);
                if (success)
                {
                    foreach (Character c in Network.instance.Clients[index].Characters)
                    {
                        buffer.WriteString(c.Name);
                        buffer.WriteInteger(c.Level);
                        buffer.WriteInteger(c.Gender);
                    }
                }
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.CharacterList, index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Character List packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void CreateCharacterResponse(int index, int Character_ID, string Name, int Gender)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.CreateCharacterResponse, ref buffer);
                buffer.WriteInteger(Character_ID);
                buffer.WriteString(Name);
                buffer.WriteInteger(Gender);
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.CreateCharacterResponse, index, buffer.ToArray());


                if (Character_ID > 0)
                {
                    buffer = new ByteBuffer.ByteBuffer();
                    BuildBasePacket((int)GameServerSendPacketNumbers.CreateCharacterResponse, ref buffer);
                    buffer.WriteInteger(Character_ID);
                    buffer.WriteString(Name);
                    buffer.WriteInteger(Gender);
                    sendData(ConnectionType.GAMESERVER, (int)GameServerSendPacketNumbers.CreateCharacterResponse, index, buffer.ToArray());

                    buffer = new ByteBuffer.ByteBuffer();
                    BuildBasePacket((int)SyncServerSendPacketNumbers.CreateCharacterResponse, ref buffer);
                    buffer.WriteInteger(Character_ID);
                    buffer.WriteString(Name);
                    buffer.WriteInteger(Gender);
                    buffer.WriteString(Network.instance.Clients[index].Username);
                    sendData(ConnectionType.SYNCSERVER, (int)SyncServerSendPacketNumbers.CreateCharacterResponse, index, buffer.ToArray());
                }
            }
            catch (Exception e)
            {
                Log.log("Building Create Character Response failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion

        #region Game Server Send Packets
        public static void WhiteListConfirmation(string ip)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)GameServerSendPacketNumbers.WhiteList, ref buffer);
                buffer.WriteString(ip);

                sendData(ConnectionType.GAMESERVER, (int)GameServerSendPacketNumbers.WhiteList, -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building White List Confirmation packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }  
        }
        #endregion

        #region Sync Server Send Packets
        public static void RegistrationNotification(int account_id, string username, string password, string email)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.RegistrationNotification, ref buffer);
                buffer.WriteInteger(account_id);
                buffer.WriteString(username);
                buffer.WriteString(password);
                buffer.WriteString(email);
                sendData(ConnectionType.SYNCSERVER, (int)SyncServerSendPacketNumbers.RegistrationNotification, -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Registration Notification packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion
    }
}
