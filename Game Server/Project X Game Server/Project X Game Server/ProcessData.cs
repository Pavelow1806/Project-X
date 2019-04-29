using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;

namespace Project_X_Game_Server
{
    public enum ClientProcessPacketNumbers
    {
        Invalid,
        EnterWorld,
        Update,
        RequestQuest,
        QuestInteract,
        CollectableInteract,
        Attack,
        PacketReceived,
        Respawn,
        Stomp,
        Blood,
        Heal
    }
    public enum LoginServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WhiteList,
        CreateCharacterResponse
    }
    public enum GameServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer
    }
    public enum SyncServerProcessPacketNumbers
    {
        Invalid,
        AuthenticateServer,
        WorldRequest,
        NewQuestLog
    }
    public enum SyncServerTable
    {
        tbl_Characters,
        tbl_NPC,
        tbl_Quests,
        tbl_Collectables,
        tbl_Spawn_Positions,
        tbl_Quest_Log,
        tbl_Experience
    }
    class ProcessData
    {
        #region Locking
        private static readonly object lockObj = new object();
        private static readonly object lockWorldObj = new object();
        #endregion

        private static int LineNumber = 0;

        public static void processData(int index, byte[] data)
        {
            lock (lockObj)
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                buffer.WriteBytes(data);

                ConnectionType Source = (ConnectionType)buffer.ReadInteger();
                int PacketNumber = buffer.ReadInteger();
                
                object[] obj;
                switch (Source)
                {
                    case ConnectionType.SYNCSERVER:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(SyncServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[(ConnectionType)index].Socket == null)
                        {
                            return;
                        }
                        Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((SyncServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.SYNCSERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);

                        obj = new object[3];
                        obj[0] = Source;
                        obj[1] = index;
                        obj[2] = data;

                        typeof(ProcessData).InvokeMember(((SyncServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    case ConnectionType.LOGINSERVER:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(LoginServerProcessPacketNumbers), PacketNumber) || Network.instance.Servers[ConnectionType.LOGINSERVER].Socket == null)
                        {
                            return;
                        }
                        Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((LoginServerProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.GAMESERVER.ToString() + ", Processing response..", Log.LogType.RECEIVED);

                        obj = new object[3];
                        obj[0] = Source;
                        obj[1] = index;
                        obj[2] = data;

                        typeof(ProcessData).InvokeMember(((LoginServerProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
                        break;
                    case ConnectionType.CLIENT:
                        if (PacketNumber == 0 || !Enum.IsDefined(typeof(ClientProcessPacketNumbers), PacketNumber) || Network.instance.Clients[index].Socket == null)
                        {
                            return;
                        }
                        //Log.log("Packet Received [#" + PacketNumber.ToString("000") + " " + ((ClientProcessPacketNumbers)PacketNumber).ToString() + "] from " + ConnectionType.CLIENT.ToString() + ", Processing response..", Log.LogType.RECEIVED);

                        obj = new object[3];
                        obj[0] = Source;
                        obj[1] = index;
                        obj[2] = data;

                        typeof(ProcessData).InvokeMember(((ClientProcessPacketNumbers)PacketNumber).ToString(), BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, obj);
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
        private static void AuthenticateServer(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            if (buffer.ReadString() == Network.instance.AuthenticationCode)
            {
                // Confirmed to be the correct server, proceed with unblocking the client communication channels
                Network.instance.Servers[(ConnectionType)index].Authenticated = true;
                Network.instance.SyncServerAuthenticated = true;
                Network.instance.Servers.Add(type, Network.instance.Servers[(ConnectionType)index]);
                Network.instance.Servers.Remove((ConnectionType)index);
                Network.instance.Servers[type].Index = (int)type;
            }
        }
        #endregion
        
        #region Login Server Communication
        private static void WhiteList(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            string ip = buffer.ReadString();
            int LineNumber = Log.log("Checking white list for IP: " + ip.Substring(0, ip.IndexOf(':')) + "..", Log.LogType.RECEIVED);
            if (!Network.instance.CheckWhiteList(ip.Substring(0, ip.IndexOf(':'))))
            {
                Network.instance.WhiteList.Add(ip.Substring(0, ip.IndexOf(':')));
                Log.log(LineNumber, "Client IP: " + ip.Substring(0, ip.IndexOf(':')) + " added successfully, sending confirmation to login server.", Log.LogType.RECEIVED);
                SendData.ConfirmWhiteList(ip.Substring(0, ip.IndexOf(':')));
            }
            else
            {
                Log.log(LineNumber, "Client IP: " + ip.Substring(0, ip.IndexOf(':')) + " was already white listed, sending reconfirmation to login server.", Log.LogType.WARNING);
                SendData.ConfirmWhiteList(ip.Substring(0, ip.IndexOf(':')));
            }
        }
        private static void CreateCharacterResponse(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int Character_ID = buffer.ReadInteger();
            string Name = buffer.ReadString();
            Gender gender = (Gender)buffer.ReadInteger();
            World.instance.players.Add(Character_ID, new Player(Character_ID, Name, 1, gender,
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 100, 10, 10, 0));
        }
        #endregion

        #region Synchronization Server Communication
        private static void WorldRequest(ConnectionType type, int index, byte[] data)
        {
            lock (lockWorldObj)
            {
                ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
                buffer.WriteBytes(data);
                ReadHeader(ref buffer);

                SyncServerTable table = (SyncServerTable)buffer.ReadInteger();
                switch (table)
                {
                    case SyncServerTable.tbl_Characters:
                        // tbl_Characters
                        int LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Characters..", Log.LogType.RECEIVED);
                        int Character_Count = buffer.ReadInteger();
                        for (int i = 0; i < Character_Count; i++)
                        {
                            int Character_ID = buffer.ReadInteger();
                            if (!World.instance.players.ContainsKey(Character_ID))
                            {
                                World.instance.players.Add(Character_ID, new Player(Character_ID, buffer.ReadString(), buffer.ReadInteger(), (Gender)buffer.ReadInteger(),
                                    buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(),
                                    0.0f, 0.0f, 0.0f, buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger()));
                                World.instance.players[Character_ID].type = EntityType.Player;
                                World.instance.players[Character_ID].Camera_Pos_X = buffer.ReadFloat();
                                World.instance.players[Character_ID].Camera_Pos_Y = buffer.ReadFloat();
                                World.instance.players[Character_ID].Camera_Pos_Z = buffer.ReadFloat();
                                World.instance.players[Character_ID].Camera_Rotation_Y = buffer.ReadFloat();
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added character " + i.ToString() + "/" + Character_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedPlayers = true;
                        Log.log(LineNumber, "Successfully added characters (" + Character_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_NPC:
                        // tbl_NPC
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_NPC..", Log.LogType.RECEIVED);
                        int NPC_Count = buffer.ReadInteger();
                        for (int i = 0; i < NPC_Count; i++)
                        {
                            int NPC_ID = buffer.ReadInteger();
                            if (!World.instance.NPCs.ContainsKey(NPC_ID))
                            {
                                World.instance.NPCs.Add(NPC_ID, new NPC(NPC_ID, (NPCStatus)buffer.ReadInteger(), buffer.ReadString(), buffer.ReadInteger(), buffer.ReadInteger(),
                                    (Gender)buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger()));
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added NPC " + i.ToString() + "/" + NPC_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedNPCs = true;
                        Log.log(LineNumber, "Successfully added NPCs (" + NPC_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_Quests:
                        // tbl_Quests
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Quests..", Log.LogType.RECEIVED);
                        int Quest_Count = buffer.ReadInteger();
                        for (int i = 0; i < Quest_Count; i++)
                        {
                            int Quest_ID = buffer.ReadInteger();
                            if (!World.instance.quests.ContainsKey(Quest_ID))
                            {
                                World.instance.quests.Add(Quest_ID, new Quest(Quest_ID, buffer.ReadString(), buffer.ReadString(), buffer.ReadString(),
                                    buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger()));
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added quest " + i.ToString() + "/" + Quest_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedQuests = true;
                        Log.log(LineNumber, "Successfully added quests (" + Quest_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_Collectables:
                        // tbl_Collectables
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Collectables..", Log.LogType.RECEIVED);
                        int Collectable_Count = buffer.ReadInteger();
                        for (int i = 0; i < Collectable_Count; i++)
                        {
                            int Collectable_ID = buffer.ReadInteger();
                            if (!World.instance.collectables.ContainsKey(Collectable_ID))
                            {
                                World.instance.collectables.Add(Collectable_ID, new Collectable(Collectable_ID, buffer.ReadString(), buffer.ReadInteger(), 0.0f, 0.0f, 0.0f, 0.0f));
                            }                                
                            Log.log(LineNumber, "Processing world request packet.. Added collectable " + i.ToString() + "/" + Collectable_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedCollectables = true;
                        Log.log(LineNumber, "Successfully added collectables (" + Collectable_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_Spawn_Positions:
                        // tbl_Spawn_Positions
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Spawn_Positions..", Log.LogType.RECEIVED);
                        int Spawn_Count = buffer.ReadInteger();
                        for (int i = 0; i < Spawn_Count; i++)
                        {
                            int Spawn_ID = buffer.ReadInteger();
                            if (!World.instance.spawns.ContainsKey(Spawn_ID))
                            {
                                World.instance.spawns.Add(Spawn_ID, new Spawn(Spawn_ID, buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat(),
                                    buffer.ReadFloat(), buffer.ReadInteger(), buffer.ReadInteger()));
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added spawn " + i.ToString() + "/" + Spawn_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedSpawns = true;
                        Log.log(LineNumber, "Successfully added spawn (" + Spawn_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_Quest_Log:
                        // tbl_Quest_Log
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Quest_Log..", Log.LogType.RECEIVED);
                        int Quest_Log_Count = buffer.ReadInteger();
                        for (int i = 0; i < Quest_Log_Count; i++)
                        {
                            int Quest_Log_ID = buffer.ReadInteger();
                            if (!World.instance.ContainsLog(Quest_Log_ID))
                            {
                                World.instance.quest_log.Add(new Quest_Log(Quest_Log_ID, buffer.ReadInteger(), buffer.ReadInteger(), (QuestStatus)buffer.ReadInteger(), buffer.ReadInteger()));
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added log " + i.ToString() + "/" + Quest_Log_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedQuestLogs = true;
                        Log.log(LineNumber, "Sucessfully added logs (" + Quest_Log_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    case SyncServerTable.tbl_Experience:
                        // tbl_Experience
                        LineNumber = Log.log("Processing world request packet.. Adding data from tbl_Experience..", Log.LogType.RECEIVED);
                        int Experience_Count = buffer.ReadInteger();
                        for (int i = 0; i < Experience_Count; i++)
                        {
                            int XP_ID = buffer.ReadInteger();
                            if (!World.instance.experience_levels.ContainsKey(XP_ID))
                            {
                                World.instance.experience_levels.Add(XP_ID, new Experience(XP_ID, buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger(), buffer.ReadInteger()));
                            }
                            Log.log(LineNumber, "Processing world request packet.. Added experience " + i.ToString() + "/" + Experience_Count.ToString(), Log.LogType.RECEIVED);
                        }
                        World.instance.ReceivedExperience = true;
                        Log.log(LineNumber, "Sucessfully added experiences (" + Experience_Count.ToString() + ")", Log.LogType.SUCCESS);
                        break;
                    default:
                        break;
                }
                World.instance.Initialise();
            }
        }
        private static void NewQuestLog(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int Character_ID = buffer.ReadInteger();
            int Quest_ID = buffer.ReadInteger();
            int Quest_Log_ID = buffer.ReadInteger();
            Quest_Log ql = World.instance.GetQuestLog(Character_ID, Quest_ID);
            if (ql != null) ql.Quest_Log_ID = Quest_Log_ID;
        }
        #endregion

        #region Client Communication
        private static void EnterWorld(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            Network.instance.Clients[index].Username = buffer.ReadString();
            string Character_Name = buffer.ReadString();
            Log.log("Account " + Network.instance.Clients[index].Username + " is entering the world with " + Character_Name + "..", Log.LogType.SUCCESS);
            Player player = World.instance.GetPlayer(Character_Name);
            player.InWorld = true;
            Network.instance.Clients[index].Character_ID = player.Character_ID;
            SendData.CharacterDetails(index, player);
            Thread.Sleep(100);
            SendData.LogActivity(Network.instance.Clients[index].Character_ID, Activity.LOGIN, Network.instance.Clients[index].SessionID);
        }
        private static void Update(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            if (data.Length == 105)
            {
                buffer.WriteBytes(data);
                ReadHeader(ref buffer);
                // Connectivity Statistics
                Client client = Network.instance.Clients[index];
                client.TCP_Throughput = buffer.ReadFloat();
                client.TCP_SetLatency(buffer.ReadFloat());
                client.TCP_PacketsReceived = buffer.ReadInteger();
                client.UDP_Throughput = buffer.ReadFloat();
                client.UDP_SetLatency(buffer.ReadFloat());
                client.UDP_PacketsReceived = buffer.ReadInteger();

                // ID
                int Character_ID = buffer.ReadInteger();
                // Position
                float X = buffer.ReadFloat();
                float Y = buffer.ReadFloat();
                float Z = buffer.ReadFloat();
                float R = buffer.ReadFloat();
                // Velocity
                float vx = buffer.ReadFloat();
                float vy = buffer.ReadFloat();
                float vz = buffer.ReadFloat();
                // Camera Position
                float cX = buffer.ReadFloat();
                float cY = buffer.ReadFloat();
                float cZ = buffer.ReadFloat();
                float cR = buffer.ReadFloat();
                // Animation State
                float Forward = buffer.ReadFloat();
                float Turn = buffer.ReadFloat();
                float Jump = buffer.ReadFloat();
                float JumpLeg = buffer.ReadFloat();
                byte bools = buffer.ReadByte();
                bool Crouch = false;
                bool OnGround = false;
                bool Attacking = false;
                bool Dead = false;
                bool Attacked = false;
                bool Cast = false;
                bool b6 = false; // Unused
                bool b7 = false; // Unused
                BitwiseRefinement.ByteToBools(bools, out Crouch, out OnGround, out Attacking, out Dead, out Attacked, out Cast, out b6, out b7);
                // Target
                EntityType TargetType = (EntityType)buffer.ReadInteger();
                int TargetID = buffer.ReadInteger();

                Player player = World.instance.players[Character_ID];
                // Position
                player.x = X;
                player.y = Y;
                player.z = Z;
                player.r = R;
                // Velocity
                player.vx = vx;
                player.vy = vy;
                player.vz = vz;
                // Camera Position and Rotation
                player.Camera_Pos_X = cX;
                player.Camera_Pos_Y = cY;
                player.Camera_Pos_Z = cZ;
                player.Camera_Rotation_Y = cR;
                // Animations
                player.AnimState.Forward = Forward;
                player.AnimState.Turn = Turn;
                player.AnimState.Jump = Jump;
                player.AnimState.JumpLeg = JumpLeg;
                player.AnimState.Crouch = Crouch;
                player.AnimState.OnGround = OnGround;
                player.AnimState.Attacking = Attacking;
                player.AnimState.Dead = Dead;
                player.AnimState.Attacked = Attacked;
                player.AnimState.Cast = Cast;
                // Target
                player.TargetType = TargetType;
                player.TargetID = TargetID;

                SendData.UpdatePlayerData(player);
            }
        }
        private static void RequestQuest(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int Character_ID = buffer.ReadInteger();
            int NPC_ID = buffer.ReadInteger();
            if (MathF.Distance(World.instance.players[Character_ID], World.instance.GetNPCByEntityID(NPC_ID)) <= World.InteractionDistance)
            {
                World.instance.UpdateQuestLog(Character_ID);
                bool Create = false;
                QuestReturn qr = World.instance.GetQuestContentByNPCEntityID(Character_ID, NPC_ID, out Create);
                Quest_Log ql = null;
                if (Create)
                {
                    ql = new Quest_Log(-1, Character_ID, qr.Quest_ID, QuestStatus.Available, 0);
                    World.instance.quest_log.Add(ql);
                    SendData.CreateQuestLog(ql);
                }
                else
                {
                    ql = World.instance.GetQuestLog(Character_ID, qr.Quest_ID);
                }
                if (ql != null)
                    SendData.QuestReturn(index, qr, ql.Quest_Log_ID);
            }
        }
        private static void QuestInteract(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int Character_ID = buffer.ReadInteger();
            int NPC_ID = buffer.ReadInteger();
            if (MathF.Distance(World.instance.players[Character_ID], World.instance.GetNPCByEntityID(NPC_ID)) <= World.InteractionDistance)
            {
                World.instance.UpdateQuestLog(Character_ID);
                bool Create = false;
                QuestReturn qr = World.instance.GetQuestContentByNPCEntityID(Character_ID, NPC_ID, out Create);
                Quest_Log ql = null;
                ql = World.instance.GetQuestLog(Character_ID, qr.Quest_ID);
                if (qr.Null != true && ql != null)
                {
                    switch (qr.Status)
                    {
                        case QuestStatus.None:
                            break;
                        case QuestStatus.InProgress:
                            if (ql.ObjectiveProgress >= qr.Target)
                            {
                                World.instance.GetQuestLog(Character_ID, qr.Quest_ID).Status = QuestStatus.Finished;
                                qr = World.instance.GetQuestContentByNPCEntityID(Character_ID, NPC_ID, out Create);
                                SendData.QuestInteractConfirm(index, true, ql.Status, qr.NPC_ID, ql.Quest_ID);
                                SendData.UpdateQuestLog(ql);
                            }
                            else
                            {
                                SendData.QuestInteractConfirm(index, false);
                            }
                            break;
                        case QuestStatus.Complete:
                            SendData.QuestInteractConfirm(index, false);
                            break;
                        case QuestStatus.Finished:
                            World.instance.GetQuestLog(Character_ID, qr.Quest_ID).Status = QuestStatus.Complete;
                            qr = World.instance.GetQuestContentByNPCEntityID(Character_ID, NPC_ID, out Create);
                            SendData.QuestInteractConfirm(index, true, ql.Status, qr.NPC_ID, ql.Quest_ID);
                            SendData.UpdateQuestLog(ql);
                            // Get the next quest in the series
                            List<Quest> q = World.instance.GetAvailableQuests(Character_ID);
                            if (q != null && q.Count > 0)
                            {
                                foreach (Quest qu in q)
                                {
                                    // Create a new quest log for it
                                    Quest_Log newql = new Quest_Log(-1, Character_ID, qu.ID, QuestStatus.Available, 0);
                                    // Update the synchronization server
                                    SendData.CreateQuestLog(newql);
                                    // Notify the client that a new quest is available
                                    SendData.UpdateQuestLog(index, qu.ID, newql.Quest_Log_ID, World.instance.GetNPCEntityID(qu.NPC_Start_ID), newql.Status, newql.ObjectiveProgress, qu.Objective_Target);
                                }
                            }
                            break;
                        case QuestStatus.Available:
                            if (qr.Target == -1)
                            {
                                World.instance.GetQuestLog(Character_ID, qr.Quest_ID).Status = QuestStatus.Finished;
                                qr = World.instance.GetQuestContentByNPCEntityID(Character_ID, NPC_ID, out Create);
                                World.instance.UpdateQuestLog(Character_ID);
                            }
                            else
                            {
                                World.instance.GetQuestLog(Character_ID, qr.Quest_ID).Status = QuestStatus.InProgress;
                                qr = World.instance.GetQuestContentByNPCEntityID(Character_ID, NPC_ID, out Create);
                                World.instance.UpdateQuestLog(Character_ID);
                            }
                            SendData.QuestInteractConfirm(index, true, ql.Status, qr.NPC_ID, ql.Quest_ID);
                            SendData.UpdateQuestLog(ql);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    SendData.QuestInteractConfirm(index, false);
                }
            }
            else
            {
                SendData.QuestInteractConfirm(index, false);
            }
            World.instance.UpdateQuestLog(Character_ID);
        }
        private static void CollectableInteract(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int Character_ID = buffer.ReadInteger();
            int Col_Entity_ID = buffer.ReadInteger();
            Collectable col = World.instance.GetCollectableByEntityID(Col_Entity_ID);
            if (col != null && col.Active && MathF.Distance(World.instance.players[Character_ID], col) <= World.InteractionDistance)
            {
                col.Active = false;
                Quest_Log quest_Log = World.instance.GetQuest_Log(Character_ID, col.Collectable_ID);
                if (quest_Log != null) quest_Log.Increment();
                SendData.CollectableInteractConfirm(index, col, quest_Log);
            }
        }
        private static void Attack(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            int Character_ID = buffer.ReadInteger();
            int NPC_Entity_ID = buffer.ReadInteger();
            Player player = World.instance.players[Character_ID];
            NPC npc = World.instance.GetNPCByEntityID(NPC_Entity_ID);
            if (player != null && npc != null)
            {
                if (MathF.Distance(player, npc) <= World.InteractionDistance && 
                    (npc.Status == NPCStatus.AGGRESSIVE || npc.Status == NPCStatus.NEUTRAL || npc.Status == NPCStatus.BOSS) &&
                    npc.Current_HP > 0)
                {
                    player.InCombat = true;
                    npc.InCombat = true;
                    npc.TargetType = EntityType.Player;
                    npc.TargetID = Character_ID;
                    npc.AnimState.Attacking = true;
                    int Damage = MathF.Damage(player.Strength, player.Agility, player.BloodMultiplier);
                    npc.Current_HP -= Damage;
                    player.AnimState.Attacking = true;
                    SendData.AttackResponse(index, player.Character_ID, npc, Damage);
                }
            }
        }
        private static void PacketReceived(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            ClientSendPacketNumbers ReceivedPacket = (ClientSendPacketNumbers)buffer.ReadInteger();
            switch (ReceivedPacket)
            {
                case ClientSendPacketNumbers.Invalid:
                    break;
                case ClientSendPacketNumbers.WorldPacket:
                    break;
                case ClientSendPacketNumbers.CharacterDetails:
                    Player player = World.instance.players[Network.instance.Clients[index].Character_ID];
                    for (int i = 0; i < Network.instance.Clients.Length; i++)
                    {
                        if (Network.instance.Clients[i] != null && Network.instance.Clients[i].Connected &&
                            Network.instance.Clients[i].InGame())
                        {
                            SendData.PlayerStateChange(i, player, PlayerState.Login);
                        }
                    }
                    SendData.WorldPacket(index);
                    break;
                case ClientSendPacketNumbers.PlayerStateChange:
                    break;
                case ClientSendPacketNumbers.QuestReturn:
                    break;
                case ClientSendPacketNumbers.QuestInteractConfirm:
                    break;
                case ClientSendPacketNumbers.CollectableInteractConfirm:
                    break;
                case ClientSendPacketNumbers.CollectableToggle:
                    break;
                case ClientSendPacketNumbers.AttackResponse:
                    break;
                default:
                    break;
            }
        }
        private static void Respawn(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);

            if (Network.instance.Clients[index].InGame() &&
                World.instance.players[Network.instance.Clients[index].Character_ID].Current_HP <= 0)
            {
                World.instance.players[Network.instance.Clients[index].Character_ID].Respawn();
                SendData.Respawned(index, World.instance.players[Network.instance.Clients[index].Character_ID].Current_HP,
                    World.instance.players[Network.instance.Clients[index].Character_ID].x,
                    World.instance.players[Network.instance.Clients[index].Character_ID].y,
                    World.instance.players[Network.instance.Clients[index].Character_ID].z,
                    World.instance.players[Network.instance.Clients[index].Character_ID].r);
            }
        }
        private static void Stomp(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);
            if (Network.instance.Clients[index].Character_ID > -1)
                World.instance.NPCsInWorld[World.instance.players[Network.instance.Clients[index].Character_ID].TargetID].Current_HP -= World.StompDamage;
        }
        private static void Blood(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);

            World.instance.players[Network.instance.Clients[index].Character_ID].BloodMultiplier = 2;
            World.instance.players[Network.instance.Clients[index].Character_ID].BloodExpire = DateTime.Now.AddSeconds(5);
        }
        private static void Heal(ConnectionType type, int index, byte[] data)
        {
            ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteBytes(data);
            ReadHeader(ref buffer);

            World.instance.players[Network.instance.Clients[index].Character_ID].Current_HP += World.HealAmount;

            SendData.Heal(index, World.instance.players[Network.instance.Clients[index].Character_ID].Current_HP, World.HealAmount);
        }
        #endregion
    }
    public class MathF
    {
        public static float Distance(Entity e1, Entity e2)
        {
            if (e1 == null || e2 == null)
            {
                return 999.0f;
            }
            //return (float)Math.Sqrt((e2.x - e1.x) * (e2.x - e1.x) + (e2.x - e1.y) * (e2.y - e1.y) + (e2.z - e1.z) * (e2.z - e1.z));
            return (float)Math.Sqrt((e2.x - e1.x) * (e2.x - e1.x) + (e2.z - e1.z) * (e2.z - e1.z));
        }
        public static int Damage(int Strength, int Agility, int BloodMultiplier)
        {
            Random random = new Random();

            int RandDamage = random.Next((int)(Strength * 1.0f), (int)(Strength * 2.0f));
            bool Crit = (random.Next(0, 100) >= 50 - Agility ? true : false);
            return (Crit ? (RandDamage * BloodMultiplier) : (RandDamage * 2 * BloodMultiplier));
        }
    }
}
