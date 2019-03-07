using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Project_X_Login_Server
{
    public enum ClientProcessPacketNumbers
    {
        Invalid,
        LoginRequest,
        RegistrationRequest,
        CharacterListRequest
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
        public static void processData(int index, byte[] Data)
        {
            buffer.WriteBytes(data);

            ConnectionType Source = (ConnectionType)buffer.ReadByte();
            int PacketNumber = buffer.ReadInteger();

            Type thisType = Type.GetType("ProcessData");

            data = Data;
            switch (Source)
            {
                case ConnectionType.GAMESERVER:
                    if (PacketNumber == 0 || !Enum.IsDefined(typeof(GameServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[index.ToString()].Socket == null)
                    {
                        return;
                    }
                    Log.log("Packet Received [#" + PacketNumber.ToString("000") + " N" + ((GameServerProcessPacketNumbers)PacketNumber).ToString() + "] from Game Server, Processing response..", Log.LogType.RECEIVED);
                    object[] obj = new object[1];
                    obj[0] = Source;
                    thisType.InvokeMember(((GameServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, obj);
                    break;
                case ConnectionType.CLIENT:
                    if (PacketNumber == 0 || !Enum.IsDefined(typeof(ClientProcessPacketNumbers), PacketNumber) || Network.instance.Clients[index].Socket == null)
                    {
                        return;
                    }
                    Log.log("Packet Received [#" + PacketNumber.ToString("000") + " N" + ((ClientProcessPacketNumbers)PacketNumber).ToString() + "] from Client Index " + Index.ToString() + ", Processing response..", Log.LogType.RECEIVED);
                    thisType.InvokeMember(((ClientProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
                    break;
                case ConnectionType.LOGINSERVER:
                    break;
                case ConnectionType.SYNCSERVER:
                    break;
                default:
                    break;
            }
            
            Reset();
        }
        #region Client Communication
        private static void LoginRequest()
        {
            string username = buffer.ReadString();
            string password = buffer.ReadString();
            Response r = Database.instance.Login(username, password);
            switch (r)
            {
                case Response.SUCCESSFUL:
                    break;
                case Response.UNSUCCESSFUL:
                    break;
                case Response.ERROR:
                    break;
                default:
                    break;
            }
        }
        private static void RegistrationRequest()
        {
            string username = buffer.ReadString();
            string password = buffer.ReadString();
            string email = buffer.ReadString();
            string response = "";
            Response r = Database.instance.RequestRegistration(username, password, email, out response);
            switch (r)
            {
                case Response.SUCCESSFUL:
                    if (response == "The account was setup successfully.")
                    {
                        // Return confirmation
                    }
                    else
                    {
                        // Registration unsuccessful (return message from DB)
                    }
                    break;
                case Response.UNSUCCESSFUL:
                    // Unsuccessful
                    break;
                case Response.ERROR:
                    // Unsuccessful
                    break;
                default:
                    break;
            }
        }
        private static void CharacterListRequest()
        {
            string username = buffer.ReadString();
            Response r = Database.instance.GetCharacters(username, Index);
            switch (r)
            {
                case Response.SUCCESSFUL:
                    break;
                case Response.UNSUCCESSFUL:
                    break;
                case Response.ERROR:
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Server Communication
        private static void AuthenticateServer(ConnectionType type)
        {
            if (buffer.ReadString() == Network.instance.AuthenticationCode)
            {
                if (type == ConnectionType.GAMESERVER)
                {
                    // Confirmed to be the game server, proceed with unblocking the client communication channels
                    Network.instance.GameServerAuthenticated = true;
                    Network.instance.Servers[Index.ToString()].Authenticated = true;
                    Network.instance.Servers.Add("Game Server", Network.instance.Servers[Index.ToString()]);
                    Network.instance.Servers.Remove(Index.ToString());
                }
                else if (type == ConnectionType.SYNCSERVER)
                {
                    // Confirmed to be the sync server, proceed with unblocking the client communication channels
                    Network.instance.SyncServerAuthenticated = true;
                    Network.instance.Servers[Index.ToString()].Authenticated = true;
                    Network.instance.Servers.Add("Synchronization Server", Network.instance.Servers[Index.ToString()]);
                    Network.instance.Servers.Remove(Index.ToString());
                }
            }
        }
        #endregion

        #region Game Server Communication

        #endregion

        #region Synchronization Server Communication

        #endregion
    }
}
