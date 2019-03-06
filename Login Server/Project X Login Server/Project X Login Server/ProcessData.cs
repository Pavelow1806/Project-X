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
    public enum ServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateGameServer
    }
    class ProcessData
    {
        private static ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();

        private static byte[] Data = null;
        private static int Index = -1;

        public static void processData(int index, byte[] data)
        {
            buffer.WriteBytes(data);

            ConnectionType Source = (ConnectionType)buffer.ReadByte();
            int PacketNumber = buffer.ReadInteger();

            Type thisType = Type.GetType("ProcessData");

            Data = data;

            if (Source == ConnectionType.CLIENT)
            {
                if (PacketNumber == 0 || !Enum.IsDefined(typeof(ClientProcessPacketNumbers), PacketNumber) || Network.instance.Clients[index].Socket == null)
                {
                    return;
                }

                thisType.InvokeMember(((ClientProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
            }
            else if (Source == ConnectionType.GAMESERVER)
            {
                if (PacketNumber == 0 || !Enum.IsDefined(typeof(ServerProcessPacketNumbers), PacketNumber) || Network.instance.GameServer.Socket == null)
                {
                    return;
                }

                thisType.InvokeMember(((ServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
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

        #region Game Server Communication
        private static void AuthenticateGameServer()
        {
            if (buffer.ReadString() == Network.instance.AuthenticationCode)
            {
                // Confirmed to be the game server, proceed with unblocking the client communication channels
                Network.instance.GameServerAuthenticated = true;
            }
        }
        #endregion

        private static void Reset()
        {
            Index = -1;
            Data = null;
            buffer = new ByteBuffer.ByteBuffer();
        }
    }
}
