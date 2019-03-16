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
        ConfirmWhiteList
    }
    public enum GameServerSendPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WhiteList
    }
    public enum SyncServerSendPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        RegistrationNotification
    }
    class SendData : Data
    {
        private static void sendData(ConnectionType destination, int PacketNumber)
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
                        Network.instance.Clients[Index].Stream.BeginWrite(data, 0, data.Length, null, null);
                        Log.log("Packet Sent     [#" + PacketNumber.ToString("000") + " " + ((ClientSendPacketNumbers)PacketNumber).ToString() + "] to Client Index " + Index.ToString() + ".", Log.LogType.SENT);
                        break;
                    case ConnectionType.SYNCSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
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
        private static void BuildBasePacket(int packetNumber)
        {
            Reset(true);
            buffer.WriteInteger((int)ConnectionType.LOGINSERVER);
            buffer.WriteInteger(packetNumber);
        }

        #region Client Send Packets
        public static void LoginResponse(int index, byte response)
        {
            try
            {
                Index = index;
                BuildBasePacket((int)ClientSendPacketNumbers.LoginResponse);
                buffer.WriteByte(response);
                data = buffer.ToArray();
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.LoginResponse);
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
                Index = index;
                BuildBasePacket((int)ClientSendPacketNumbers.ConfirmWhiteList);

                data = buffer.ToArray();
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.ConfirmWhiteList);
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
                Index = index;
                BuildBasePacket((int)ClientSendPacketNumbers.RegistrationResponse);
                buffer.WriteByte(success);
                buffer.WriteString(response);
                data = buffer.ToArray();
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.RegistrationResponse);
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
                Index = index;
                BuildBasePacket((int)ClientSendPacketNumbers.CharacterList);
                buffer.WriteByte((success) ? (byte)1 : (byte)0);
                buffer.WriteInteger(Network.instance.Clients[Index].Characters.Count);
                if (success)
                {
                    foreach (Character c in Network.instance.Clients[Index].Characters)
                    {
                        buffer.WriteString(c.Name);
                        buffer.WriteInteger(c.Level);
                    }
                }
                data = buffer.ToArray();
                sendData(ConnectionType.CLIENT, (int)ClientSendPacketNumbers.CharacterList);
            }
            catch (Exception e)
            {
                Log.log("Building Character List packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion

        #region Game Server Send Packets
        public static void WhiteListConfirmation(string ip)
        {
            try
            {
                BuildBasePacket((int)GameServerSendPacketNumbers.WhiteList);
                buffer.WriteString(ip);
                data = buffer.ToArray();
                sendData(ConnectionType.GAMESERVER, (int)GameServerSendPacketNumbers.WhiteList);
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
                BuildBasePacket((int)SyncServerSendPacketNumbers.RegistrationNotification);
                buffer.WriteInteger(account_id);
                buffer.WriteString(username);
                buffer.WriteString(password);
                buffer.WriteString(email);
                data = buffer.ToArray();
                sendData(ConnectionType.SYNCSERVER, (int)SyncServerSendPacketNumbers.RegistrationNotification);
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
