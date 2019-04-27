using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class World
    {
        public static World instance;

        public bool Running = false;
        private Thread UpdateThread;
        public static float GlobalAttackSpeed = 2.0f;
        public static float InteractionDistance = 15.0f;
        public static int TickRate = 5;
        public static int HealthRegenPer5 = 5;
        private static DateTime NextTick = default(DateTime);
        private int entityCounter = 0;
        public int EntityCounter
        {
            get
            {
                return ++entityCounter;
            }
        }
        private ByteBuffer.ByteBuffer WorldBuffer = new ByteBuffer.ByteBuffer();

        public Dictionary<int, Player> players = new Dictionary<int, Player>();
        public bool ReceivedPlayers = false;
        public List<Player> playersInWorld = new List<Player>();

        public Dictionary<int, NPC> NPCs = new Dictionary<int, NPC>();
        public bool ReceivedNPCs = false;
        public List<NPC> NPCsInWorld = new List<NPC>();
        
        public Dictionary<int, Collectable> collectables = new Dictionary<int, Collectable>();
        public bool ReceivedCollectables = false;
        public List<Collectable> collectablesInWorld = new List<Collectable>();

        public Dictionary<int, Spawn> spawns = new Dictionary<int, Spawn>();
        public bool ReceivedSpawns = false;

        public Dictionary<int, Quest> quests = new Dictionary<int, Quest>();
        public bool ReceivedQuests = false;

        public List<Quest_Log> quest_log = new List<Quest_Log>();
        public bool ReceivedQuestLogs = false;

        public Dictionary<int, Experience> experience_levels = new Dictionary<int, Experience>();
        public bool ReceivedExperience = false;

        public World()
        {
            instance = this;
        }

        public void Initialise()
        {
            if (ReceivedPlayers && ReceivedNPCs && ReceivedCollectables && 
                ReceivedSpawns && ReceivedQuests && ReceivedQuestLogs &&
                ReceivedExperience)
            {
                UpdateThread = new Thread(new ThreadStart(Start));
                UpdateThread.Start();
            }
        }

        public void Start()
        {
            Log.log("World Loop Thread starting..", Log.LogType.SYSTEM);
            // Spawn all of the NPC's and Collectables
            int i_npc = 0;
            NPCsInWorld.Clear();
            int i_col = 0;
            collectablesInWorld.Clear();
            // Spawn all NPC's
            foreach (KeyValuePair<int, Spawn> spawn in spawns)
            {
                if (!spawn.Value.InUse)
                {
                    if (spawn.Value.NPC_ID > -1)
                    {
                        NPCsInWorld.Add
                        (
                            new NPC
                            (
                                NPCs[spawn.Value.NPC_ID].NPC_ID,
                                NPCs[spawn.Value.NPC_ID].Status,
                                NPCs[spawn.Value.NPC_ID].Name,
                                NPCs[spawn.Value.NPC_ID].Respawn_Time,
                                NPCs[spawn.Value.NPC_ID].Level,
                                NPCs[spawn.Value.NPC_ID].gender,
                                NPCs[spawn.Value.NPC_ID].Max_HP,
                                NPCs[spawn.Value.NPC_ID].Strength,
                                NPCs[spawn.Value.NPC_ID].Agility
                            )
                        );
                        NPCsInWorld[i_npc].x = spawn.Value.Pos_X;
                        NPCsInWorld[i_npc].y = spawn.Value.Pos_Y;
                        NPCsInWorld[i_npc].z = spawn.Value.Pos_Z;
                        NPCsInWorld[i_npc].r = spawn.Value.Rotation_Y;
                        NPCsInWorld[i_npc].Spawn_ID = spawn.Key;
                        ++i_npc;
                    }
                    else if (spawn.Value.Collectable_ID > -1)
                    {
                        collectablesInWorld.Add
                        (
                            new Collectable
                            (
                                collectables[spawn.Value.Collectable_ID].Collectable_ID,
                                collectables[spawn.Value.Collectable_ID].Name,
                                collectables[spawn.Value.Collectable_ID].Respawn_Time,
                                spawn.Value.Pos_X,
                                spawn.Value.Pos_Y,
                                spawn.Value.Pos_Z,
                                spawn.Value.Rotation_Y
                            )
                        );
                        collectablesInWorld[i_col].Spawn_ID = spawn.Key;
                        ++i_col;
                    }
                }
            }
            Running = true;
            // Main loop
            NextTick = DateTime.Now.AddSeconds(TickRate);
            while (Running)
            {
                if (DateTime.Now > NextTick)
                {
                    foreach (NPC npc in NPCsInWorld)
                    {
                        if (npc.Active)
                        {
                            if (npc.Current_HP + HealthRegenPer5 > npc.Max_HP)
                            {
                                npc.Current_HP = npc.Max_HP;
                            }
                            else
                            {
                                npc.Current_HP += HealthRegenPer5;
                            }
                        }
                    }
                }
                foreach (Player player in playersInWorld)
                {
                    if (player.InCombat && MathF.Distance(player, GetNPCByEntityID(player.TargetID)) > InteractionDistance)
                    {
                        player.InCombat = false;
                        player.AnimState.Attacking = false;
                    }
                }
                foreach (NPC npc in NPCsInWorld)
                {
                    if (npc.InCombat)
                    {
                        if (DateTime.Now >= npc.NextAttack && npc.TargetID > 0 && 
                            MathF.Distance(npc, players[npc.TargetID]) <= InteractionDistance)
                        {
                            players[npc.TargetID].Current_HP -= MathF.Damage(npc.Strength, npc.Agility);
                            npc.NextAttack = DateTime.Now.AddSeconds(GlobalAttackSpeed);
                        }
                    }
                    if (npc.Current_HP <= 0)
                    {
                        npc.Active = false;
                    }
                }

            }
            UpdateThread.Join();
        }

        public Player GetPlayer(string Character_Name)
        {
            foreach (KeyValuePair<int, Player> player in players)
            {
                if (player.Value.Name == Character_Name)
                {
                    return player.Value;
                }
            }
            return null;
        }
        public Player FindPlayerInWorld(int Character_ID)
        {
            foreach (Player player in playersInWorld)
            {
                if (player.Character_ID == Character_ID)
                {
                    return player;
                }
            }
            return null;
        }
        public List<Quest_Log> GetQuestLog(int Character_ID)
        {
            List<Quest_Log> result = new List<Quest_Log>();
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Character_ID == Character_ID)
                {
                    result.Add(ql);
                }
            }
            return result;
        }
        public List<Quest> GetAvailableQuests(int Character_ID)
        {
            List<Quest> result = new List<Quest>();
            int MaxCompletedQuestID = 0;
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Character_ID == Character_ID && 
                    ql.Status == QuestStatus.Complete && 
                    ql.Quest_ID > MaxCompletedQuestID)
                {
                    MaxCompletedQuestID = ql.Quest_ID;
                }
            }
            if (MaxCompletedQuestID == 0)
            {
                result.Add(quests[1]);
            }
            else
            {
                foreach (KeyValuePair<int, Quest> q in quests)
                {
                    if (q.Value.Start_Requirement_Quest_ID == MaxCompletedQuestID)
                    {
                        result.Add(q.Value);
                    }
                }
            }
            return result;
        }
        public NPC GetNPCByEntityID(int Entity_ID)
        {
            foreach (NPC npc in NPCsInWorld)
            {
                if (npc.Entity_ID == Entity_ID)
                {
                    return npc;
                }
            }
            return null;
        }
        public int GetNPCEntityID(int NPC_ID)
        {
            foreach (NPC npc in NPCsInWorld)
            {
                if (npc.NPC_ID == NPC_ID)
                {
                    return npc.Entity_ID;
                }
            }
            return -1;
        }
        public Collectable GetCollectableByEntityID(int Col_Entity_ID)
        {
            foreach (Collectable col in collectablesInWorld)
            {
                if (col.Entity_ID == Col_Entity_ID)
                {
                    return col;
                }
            }
            return null;
        }
        public QuestStatus GetQuestStateByNPC(int Character_ID, int NPC_ID)
        {
            List<Quest> NPC_Quests = new List<Quest>();
            foreach (KeyValuePair<int, Quest> quest in quests)
            {
                if (quest.Value.NPC_Start_ID == NPC_ID ||
                    quest.Value.NPC_End_ID == NPC_ID)
                {
                    NPC_Quests.Add(quest.Value);
                }
            }
            List<Quest_Log> Character_Quest_Log = new List<Quest_Log>();
            foreach (Quest_Log questlog in quest_log)
            {
                if (questlog.Character_ID == Character_ID)
                {
                    Character_Quest_Log.Add(questlog);
                }
            }
            QuestStatus result = QuestStatus.None;
            foreach (Quest q in NPC_Quests)
            {
                foreach (Quest_Log ql in Character_Quest_Log)
                {
                    if (q.ID == ql.Quest_ID)
                    {
                        if (ql.Status == QuestStatus.InProgress && 
                            (result != QuestStatus.Finished && result != QuestStatus.Complete))
                        {
                            result = ql.Status;
                        }
                        else if (ql.Status == QuestStatus.Finished && 
                            (result != QuestStatus.Complete && result != QuestStatus.Finished))
                        {
                            result = ql.Status;
                        }
                        else if (ql.Status == QuestStatus.Complete &&
                            result != QuestStatus.Complete)
                        {
                            result = ql.Status;
                        }
                    }
                }
            }
            if (result == QuestStatus.None)
            {
                List<Quest> qs = GetAvailableQuests(Character_ID);
                foreach (Quest que in qs)
                {
                    if (que.NPC_Start_ID == NPC_ID)
                    {
                        result = QuestStatus.Available;
                    }
                }
            }
            return result;
        }
        public List<Quest> GetNPCQuests(int NPC_ID)
        {
            List<Quest> result = new List<Quest>();
            foreach (KeyValuePair<int, Quest> q in quests)
            {
                if (q.Value.NPC_End_ID == NPC_ID || q.Value.NPC_Start_ID == NPC_ID)
                {
                    result.Add(q.Value);
                }
            }
            return result;
        }
        public QuestReturn GetQuestContentByNPC(int Character_ID, int NPC_Entity_ID, out bool Create)
        {
            QuestReturn result = new QuestReturn(true, QuestStatus.None, -1, -1, "", "", -1);
            NPC Subject = GetNPCByEntityID(NPC_Entity_ID);
            List<Quest> NPCQuests = GetNPCQuests(Subject.NPC_ID);
            // Check whether any of the quests have data in the log
            foreach (Quest_Log ql in quest_log)
            {
                if (quests[ql.Quest_ID].NPC_Start_ID == Subject.NPC_ID || quests[ql.Quest_ID].NPC_End_ID == Subject.NPC_ID)
                {
                    result.Null = false;
                    result.Status = ql.Status;
                    result.Quest_ID = ql.Quest_ID;
                    result.NPC_ID = Subject.Entity_ID;
                    result.Title = quests[ql.Quest_ID].Title;
                    if (quests[ql.Quest_ID].NPC_Start_ID == Subject.NPC_ID && (ql.Status != QuestStatus.Finished || ql.Status != QuestStatus.Complete))
                    {
                        result.Text = quests[ql.Quest_ID].Start_Text;
                    }
                    else
                    {
                        result.Text = quests[ql.Quest_ID].End_Text;
                    }
                    result.Target = quests[ql.Quest_ID].Objective_Target;
                    Create = false;
                    return result;
                }
            }
            List<Quest> AvailableQuests = GetAvailableQuests(Character_ID);
            foreach (Quest q in AvailableQuests)
            {
                if (q.NPC_Start_ID == Subject.NPC_ID)
                {
                    result.Null = false;
                    result.Status = QuestStatus.Available;
                    result.Quest_ID = q.ID;
                    result.NPC_ID = Subject.Entity_ID;
                    result.Title = quests[q.ID].Title;
                    if (quests[q.ID].NPC_Start_ID == Subject.NPC_ID)
                    {
                        result.Text = quests[q.ID].Start_Text;
                    }
                    else
                    {
                        result.Text = quests[q.ID].End_Text;
                    }
                    result.Target = quests[q.ID].Objective_Target;
                    Create = true;
                    return result;
                }
            }
            Create = false;
            return result;
        }
        public bool ContainsLog(int LogID)
        {
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Quest_Log_ID == LogID)
                {
                    return true;
                }
            }
            return false;
        }
        public Quest_Log GetQuestLog(int Character_ID, int Quest_ID)
        {
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Quest_ID == Quest_ID && ql.Character_ID == Character_ID)
                {
                    return ql;
                }
            }
            return null;
        }
        public Quest_Log GetQuest_Log(int Character_ID, int Collectable_ID)
        {
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Character_ID == Character_ID && 
                    quests[ql.Quest_ID].Item_Objective_ID == Collectable_ID && 
                    ql.Status == QuestStatus.InProgress)
                {
                    return ql;
                }
            }
            return null;
        }
        public Quest_Log GetQuestLogByNPCID(int Character_ID, int NPC_ID)
        {
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Character_ID == Character_ID && 
                    quests[ql.Quest_ID].NPC_Objective_ID == NPC_ID &&
                    ql.Status == QuestStatus.InProgress)
                {
                    return ql;
                }
            }
            return null;
        }
        public Quest GetNextQuest(int QuestID)
        {
            foreach (KeyValuePair<int, Quest> q in quests)
            {
                if (q.Value.Start_Requirement_Quest_ID == QuestID)
                {
                    return q.Value;
                }
            }
            return null;
        }
        public List<Quest> GetNextQuests(int QuestID)
        {
            List<Quest> result = new List<Quest>();
            foreach (KeyValuePair<int, Quest> q in quests)
            {
                if (q.Value.Start_Requirement_Quest_ID == QuestID)
                {
                    result.Add(q.Value);
                }
            }
            return result;
        }
        public byte[] PullBuffer()
        {
            WorldBuffer = new ByteBuffer.ByteBuffer();
            // Players
            WorldBuffer.WriteInteger(playersInWorld.Count);
            for (int i = 0; i < playersInWorld.Count; i++)
            {
                WorldBuffer.WriteBytes(playersInWorld[i].GetBuffer());
            }
            // NPCs
            WorldBuffer.WriteInteger(NPCsInWorld.Count);
            for (int i = 0; i < NPCsInWorld.Count; i++)
            {
                WorldBuffer.WriteBytes(NPCsInWorld[i].GetBuffer());
            }
            if (WorldBuffer != null)
            {
                return WorldBuffer.ToArray();
            }
            else
            {
                return null;
            }
        }
    }
}
