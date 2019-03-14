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
        private static void sendData(ConnectionType destination)
        {
            try
            {
                buffer.WriteBytes(data);
                switch (destination)
                {
                    case ConnectionType.GAMESERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                        break;
                    case ConnectionType.CLIENT:
                        Network.instance.Clients[Index].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                        break;
                    case ConnectionType.SYNCSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
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

            Reset(true);
        }
        private static void BuildBasePacket(int packetNumber)
        {
            buffer.WriteByte((byte)ConnectionType.LOGINSERVER);
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
                sendData(ConnectionType.CLIENT);
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
                sendData(ConnectionType.CLIENT);
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
                sendData(ConnectionType.CLIENT);
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
                BuildBasePacket((int)ClientSendPacketNumbers.RegistrationResponse);
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
                else
                {

                }
                data = buffer.ToArray();
                sendData(ConnectionType.CLIENT);
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
                sendData(ConnectionType.GAMESERVER);
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
                sendData(ConnectionType.SYNCSERVER);
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
