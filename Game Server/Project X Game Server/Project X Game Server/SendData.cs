using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        UpdateQuestLog
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
        CreateQuestLog
    }
    public enum PlayerState
    {
        Login,
        Logout,
        Update
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
                buffer.WriteFloat(player.x);
                buffer.WriteFloat(player.y);
                buffer.WriteFloat(player.z);
                buffer.WriteFloat(player.r);
                // Camera Position
                buffer.WriteFloat(player.Camera_Pos_X);
                buffer.WriteFloat(player.Camera_Pos_Y);
                buffer.WriteFloat(player.Camera_Pos_Z);
                buffer.WriteFloat(player.Camera_Rotation_Y);

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
                if (PostUDPMessages) Log.log("UDP Packet sent to IP: " + connection.IP.Substring(0, connection.IP.IndexOf(':')) + " Length: " + data.Length.ToString(), Log.LogType.SENT);
            }
            catch (Exception e)
            {
                Log.log("An error occurred when sending a UDP packet to IP: " + connection.IP.Substring(connection.IP.IndexOf(':')).ToString() + ". > " + e.Message, Log.LogType.ERROR);
            }
        }
        public static void WorldPacket(int index)
        {
            //try
            //{
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            BuildBasePacket((int)ClientSendPacketNumbers.WorldPacket, ref buffer);
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
                buffer.WriteFloat(player.x);
                buffer.WriteFloat(player.y);
                buffer.WriteFloat(player.z);
                buffer.WriteFloat(player.r);
            }
            // NPCs
            buffer.WriteInteger(World.instance.NPCsInWorld.Count);
            foreach (NPC npc in World.instance.NPCsInWorld)
            {
                buffer.WriteInteger(npc.NPC_ID);
                buffer.WriteInteger(npc.Entity_ID);
                buffer.WriteString(npc.Name);
                buffer.WriteInteger((int)npc.gender);
                buffer.WriteInteger((int)npc.Status);
                buffer.WriteInteger(npc.Level);
                buffer.WriteInteger(npc.Current_HP);
                buffer.WriteInteger(npc.Max_HP);
                buffer.WriteFloat(npc.x);
                buffer.WriteFloat(npc.y);
                buffer.WriteFloat(npc.z);
                buffer.WriteFloat(npc.r);
                buffer.WriteInteger((int)World.instance.GetQuestStateByNPC(Network.instance.Clients[index].Character_ID, npc.NPC_ID));
                bool Create = false;
                QuestReturn qr = World.instance.GetQuestContentByNPC(Network.instance.Clients[index].Character_ID, npc.Entity_ID, out Create);
                Quest_Log ql = null;
                if (Create)
                {
                    ql = new Quest_Log(-1, Network.instance.Clients[index].Character_ID, qr.Quest_ID, QuestStatus.Available, 0);
                    World.instance.quest_log.Add(ql);
                    SendData.CreateQuestLog(ql);
                }
                else
                {
                    ql = World.instance.GetQuestLog(Network.instance.Clients[index].Character_ID, qr.Quest_ID);
                }
                buffer.WriteInteger(qr.Quest_ID);
                if (qr.Null || ql == null)
                {
                    buffer.WriteInteger(-1);
                    buffer.WriteInteger(0);
                    buffer.WriteInteger(0);
                }
                else
                {
                    buffer.WriteInteger(ql.Quest_Log_ID);
                    buffer.WriteInteger(ql.ObjectiveProgress);
                    buffer.WriteInteger(World.instance.quests[ql.Quest_ID].Objective_Target);
                }
            }
            // Collectables
            buffer.WriteInteger(World.instance.collectablesInWorld.Count);
            foreach (Collectable collectable in World.instance.collectablesInWorld)
            {
                buffer.WriteInteger(collectable.Collectable_ID);
                buffer.WriteInteger(collectable.Entity_ID);
                buffer.WriteString(collectable.Name);
                buffer.WriteInteger(collectable.Respawn_Time);
                buffer.WriteFloat(collectable.x);
                buffer.WriteFloat(collectable.y);
                buffer.WriteFloat(collectable.z);
                buffer.WriteFloat(collectable.r);
                buffer.WriteByte((collectable.Active) ? (byte)1 : (byte)0);
            }
            Log.log("Sending initial world packet to client..", Log.LogType.SENT);
            sendData(ConnectionType.CLIENT, ClientSendPacketNumbers.WorldPacket.ToString(), index, buffer.ToArray());
            //}
            //catch (Exception e)
            //{
            //    Log.log("Building initial world packet failed. > " + e.Message, Log.LogType.ERROR);
            //    return;
            //}
        }
        public static void CharacterDetails(int index, Player Character)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.CharacterDetails, ref buffer);
                buffer.WriteInteger(Character.Character_ID);
                buffer.WriteString(Character.Name);
                buffer.WriteInteger(Character.Level);
                buffer.WriteInteger((int)Character.gender);
                // Position
                buffer.WriteFloat(Character.x);
                buffer.WriteFloat(Character.y);
                buffer.WriteFloat(Character.z);
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
                buffer.WriteInteger(Character.Experience);
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
                        buffer.WriteFloat(player.x);
                        buffer.WriteFloat(player.y);
                        buffer.WriteFloat(player.z);
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
        public static void UpdateQuestLog(int index, int Quest_ID, int Quest_Log_ID, int NPC_Entity_ID, QuestStatus status, int Progress, int Objective)
        {
            try
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                BuildBasePacket((int)ClientSendPacketNumbers.UpdateQuestLog, ref buffer);

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

        public static void SendUDP_WorldUpdate(int index)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            byte[] data = World.instance.PullBuffer();
            if (data != null)
            {
                buffer.WriteBytes(data);
                SendUDP_Packet(Network.instance.Clients[index], buffer.ToArray());
            }
        }
        #endregion
    }
}
