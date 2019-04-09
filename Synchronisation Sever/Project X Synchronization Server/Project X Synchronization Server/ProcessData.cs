using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Project_X_Synchronization_Server
{
    public enum ClientProcessPacketNumbers
    {
        Invalid,
        LoginRequest,
        RegistrationRequest,
        CharacterListRequest
    }
    public enum LoginServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer
    }
    public enum GameServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WorldRequest,
        UpdatePlayerData
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
        public static void processData(byte[] data)
        {
            lock (lockObj)
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                buffer.WriteBytes(data);

                ConnectionType Source = (ConnectionType)buffer.ReadInteger();
                int PacketNumber = buffer.ReadInteger();

                Type thisType = Type.GetType("ProcessData");
                
                object[] obj = new object[2];
                switch (Source)
                {
                    case ConnectionType.GAMESERVER:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(GameServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[ConnectionType.GAMESERVER].Socket == null)
                        {
                            return;
                        }
                        //Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((GameServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.LOGINSERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);
                        obj[0] = Source;
                        obj[1] = data;
                        typeof(ProcessData).InvokeMember(((GameServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    case ConnectionType.LOGINSERVER:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(LoginServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[ConnectionType.LOGINSERVER].Socket == null)
                        {
                            return;
                        }
                        Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((LoginServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.GAMESERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);
                        obj[0] = Source;
                        obj[1] = data;
                        typeof(ProcessData).InvokeMember(((LoginServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ReadHeader(ref ByteBuffer.ByteBuffer buffer)
        {
            ConnectionType Source = (ConnectionType)buffer.ReadInteger();
            int PacketNumber = buffer.ReadInteger();
        }

        #region Server Communication
        private static void AuthenticateServer(ConnectionType type, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            if (buffer.ReadString() == Network.instance.AuthenticationCode)
            {
                // Confirmed to be the correct server, proceed with unblocking the client communication channels
                Network.instance.Servers[type].Authenticated = true;
            }
        }
        #endregion

        #region Game Server Communication
        private static void WorldRequest(ConnectionType type, byte[] data)
        {
            Log.log("Recevied request from game server for update request.", Log.LogType.RECEIVED);
            SendData.WorldRequest();
        }
        private static void UpdatePlayerData(ConnectionType type, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            // ID
            int Character_ID = buffer.ReadInteger();
            // Position
            float x = buffer.ReadFloat();
            float y = buffer.ReadFloat();
            float z = buffer.ReadFloat();
            float r = buffer.ReadFloat();
            // Camera Position
            float cx = buffer.ReadFloat();
            float cy = buffer.ReadFloat();
            float cz = buffer.ReadFloat();
            float cr = buffer.ReadFloat();
            // Player
            Data.tbl_Characters[Character_ID].Pos_X = x;
            Data.tbl_Characters[Character_ID].Pos_Y = y;
            Data.tbl_Characters[Character_ID].Pos_Z = z;
            Data.tbl_Characters[Character_ID].Rotation_Y = r;
            // Camera
            Data.tbl_Characters[Character_ID].Camera_Pos_X = cx;
            Data.tbl_Characters[Character_ID].Camera_Pos_Y = cy;
            Data.tbl_Characters[Character_ID].Camera_Pos_Z = cz;
            Data.tbl_Characters[Character_ID].Camera_Rotation_Y = cr;
        }
        #endregion

        #region Synchronization Server Communication

        #endregion
    }
}
