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
        AuthenticateServer,
        RegistrationNotification,
        CreateCharacterResponse
    }
    public enum GameServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WorldRequest,
        UpdatePlayerData,
        UpdateQuestLog,
        CreateQuestLog,
        ConnectivityData,
        LogActivity
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
        private static void UpdateQuestLog(ConnectionType type, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int Quest_ID = buffer.ReadInteger();
            int Character_ID = buffer.ReadInteger();
            int Objective_Progress = buffer.ReadInteger();
            int Quest_Status = buffer.ReadInteger();
            _Quest_Log ql = Data.ContainsKey(Character_ID, Quest_ID);
            if (ql != null)
            {
                ql.Quest_Status = Quest_Status;
                ql.Progress = Objective_Progress;
            }
            else
            {
                ql = new _Quest_Log(-1, Character_ID, Quest_ID, Quest_Status, 0);
                ql.Log_ID = Database.instance.Insert_Record("CALL CreateQuestLog(" + Quest_ID + ", " + Character_ID + ", 0, " + Quest_Status + ");");
                Data.tbl_Quest_Log.TryAdd(ql.Log_ID, ql);
            }
        }
        private static void CreateQuestLog(ConnectionType type, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int Quest_ID = buffer.ReadInteger();
            int Character_ID = buffer.ReadInteger();
            int Progress = buffer.ReadInteger();
            int Status = buffer.ReadInteger();
            int Log_ID = Database.instance.Insert_Record("CALL CreateQuestLog(" + Quest_ID + ", " + Character_ID + ", 0, " + Status + ");");
            if (!Data.tbl_Quest_Log.ContainsKey(Log_ID))
            {
                Data.tbl_Quest_Log.TryAdd(Log_ID, new _Quest_Log(Log_ID, Character_ID, Quest_ID, Status, Progress));
            }
            else
            {
                Data.tbl_Quest_Log[Log_ID].Quest_Status = Status;
                Data.tbl_Quest_Log[Log_ID].Progress = Progress;
            }
            SendData.NewQuestLog(Data.tbl_Quest_Log[Log_ID]);
        }
        private static void ConnectivityData(ConnectionType type, byte[] data)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                buffer.WriteBytes(data);
                ReadHeader(ref buffer);

                int Count = buffer.ReadInteger();
                for (int i = 0; i < Count; i++)
                {
                    int Character_ID = buffer.ReadInteger();
                    float TCP_Throughput = buffer.ReadFloat();
                    int TCP_Packets_Received = buffer.ReadInteger();
                    int TCP_Packets_Sent = buffer.ReadInteger();
                    float TCP_Latency = buffer.ReadFloat();
                    float UDP_Throughput = buffer.ReadFloat();
                    int UDP_Packets_Received = buffer.ReadInteger();
                    int UDP_Packets_Sent = buffer.ReadInteger();
                    float UDP_Latency = buffer.ReadFloat();
                    DateTime LogStart = Convert.ToDateTime(buffer.ReadString());
                    DateTime LogFinish = Convert.ToDateTime(buffer.ReadString());
                    Data.tbl_Connectivity.Add(new _Connectivity(Character_ID, LogStart,
                        TCP_Latency, TCP_Throughput, TCP_Packets_Sent, TCP_Packets_Received, UDP_Latency, UDP_Throughput, UDP_Packets_Sent, UDP_Packets_Received,
                        true));
                }
            }
            catch (Exception e)
            {
                Log.log("Connectivity Data threw an error " + e.Message);
            }
        }
        private static void LogActivity(ConnectionType type, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);

            int Account_ID = buffer.ReadInteger();
            int Activity = buffer.ReadInteger();
            DateTime DTStamp = Convert.ToDateTime(buffer.ReadString());
            string SessionID = buffer.ReadString();

            Data.tbl_Activity.Add(new _Activity(-1, Account_ID, (Activity)Activity, DTStamp, SessionID, true));
        }
        #endregion

        #region Login Server Communication
        private static void RegistrationNotification(ConnectionType type, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);

            int Account_ID = buffer.ReadInteger();
            string Username = buffer.ReadString();
            string Password = buffer.ReadString();
            string Email = buffer.ReadString();

            Data.tbl_Accounts.TryAdd(Account_ID, new _Accounts(Account_ID, Username, Email, Password, false));
        }
        private static void CreateCharacterResponse(ConnectionType type, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);

            int Character_ID = buffer.ReadInteger();
            string Name = buffer.ReadString();
            int Gender = buffer.ReadInteger();
            string Username = buffer.ReadString();

            Data.tbl_Characters.TryAdd(Character_ID, new _Characters(Character_ID, Data.GetAccountID(Username), Name, 1, Gender,
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 100, 10, 10, 0));
        }
        #endregion
    }
}
