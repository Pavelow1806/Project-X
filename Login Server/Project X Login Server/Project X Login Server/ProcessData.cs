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
        CharacterListRequest,
        CreateCharacter
    }
    public enum GameServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        ConfirmWhiteList
    }
    public enum SyncServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer
    }
    class ProcessData
    {
        #region Locking
        private static readonly object lockObj = new object();
        #endregion
        public static void processData(int index, byte[] data)
        {
            lock (lockObj)
            {
                try
                {
                    ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                    buffer.WriteBytes(data);

                    ConnectionType Source = (ConnectionType)buffer.ReadInteger();
                    int PacketNumber = buffer.ReadInteger();

                    object[] obj;
                    switch (Source)
                    {
                        case ConnectionType.GAMESERVER:
                            if (PacketNumber == 0 || !Enum.IsDefined(typeof(GameServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[(ConnectionType)index].Socket == null)
                            {
                                return;
                            }
                            Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((GameServerProcessPacketNumbers)PacketNumber).ToString() + "] from Game Server, Processing response..", Log.LogType.RECEIVED);

                            obj = new object[3];
                            obj[0] = Source;
                            obj[1] = index;
                            obj[2] = data;

                            typeof(ProcessData).InvokeMember(((GameServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                            break;
                        case ConnectionType.CLIENT:
                            if (PacketNumber == 0 || !Enum.IsDefined(typeof(ClientProcessPacketNumbers), PacketNumber) || Network.instance.Clients[index].Socket == null)
                            {
                                return;
                            }
                            Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((ClientProcessPacketNumbers)PacketNumber).ToString() + "] from Client Index " + index.ToString() + ", Processing response..", Log.LogType.RECEIVED);

                            obj = new object[3];
                            obj[0] = Source;
                            obj[1] = index;
                            obj[2] = data;

                            typeof(ProcessData).InvokeMember(((ClientProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                            break;
                        case ConnectionType.SYNCSERVER:
                            if (PacketNumber == 0 || !Enum.IsDefined(typeof(SyncServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[(ConnectionType)index].Socket == null)
                            {
                                return;
                            }
                            Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((SyncServerProcessPacketNumbers)PacketNumber).ToString() + "] from Synchronization Server, Processing response..", Log.LogType.RECEIVED);

                            obj = new object[3];
                            obj[0] = Source;
                            obj[1] = index;
                            obj[2] = data;

                            typeof(ProcessData).InvokeMember(((SyncServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                            break;
                        default:
                            break;

                    }
                }
                catch (Exception e)
                {
                    Log.log("An error occurred when attempting to process a packet. > " + e.Message, Log.LogType.ERROR);
                }
            }
        }

        private static void ReadHeader(ref ByteBuffer.ByteBuffer buffer)
        {
            ConnectionType Source = (ConnectionType)buffer.ReadInteger();
            int PacketNumber = buffer.ReadInteger();
        }

        #region Client Communication
        private static void LoginRequest(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            string username = buffer.ReadString();
            string password = buffer.ReadString();
            Response r = Database.instance.Login(username, password);
            switch (r)
            {
                case Response.SUCCESSFUL:
                    Network.instance.Clients[index].LoggedIn = true;
                    Network.instance.Clients[index].Username = username;
                    SendData.LoginResponse(index, 1);
                    SendData.WhiteListConfirmation(Network.instance.Clients[index].IP);
                    break;
                case Response.UNSUCCESSFUL:
                    SendData.LoginResponse(index, 0);
                    break;
                case Response.ERROR:
                    SendData.LoginResponse(index, 2);
                    break;
                default:
                    break;
            }
        }
        private static void RegistrationRequest(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int LineNumber = Log.log("Registration request received from index: " + index + ", checking database.", Log.LogType.RECEIVED);
            string username = buffer.ReadString();
            string password = buffer.ReadString();
            string email = buffer.ReadString();
            string response = "";
            int account_id = -1;
            Response r = Database.instance.RequestRegistration(username, password, email, out response, out account_id);
            switch (r)
            {
                case Response.SUCCESSFUL:
                    if (response == "The account was setup successfully." && account_id > -1)
                    {
                        // Return confirmation
                        Log.log(LineNumber, "Registration of account was successful, account with ID: " + account_id + " and Username: " + username + " created, sending response.", Log.LogType.RECEIVED);
                        SendData.RegistrationResponse(index, 1, response);
                        SendData.RegistrationNotification(account_id, username, password, email);
                    }
                    else
                    {
                        // Registration unsuccessful (return message from DB)
                        Log.log(LineNumber, "Registration of account was unsuccessful, account with Username: " + username + " not created, sending response.", Log.LogType.RECEIVED);
                        SendData.RegistrationResponse(index, 0, response);
                    }
                    break;
                case Response.UNSUCCESSFUL:
                    Log.log(LineNumber, "Registration of account was unsuccessful, account with Username: " + username + " not created, sending response.", Log.LogType.RECEIVED);
                    SendData.RegistrationResponse(index, 0, response);
                    // Unsuccessful
                    break;
                case Response.ERROR:
                    Log.log(LineNumber, "Registration of account was unsuccessful, account with Username: " + username + " not created, sending response, please fix errors.", Log.LogType.ERROR);
                    SendData.RegistrationResponse(index, 0, response);
                    // Unsuccessful
                    break;
            }
        }
        private static void CharacterListRequest(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            string username = buffer.ReadString();
            Response r = Database.instance.GetCharacters(username, index);
            switch (r)
            {
                case Response.SUCCESSFUL:
                    SendData.CharacterList(index, true);
                    break;
                case Response.UNSUCCESSFUL:
                    SendData.CharacterList(index, false);
                    break;
                case Response.ERROR:
                    SendData.CharacterList(index, false);
                    break;
                default:
                    break;
            }
        }
        private static void CreateCharacter(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            string Character_Name = buffer.ReadString();
            int Gender = buffer.ReadInteger();
            int Character_ID = Database.instance.CreateCharacter(Network.instance.Clients[index].Username, Character_Name, Gender);
            SendData.CreateCharacterResponse(index, Character_ID, Character_Name, Gender);
        }
        #endregion

        #region Server Communication
        private static void AuthenticateServer(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            if (buffer.ReadString() == Network.instance.AuthenticationCode)
            {
                if (type == ConnectionType.SYNCSERVER)
                {
                    Network.instance.SyncServerAuthenticated = true;
                }
                else if (type == ConnectionType.GAMESERVER)
                {
                    Network.instance.GameServerAuthenticated = true;
                }
                Network.instance.Servers[(ConnectionType)index].Type = type;
                Network.instance.Servers[(ConnectionType)index].Authenticated = true;
                Network.instance.Servers.Add(type, Network.instance.Servers[(ConnectionType)index]);
                Network.instance.Servers.Remove((ConnectionType)index);
                Network.instance.Servers[type].Index = (int)type;
                // Send DB updates
                Database.instance.LogActivity(Network.instance.Servers[type].Username, Activity.AUTHENTICATED, Network.instance.Servers[type].SessionID);
            }
        }
        #endregion

        #region Game Server Communication
        private static void ConfirmWhiteList(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            string ip = buffer.ReadString();
            Log.log("White list of client with IP: " + ip + " confirmed. Notifying client.", Log.LogType.RECEIVED);
            SendData.ConfirmWhiteList(Network.instance.GetClient(ip));
        }
        #endregion

        #region Synchronization Server Communication

        #endregion
    }
}
