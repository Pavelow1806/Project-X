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
        #region Locking
        private static readonly object lockObj = new object();
        #endregion
        public static void processData(int index, byte[] Data)
        {
            lock (lockObj)
            {
                try
                {
                    buffer.WriteBytes(Data);

                    ConnectionType Source = (ConnectionType)buffer.ReadByte();
                    int PacketNumber = buffer.ReadInteger();

                    Type thisType = Type.GetType("ProcessData");

                    Index = index;
                    data = Data;
                    object[] obj;
                    switch (Source)
                    {
                        case ConnectionType.GAMESERVER:
                            if (PacketNumber == 0 || !Enum.IsDefined(typeof(GameServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[(ConnectionType)Index].Socket == null)
                            {
                                return;
                            }
                            Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((GameServerProcessPacketNumbers)PacketNumber).ToString() + "] from Game Server, Processing response..", Log.LogType.RECEIVED);

                            obj = new object[1];
                            obj[0] = Source;

                            typeof(ProcessData).InvokeMember(((GameServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                            break;
                        case ConnectionType.CLIENT:
                            if (PacketNumber == 0 || !Enum.IsDefined(typeof(ClientProcessPacketNumbers), PacketNumber) || Network.instance.Clients[index].Socket == null)
                            {
                                return;
                            }
                            Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((ClientProcessPacketNumbers)PacketNumber).ToString() + "] from Client Index " + Index.ToString() + ", Processing response..", Log.LogType.RECEIVED);

                            obj = new object[1];
                            obj[0] = Source;

                            typeof(ProcessData).InvokeMember(((SyncServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                            break;
                        case ConnectionType.LOGINSERVER:
                            break;
                        case ConnectionType.SYNCSERVER:
                            if (PacketNumber == 0 || !Enum.IsDefined(typeof(SyncServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[(ConnectionType)Index].Socket == null)
                            {
                                return;
                            }
                            Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((SyncServerProcessPacketNumbers)PacketNumber).ToString() + "] from Synchronization Server, Processing response..", Log.LogType.RECEIVED);

                            obj = new object[1];
                            obj[0] = Source;

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
                Reset();
            }
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
                Network.instance.SyncServerAuthenticated = true;
                Network.instance.Servers[(ConnectionType)Index].Authenticated = true;
                Network.instance.Servers.Add(type, Network.instance.Servers[(ConnectionType)Index]);
                Network.instance.Servers.Remove((ConnectionType)Index);
                Network.instance.Servers[type].Index = (int)type;
                // Send DB updates
                Database.instance.LogActivity(Network.instance.Servers[type].Username, Activity.AUTHENTICATED, Network.instance.Servers[type].SessionID);
            }
        }
        #endregion

        #region Game Server Communication

        #endregion

        #region Synchronization Server Communication

        #endregion
    }
}
