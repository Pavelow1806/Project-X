using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Project_X_Game_Server
{
    public enum ClientSendPacketNumbers
    {
        Invalid,
        WorldPacket,
        CharacterDetails,
        PlayerStateChange,
        QuestReturn,
        QuestInteractConfirm,
        CollectableInteractConfirm,
        CollectableToggle,
        AttackResponse,
        UpdateQuestLog,
        Attacked,
        Respawned,
        Heal,
        StompResponse
    }
    public enum ServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer
    }
    public enum LoginServerSendPacketNumbers
    {
        Invalid,
        AuthenticateGameServer,
        ConfirmWhiteList
    }
    public enum SyncServerSendPacketNumbers
    {
        Invalid,
        AuthenticateSyncServer,
        WorldRequest,
        UpdatePlayerData,
        UpdateQuestLog,
        CreateQuestLog,
        ConnectivityData,
        LogActivity
    }
    public enum PlayerState
    {
        Login,
        Logout,
        Update
    }
    public enum WorldSplit
    {
        Players,
        NPCs,
        Collectables,
        QuestLog
    }
    class SendData
    {
        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        int LineNumber = 0;

        public static bool PostUDPMessages = false;

        private static void sendData(ConnectionType destination, string PacketName, int index, byte[] data)
        {
            try
            {
                switch (destination)
                {
                    case ConnectionType.CLIENT:
                        Log.log("Successfully sent packet (" + PacketName + ") to " + destination.ToString() + " (" + data.Length + ")", Log.LogType.SENT);
                        Network.instance.Clients[index].Stream.BeginWrite(data, 0, data.Length, null, null);
                        ++Network.instance.Clients[index].TCP_PacketsSent;
                        break;
                    case ConnectionType.LOGINSERVER:
                        Log.log("Successfully sent packet (" + PacketName + ") to " + destination.ToString() + " (" + data.Length + ")", Log.LogType.SENT);
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
                        break;
                    case ConnectionType.SYNCSERVER:
                        Network.instance.Servers[destination].Stream.BeginWrite(data, 0, data.Length, null, null);
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
            buffer.WriteInteger((int)ConnectionType.GAMESERVER);
            buffer.WriteInteger(packetNumber);
        }
        #region Generic
        public static void Authenticate(Connection server)
        {
            if (!server.Authenticated)
            {
                try
                {
                    ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                    BuildBasePacket((int)ServerSendPacketNumbers.AuthenticateGameServer, ref buffer);
                    buffer.WriteString(Network.instance.AuthenticationCode);
                    sendData(server.Type, ServerSendPacketNumbers.AuthenticateGameServer.ToString(), -1, buffer.ToArray());
                }
                catch (Exception e)
                {
                    Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                    return;
                }
            }
            else
            {
                Log.log("An attempt was made to send an authentication packet, the " + server.Type.ToString() + " is already authenticated.", Log.LogType.ERROR);
            }
        }
        #endregion

        #region Login Server
        public static void ConfirmWhiteList(string ip)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)LoginServerSendPacketNumbers.ConfirmWhiteList, ref buffer);
                buffer.WriteString(ip);
                Log.log("Sent white list confirmation of IP: " + ip, Log.LogType.SENT);
                sendData(ConnectionType.LOGINSERVER, LoginServerSendPacketNumbers.ConfirmWhiteList.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Authentication packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion

        #region Sync Server
        public static void WorldRequest(int LineNumber)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.WorldRequest, ref buffer);
                Log.log(LineNumber, "Requesting world status update from synchronization server..", Log.LogType.SENT);
                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.WorldRequest.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log(LineNumber, "Building World Request packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void UpdatePlayerData(Player player)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.UpdatePlayerData, ref buffer);
                // ID
                buffer.WriteInteger(player.Character_ID);
                // Position
                buffer.WriteFloat(player.position.x);
                buffer.WriteFloat(player.position.y);
                buffer.WriteFloat(player.position.z);
                buffer.WriteFloat(player.r);
                // Camera Position
                buffer.WriteFloat(player.Camera_Pos_X);
                buffer.WriteFloat(player.Camera_Pos_Y);
                buffer.WriteFloat(player.Camera_Pos_Z);
                buffer.WriteFloat(player.Camera_Rotation_Y);
                // Experience
                buffer.WriteInteger(player.Level);
                buffer.WriteInteger(player.experience);
                buffer.WriteInteger(player.Max_HP);
                buffer.WriteInteger(player.Strength);
                buffer.WriteInteger(player.Agility);

                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.UpdatePlayerData.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Update Player Data packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void UpdateQuestLog(Quest_Log ql)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.UpdateQuestLog, ref buffer);
                buffer.WriteInteger(ql.Quest_ID);
                buffer.WriteInteger(ql.Character_ID);
                buffer.WriteInteger(ql.ObjectiveProgress);
                buffer.WriteInteger((int)ql.Status);

                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.UpdatePlayerData.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Update Quest Log packet failed. > " + e.Message, Log.LogType.ERROR);
            }
        }
        public static void CreateQuestLog(Quest_Log ql)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.CreateQuestLog, ref buffer);
                buffer.WriteInteger(ql.Quest_ID);
                buffer.WriteInteger(ql.Character_ID);
                buffer.WriteInteger(ql.ObjectiveProgress);
                buffer.WriteInteger((int)ql.Status);

                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.UpdatePlayerData.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Create Quest Log packet failed. > " + e.Message, Log.LogType.ERROR);
            }
        }
        public static void ConnectivityData(Dictionary<int, Connectivity> ConnectivityData)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.ConnectivityData, ref buffer);

                buffer.WriteInteger(ConnectivityData.Count);
                foreach (KeyValuePair<int, Connectivity> c in ConnectivityData)
                {
                    buffer.WriteInteger(c.Value.Character_ID);
                    buffer.WriteFloat(c.Value.TCP_Throughput);
                    buffer.WriteInteger(c.Value.TCP_PacketsReceived);
                    buffer.WriteInteger(c.Value.TCP_PacketsSent);
                    buffer.WriteFloat(c.Value.TCP_Latency);
                    buffer.WriteFloat(c.Value.UDP_Throughput);
                    buffer.WriteInteger(c.Value.UDP_PacketsReceived);
                    buffer.WriteInteger(c.Value.UDP_PacketsSent);
                    buffer.WriteFloat(c.Value.UDP_Latency);
                    buffer.WriteString(c.Value.LogStart.ToString());
                    buffer.WriteString(c.Value.LogFinish.ToString());
                }

                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.ConnectivityData.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Connectivity Data packet failed. > " + e.Message, Log.LogType.ERROR);
            }
        }
        public static void LogActivity(int Account_ID, Activity activity, string SessionID)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)SyncServerSendPacketNumbers.ConnectivityData, ref buffer);

                buffer.WriteInteger(Account_ID);
                buffer.WriteInteger((int)activity);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
                buffer.WriteString(SessionID);

                sendData(ConnectionType.SYNCSERVER, SyncServerSendPacketNumbers.ConnectivityData.ToString(), -1, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Connectivity Data packet failed. > " + e.Message, Log.LogType.ERROR);
            }
        }
        #endregion

        #region Client Communication
        public static void SendUDP_Packet(Connection connection, byte[] data)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress Receiver = IPAddress.Parse(connection.IP.Substring(0, connection.IP.IndexOf(':')));
                IPEndPoint EndPoint = new IPEndPoint(Receiver, Network.UDPPort);
                socket.SendTo(data, EndPoint);
                ++Network.instance.Clients[connection.Index].UDP_PacketsSent;
                if (PostUDPMessages) Log.log("UDP Packet sent to IP: " + connection.IP.Substring(0, connection.IP.IndexOf(':')) + " Length: " + data.Length.ToString(), Log.LogType.SENT);
            }
            catch (Exception e)
            {
                Log.log("An error occurred when sending a UDP packet to IP: " + connection.IP.Substring(connection.IP.IndexOf(':')).ToString() + ". > " + e.Message, Log.LogType.ERROR);
            }
        }
        public static void WorldPacket(int index)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            BuildBasePacket((int)ClientSendPacketNumbers.WorldPacket, ref buffer);
            buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            buffer.WriteInteger((int)WorldSplit.Players);
            // Players
            buffer.WriteInteger(World.instance.playersInWorld.Count); // Minus 1 due to the player receiving not getting their own details
            foreach (Player player in World.instance.playersInWorld)
            {
                buffer.WriteInteger(player.Character_ID);
                buffer.WriteInteger(player.Entity_ID);
                buffer.WriteString(player.Name);
                buffer.WriteInteger((int)player.gender);
                buffer.WriteInteger(player.Level);
                buffer.WriteInteger(player.Current_HP);
                buffer.WriteInteger(player.Max_HP);
                buffer.WriteFloat(player.position.x);
                buffer.WriteFloat(player.position.y);
                buffer.WriteFloat(player.position.z);
                buffer.WriteFloat(player.r);
            }
            sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.WorldPacket.ToString(), index, buffer.ToArray());
            Log.log("World Packet: Sent Players.", Log.LogType.SUCCESS);

            Thread.Sleep(1000);

            buffer = new ByteBuffer.ByteBuffer();
            BuildBasePacket((int)ClientSendPacketNumbers.WorldPacket, ref buffer);
            buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            buffer.WriteInteger((int)WorldSplit.NPCs);
            // NPCs
            if (Network.instance.Clients[index].Version != "")
                buffer.WriteInteger(World.instance.NPCsInWorld.Count);
            else
                buffer.WriteInteger(World.instance.GetOriginalNPCCount());
            foreach (NPC npc in World.instance.NPCsInWorld)
            {
                if (Network.instance.Clients[index].Version != "" || (Network.instance.Clients[index].Version == "" && npc.NPC_ID != 12))
                {
                    buffer.WriteInteger(npc.NPC_ID);
                    buffer.WriteInteger(npc.Entity_ID);
                    buffer.WriteString(npc.Name);
                    buffer.WriteInteger((int)npc.gender);
                    buffer.WriteInteger((int)npc.Status);
                    buffer.WriteInteger(npc.Level);
                    buffer.WriteInteger(npc.Current_HP);
                    buffer.WriteInteger(npc.Max_HP);
                    buffer.WriteFloat(npc.position.x);
                    buffer.WriteFloat(npc.position.y);
                    buffer.WriteFloat(npc.position.z);
                    buffer.WriteFloat(npc.r);
                    buffer.WriteInteger((int)World.instance.GetQuestStateByNPC(Network.instance.Clients[index].Character_ID, npc.NPC_ID));
                }
            }
            sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.WorldPacket.ToString(), index, buffer.ToArray());
            Log.log("World Packet: Sent NPC's.", Log.LogType.SUCCESS);

            Thread.Sleep(1000);

            buffer = new ByteBuffer.ByteBuffer();
            BuildBasePacket((int)ClientSendPacketNumbers.WorldPacket, ref buffer);
            buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            buffer.WriteInteger((int)WorldSplit.Collectables);
            // Collectables
            buffer.WriteInteger(World.instance.collectablesInWorld.Count);
            foreach (Collectable collectable in World.instance.collectablesInWorld)
            {
                buffer.WriteInteger(collectable.Collectable_ID);
                buffer.WriteInteger(collectable.Entity_ID);
                buffer.WriteString(collectable.Name);
                buffer.WriteInteger(collectable.Respawn_Time);
                buffer.WriteFloat(collectable.position.x);
                buffer.WriteFloat(collectable.position.y);
                buffer.WriteFloat(collectable.position.z);
                buffer.WriteFloat(collectable.r);
                buffer.WriteByte((collectable.Active) ? (byte)1 : (byte)0);
            }
            sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.WorldPacket.ToString(), index, buffer.ToArray());
            Log.log("World Packet: Sent Collectables.", Log.LogType.SUCCESS);

            Thread.Sleep(1000);

            buffer = new ByteBuffer.ByteBuffer();
            BuildBasePacket((int)ClientSendPacketNumbers.WorldPacket, ref buffer);
            buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            buffer.WriteInteger((int)WorldSplit.QuestLog);
            // Quest Log
            List<Quest_Log> QuestLog = World.instance.GetQuestLog(Network.instance.Clients[index].Character_ID);
            buffer.WriteInteger(QuestLog.Count);
            foreach (Quest_Log ql in QuestLog)
            {
                buffer.WriteInteger(ql.Quest_ID);
                buffer.WriteInteger((int)ql.Status);
                buffer.WriteInteger(ql.ObjectiveProgress);
                Quest q = World.instance.quests[ql.Quest_ID];
                buffer.WriteInteger(q.Objective_Target);
                buffer.WriteString(q.Title);
                if (ql.Status == QuestStatus.Available || ql.Status == QuestStatus.InProgress)
                {
                    buffer.WriteInteger(World.instance.GetNPCEntityID(q.NPC_Start_ID));
                    buffer.WriteString(q.Start_Text);
                }
                else if (ql.Status == QuestStatus.Finished || ql.Status == QuestStatus.Complete)
                {
                    buffer.WriteInteger(World.instance.GetNPCEntityID(q.NPC_End_ID));
                    buffer.WriteString(q.End_Text);
                }
            }
            sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.WorldPacket.ToString(), index, buffer.ToArray());
            Log.log("World Packet: Sent Quest Logs.", Log.LogType.SUCCESS);
        }
        public static void CharacterDetails(int index, Player Character)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.CharacterDetails, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
                buffer.WriteInteger(Character.Character_ID);
                buffer.WriteString(Character.Name);
                buffer.WriteInteger(Character.Level);
                buffer.WriteInteger((int)Character.gender);
                // Position
                buffer.WriteFloat(Character.position.x);
                buffer.WriteFloat(Character.position.y);
                buffer.WriteFloat(Character.position.z);
                buffer.WriteFloat(Character.r);
                // Camera Position
                buffer.WriteFloat(Character.Camera_Pos_X);
                buffer.WriteFloat(Character.Camera_Pos_Y);
                buffer.WriteFloat(Character.Camera_Pos_Z);
                buffer.WriteFloat(Character.Camera_Rotation_Y);
                // Stats
                buffer.WriteInteger(Character.Max_HP);
                buffer.WriteInteger(Character.Current_HP);
                buffer.WriteInteger(Character.Strength);
                buffer.WriteInteger(Character.Agility);
                buffer.WriteInteger(Character.experience);
                if (Network.instance.Clients[index].Version != "")
                {
                    buffer.WriteInteger(World.instance.GetLevelCriteria(Character.Level));
                    buffer.WriteInteger(World.instance.GetLevelCriteria(Character.Level + 1));
                }
                // Quest Log
                List<Quest_Log> ql = World.instance.GetQuestLog(Character.Character_ID);
                buffer.WriteInteger(ql.Count);
                foreach (Quest_Log l in ql)
                {
                    Quest q = World.instance.quests[l.Quest_ID];
                    buffer.WriteInteger(q.ID);
                    buffer.WriteString(q.Title);
                    buffer.WriteInteger(l.ObjectiveProgress);
                    buffer.WriteInteger(q.NPC_Start_ID);
                    buffer.WriteInteger(q.NPC_End_ID);
                    buffer.WriteInteger(q.Objective_Target);
                }
                // Available quests
                List<Quest> availablequests = World.instance.GetAvailableQuests(Character.Character_ID);
                buffer.WriteInteger(availablequests.Count);
                foreach (Quest quest in availablequests)
                {
                    buffer.WriteInteger(quest.ID);
                    buffer.WriteInteger(quest.NPC_Start_ID);
                }
                Log.log("Sending Character Data packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.CharacterDetails.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Send Character Data packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
                throw;
            }
        }
        public static void PlayerStateChange(int index, Player player, PlayerState state)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.PlayerStateChange, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
                buffer.WriteInteger((int)state);
                switch (state)
                {
                    case PlayerState.Login:
                        buffer.WriteInteger(player.Character_ID);
                        buffer.WriteInteger(player.Entity_ID);
                        buffer.WriteString(player.Name);
                        buffer.WriteInteger((int)player.gender);
                        buffer.WriteInteger(player.Level);
                        buffer.WriteInteger(player.Current_HP);
                        buffer.WriteInteger(player.Max_HP);
                        buffer.WriteFloat(player.position.x);
                        buffer.WriteFloat(player.position.y);
                        buffer.WriteFloat(player.position.z);
                        buffer.WriteFloat(player.r);
                        break;
                    case PlayerState.Logout:
                        buffer.WriteInteger(player.Character_ID);
                        buffer.WriteInteger(player.Entity_ID);
                        break;
                    case PlayerState.Update:
                        buffer.WriteInteger(player.Character_ID);
                        buffer.WriteInteger(player.Entity_ID);
                        buffer.WriteInteger(player.Level);
                        if (Network.instance.Clients[index].Version != "")
                        {
                            buffer.WriteInteger(player.Strength);
                            buffer.WriteInteger(player.Agility);
                            buffer.WriteInteger(player.Max_HP);
                            buffer.WriteInteger(player.Current_HP);
                            buffer.WriteInteger(player.experience);
                            buffer.WriteInteger(World.instance.GetLevelCriteria(player.Level));
                            buffer.WriteInteger(World.instance.GetLevelCriteria(player.Level + 1));
                        }
                        break;
                    default:
                        break;
                }
                Log.log("Sending Player State Change packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.PlayerStateChange.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Player State Change packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void QuestReturn(int index, QuestReturn qr, int quest_Log_ID)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.QuestReturn, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
                if (qr.Null)
                {
                    buffer.WriteByte(1);
                }
                else
                {
                    buffer.WriteByte(0);
                    buffer.WriteInteger((int)qr.Status);
                    buffer.WriteInteger(qr.Quest_ID);
                    buffer.WriteInteger(quest_Log_ID);
                    buffer.WriteInteger(qr.NPC_ID);
                    buffer.WriteString(qr.Title);
                    buffer.WriteString(qr.Text);
                    buffer.WriteInteger(World.instance.GetQuestLog(Network.instance.Clients[index].Character_ID, qr.Quest_ID).ObjectiveProgress);
                    buffer.WriteInteger(qr.Target);
                }
                Log.log("Sending Quest Return packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.QuestReturn.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Quest Return packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void QuestInteractConfirm(int index, bool Confirmed, QuestStatus NewStatus = QuestStatus.None, int NPC_ID = -1, int Quest_ID = -1)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.QuestInteractConfirm, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
                buffer.WriteByte((Confirmed) ? (byte)1 : (byte)0);
                buffer.WriteInteger(Quest_ID);
                buffer.WriteInteger((int)NewStatus);
                buffer.WriteInteger(NPC_ID);
                Log.log("Sending Quest Confirmation packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.QuestInteractConfirm.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Quest Confirmation packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void CollectableInteractConfirm(int index, Collectable col, Quest_Log ql = null)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.CollectableInteractConfirm, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(col.Entity_ID);
                buffer.WriteByte(col.Active ? (byte)1 : (byte)0);
                if (ql != null)
                {
                    buffer.WriteByte(1);
                    buffer.WriteInteger(ql.Quest_ID);
                    buffer.WriteInteger(ql.ObjectiveProgress);
                    buffer.WriteInteger((int)ql.Status);
                }
                else
                {
                    buffer.WriteByte(0);
                }

                Log.log("Sending Collectable Interaction Confirm packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.CollectableInteractConfirm.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Collectable Interaction Confirm packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void CollectableToggle(int Collectable_Entity_ID, bool Active)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.CollectableToggle, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(Collectable_Entity_ID);
                buffer.WriteByte(Active ? (byte)1 : (byte)0);

                Log.log("Sending Collectable Interaction Confirm packet to clients..", Log.LogType.SENT);
                for (int i = 0; i < Network.instance.Clients.Length; i++)
                {
                    if (Network.instance.Clients[i].Connected && 
                        Network.instance.Clients[i].Socket != null && 
                        Network.instance.Clients[i].Socket.Connected &&
                        Network.instance.Clients[i].InGame())
                    {
                        sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.CollectableToggle.ToString(), i, buffer.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                Log.log("Building Collectable Toggle packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void AttackResponse(int index, int Character_ID, NPC npc, int Damage)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.AttackResponse, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(Character_ID);
                buffer.WriteInteger(npc.Entity_ID);
                buffer.WriteInteger(Damage);
                buffer.WriteInteger(npc.Current_HP);
                Quest_Log ql = World.instance.GetQuestLogByNPCID(Character_ID, npc.NPC_ID);
                if (ql != null && npc.Current_HP <= 0)
                {
                    ql.Increment();
                    buffer.WriteByte(1);
                    buffer.WriteInteger(ql.Quest_ID);
                    buffer.WriteInteger(ql.ObjectiveProgress);
                    buffer.WriteInteger((int)ql.Status);
                }
                else
                {
                    buffer.WriteByte(0);
                }

                Log.log("Sending Attack Response packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.AttackResponse.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Attack Response packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void AttackResponseNew(int index, int Character_ID, NPC npc, int Damage, bool Crit)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.AttackResponse, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(Character_ID);
                buffer.WriteInteger(npc.Entity_ID);
                buffer.WriteInteger(Damage);
                buffer.WriteInteger(npc.Current_HP);
                buffer.WriteByte(Crit ? (byte)1 : (byte)0);
                if (Crit) Log.log("A Crit happened! : " + Damage.ToString(), Log.LogType.GENERAL);
                Quest_Log ql = World.instance.GetQuestLogByNPCID(Character_ID, npc.NPC_ID);
                if (ql != null && npc.Current_HP <= 0)
                {
                    ql.Increment();
                    buffer.WriteByte(1);
                    buffer.WriteInteger(ql.Quest_ID);
                    buffer.WriteInteger(ql.ObjectiveProgress);
                    buffer.WriteInteger((int)ql.Status);
                }
                else
                {
                    buffer.WriteByte(0);
                }

                Log.log("Sending Attack Response packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.AttackResponse.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Attack Response packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void UpdateQuestLog(int index, int Quest_ID, int Quest_Log_ID, int NPC_Entity_ID, QuestStatus status, int Progress, int Objective)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.UpdateQuestLog, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(Quest_ID);
                buffer.WriteInteger(Quest_Log_ID);
                buffer.WriteInteger(NPC_Entity_ID);
                buffer.WriteInteger((int)status);
                buffer.WriteInteger(Progress);
                buffer.WriteInteger(Objective);

                Log.log("Sending Update Quest Log packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.UpdateQuestLog.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Update Quest Log packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void Attacked(int index, int Current_HP, int Damage)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.Attacked, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(Current_HP);
                buffer.WriteInteger(Damage);

                Log.log("Sending Attacked packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.Attacked.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Attacked packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void Respawned(int index, int Current_HP, float x, float y, float z, float r)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.Respawned, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(Current_HP);
                buffer.WriteFloat(x);
                buffer.WriteFloat(y);
                buffer.WriteFloat(z);
                buffer.WriteFloat(r);

                Log.log("Sending Respawned packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.Respawned.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Respawned packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void Heal(int index, int Current_HP, int Heal_Amount)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.Heal, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(Current_HP);
                buffer.WriteInteger(Heal_Amount);

                Log.log("Sending Heal packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.Heal.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Heal packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        public static void StompResponse(int index, List<DamageResponse> value)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.StompResponse, ref buffer);
                buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));

                buffer.WriteInteger(value.Count);
                for (int i = 0; i < value.Count; i++)
                {
                    buffer.WriteInteger(value[i].NPC_Entity_ID);
                    buffer.WriteInteger(value[i].New_HP);
                    buffer.WriteInteger(value[i].Damage);
                    buffer.WriteByte(value[i].Crit ? (byte)1 : (byte)0);
                }

                Log.log("Sending Stomp Response packet to client..", Log.LogType.SENT);
                sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.StompResponse.ToString(), index, buffer.ToArray());
            }
            catch (Exception e)
            {
                Log.log("Building Stomp Response packet failed. > " + e.Message, Log.LogType.ERROR);
                return;
            }
        }
        #endregion
    }
}
