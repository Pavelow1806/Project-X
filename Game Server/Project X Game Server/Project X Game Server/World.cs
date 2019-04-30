using System;
using System.Collections.Concurrent;
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
        public static int TickRate = 10;
        public static int HealthRegenPer5 = 1;
        public static int HealAmount = 20;
        public static int StompDamage = 20;
        public static int BloodDamageIncrease = 20;
        private static DateTime NextTick = default(DateTime);
        private int entityCounter = 0;
        public int EntityCounter
        {
            get
            {
                return ++entityCounter;
            }
        }
        private ByteBuffer.ByteBuffer WorldBuffer = null;

        public ConcurrentDictionary<int, Player> players = new ConcurrentDictionary<int, Player>();
        public bool ReceivedPlayers = false;
        public List<Player> playersInWorld = new List<Player>();

        public ConcurrentDictionary<int, NPC> NPCs = new ConcurrentDictionary<int, NPC>();
        public bool ReceivedNPCs = false;
        public List<NPC> NPCsInWorld = new List<NPC>();
        
        public ConcurrentDictionary<int, Collectable> collectables = new ConcurrentDictionary<int, Collectable>();
        public bool ReceivedCollectables = false;
        public ConcurrentBag<Collectable> collectablesInWorld = new ConcurrentBag<Collectable>();

        public ConcurrentDictionary<int, Spawn> spawns = new ConcurrentDictionary<int, Spawn>();
        public bool ReceivedSpawns = false;

        public ConcurrentDictionary<int, Quest> quests = new ConcurrentDictionary<int, Quest>();
        public bool ReceivedQuests = false;

        public ConcurrentBag<Quest_Log> quest_log = new ConcurrentBag<Quest_Log>();
        public bool ReceivedQuestLogs = false;

        public ConcurrentDictionary<int, Experience> experience_levels = new ConcurrentDictionary<int, Experience>();
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
            collectablesInWorld = new ConcurrentBag<Collectable>();
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
                        collectablesInWorld.Last().Spawn_ID = spawn.Key;
                        ++i_col;
                    }
                }
            }
            Running = true;
            // Main loop
            NextTick = DateTime.Now.AddSeconds(5);
            while (Running)
            {
                try
                {
                    if (DateTime.Now >= NextTick)
                    {
                        foreach (NPC npc in NPCsInWorld)
                        {
                            if (npc.Active && npc.Current_HP > 0)
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
                        foreach (Player player in playersInWorld)
                        {
                            if (player.Current_HP + HealthRegenPer5 > player.Max_HP)
                            {
                                player.Current_HP = player.Max_HP;
                            }
                            else
                            {
                                player.Current_HP += HealthRegenPer5;
                                SendData.Heal(Network.instance.GetIndex(player.Character_ID), player.Current_HP, HealAmount);
                            }
                            if (player.InCombat && MathF.Distance(player, GetNPCByEntityID(player.TargetID)) > InteractionDistance)
                            {
                                player.InCombat = false;
                                player.AnimState.Attacking = false;
                            }
                        }
                        NextTick = DateTime.Now.AddSeconds(5);
                    }
                    foreach (NPC npc in NPCsInWorld)
                    {
                        if (npc.TargetID > -1 && npc.Current_HP > 0 && npc.Active)
                        {
                            if (npc.InCombat && players[npc.TargetID].InWorld && MathF.Distance(npc, players[npc.TargetID]) <= InteractionDistance)
                            {
                                if (DateTime.Now >= npc.NextAttack && npc.TargetID > 0 &&
                                    players[npc.TargetID].Current_HP > 0)
                                {
                                    int Damage = MathF.Damage(npc.Strength, npc.Agility, npc.BloodMultiplier);
                                    players[npc.TargetID].Current_HP -= Damage;
                                    SendData.Attacked(Network.instance.GetIndex(npc.TargetID), players[npc.TargetID].Current_HP, Damage);
                                    npc.NextAttack = DateTime.Now.AddSeconds(GlobalAttackSpeed);
                                }
                            }
                            else if (!players[npc.TargetID].InWorld || (npc.InCombat && MathF.Distance(npc, players[npc.TargetID]) > InteractionDistance))
                            {
                                players[npc.TargetID].InCombat = false;
                                npc.InCombat = false;
                                npc.TargetID = -1;
                                npc.TargetType = EntityType.NONE;
                            }
                        }
                        else if (npc.TargetID == -1 && npc.Active)
                        {
                            npc.InCombat = false;
                            npc.TargetID = -1;
                            npc.TargetType = EntityType.NONE;
                        }
                        else if (npc.Active && npc.Current_HP <= 0)
                        {
                            if (npc.TargetID > -1)
                                players[npc.TargetID].InCombat = false;
                            npc.InCombat = false;
                            npc.TargetID = -1;
                            npc.TargetType = EntityType.NONE;
                            npc.Active = false;
                        }
                    }
                    foreach (Player p in playersInWorld)
                    {
                        if (DateTime.Now > p.BloodExpire)
                        {
                            p.BloodMultiplier = 1;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.log("An error was caught during the world loop: " + e.Message);
                }
                Thread.Sleep(TickRate);
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
            foreach (KeyValuePair<int, Quest> q in quests)
            {
                if (GetQuestStatus(Character_ID, q.Value.ID) == QuestStatus.Complete)
                {
                    List<Quest> NextQuests = GetNextQuests(q.Value.ID);
                    if (NextQuests != null && NextQuests.Count > 0)
                    {
                        for (int i = 0; i < NextQuests.Count; i++)
                        {
                            if (GetQuestStatus(Character_ID, NextQuests[i].ID) == QuestStatus.None ||
                                GetQuestStatus(Character_ID, NextQuests[i].ID) == QuestStatus.Available)
                            {
                                result.Add(NextQuests[i]);
                            }
                        }
                    }
                }
            }
            return result;
        }
        public void UpdateQuestLog(int Character_ID)
        {
            List<Quest_Log> QuestLog = GetQuestLog(Character_ID);
            List<Quest> ToBeChecked = new List<Quest>();
            bool found = false;
            foreach (KeyValuePair<int, Quest> q in quests)
            {
                found = false;
                foreach (Quest_Log ql in QuestLog)
                {
                    if (ql.Quest_ID == q.Value.ID)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    ToBeChecked.Add(q.Value);
                }
            }
            foreach (Quest quest in ToBeChecked)
            {
                Quest_Log QL = GetQuestLog(Character_ID, quest.Start_Requirement_Quest_ID);
                if ((QL != null && QL.Status == QuestStatus.Complete) || quest.Start_Requirement_Quest_ID == -1)
                {
                    Quest_Log quest_Log = new Quest_Log(-1, Character_ID, quest.ID, QuestStatus.Available, 0);
                    World.instance.quest_log.Add(quest_Log);
                    SendData.CreateQuestLog(quest_Log);
                }
            }
        }
        public QuestStatus GetQuestStatus(int Character_ID, int Quest_ID)
        {
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Character_ID == Character_ID && ql.Quest_ID == Quest_ID)
                {
                    return ql.Status;
                }
            }
            return QuestStatus.None;
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
            World.instance.UpdateQuestLog(Character_ID);
            QuestStatus result = QuestStatus.None;
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Status == QuestStatus.Available && quests[ql.Quest_ID].NPC_Start_ID == NPC_ID)
                {
                    result = ql.Status;
                }
                else if ((ql.Status == QuestStatus.InProgress || ql.Status == QuestStatus.Finished) && quests[ql.Quest_ID].NPC_End_ID == NPC_ID)
                {
                    if (result != QuestStatus.Available)
                    {
                        result = ql.Status;
                    }
                }
            }
            if (result == QuestStatus.None)
            {
                List<Quest> AvailableQuests = GetAvailableQuests(Character_ID);
                foreach (Quest q in AvailableQuests)
                {
                    if (q.NPC_Start_ID == NPC_ID)
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
        public QuestReturn GetQuestContentByNPCEntityID(int Character_ID, int NPC_Entity_ID, out bool Create)
        {
            World.instance.UpdateQuestLog(Character_ID);
            QuestReturn result = new QuestReturn(true, QuestStatus.None, -1, -1, "", "", -1);
            NPC Subject = GetNPCByEntityID(NPC_Entity_ID);
            List<Quest> NPCQuests = GetNPCQuests(Subject.NPC_ID);
            List<int> Quests_In_log = new List<int>();
            foreach (Quest_Log ql in quest_log)
            {
                if (ql.Character_ID == Character_ID)
                {
                    //if (GetNPCEntityID(quests[ql.Quest_ID].NPC_Start_ID) == NPC_Entity_ID)
                    //{
                    if (GetNPCEntityID(quests[ql.Quest_ID].NPC_Start_ID) == NPC_Entity_ID ||
                        GetNPCEntityID(quests[ql.Quest_ID].NPC_End_ID) == NPC_Entity_ID)
                    {
                        if (ql.Status == QuestStatus.Available)
                        {
                            result.Null = false;
                            result.Status = ql.Status;
                            result.NPC_ID = GetNPCEntityID(quests[ql.Quest_ID].NPC_Start_ID);
                            result.Quest_ID = ql.Quest_ID;
                            result.Target = quests[ql.Quest_ID].Objective_Target;
                            result.Text = quests[ql.Quest_ID].Start_Text;
                            result.Title = quests[ql.Quest_ID].Title;
                        }
                        else if (ql.Status == QuestStatus.InProgress)
                        {
                            if (result.Status != QuestStatus.Available)
                            {
                                result.Null = false;
                                result.Status = ql.Status;
                                result.NPC_ID = GetNPCEntityID(quests[ql.Quest_ID].NPC_Start_ID);
                                result.Quest_ID = ql.Quest_ID;
                                result.Target = quests[ql.Quest_ID].Objective_Target;
                                result.Text = quests[ql.Quest_ID].Start_Text;
                                result.Title = quests[ql.Quest_ID].Title;
                            }
                        }
                        //}
                        //if (GetNPCEntityID(quests[ql.Quest_ID].NPC_End_ID) == NPC_Entity_ID)
                        //{
                        if (ql.Status == QuestStatus.Finished)
                        {
                            if (result.Status != QuestStatus.Available)
                            {
                                result.Null = false;
                                result.Status = ql.Status;
                                result.NPC_ID = GetNPCEntityID(quests[ql.Quest_ID].NPC_End_ID);
                                result.Quest_ID = ql.Quest_ID;
                                result.Target = quests[ql.Quest_ID].Objective_Target;
                                result.Text = quests[ql.Quest_ID].End_Text;
                                result.Title = quests[ql.Quest_ID].Title;
                            }
                        }
                        else if (ql.Status == QuestStatus.Complete)
                        {
                            if (result.Status != QuestStatus.Available)
                            {
                                result.Null = false;
                                result.Status = ql.Status;
                                result.NPC_ID = GetNPCEntityID(quests[ql.Quest_ID].NPC_End_ID);
                                result.Quest_ID = ql.Quest_ID;
                                result.Target = quests[ql.Quest_ID].Objective_Target;
                                result.Text = quests[ql.Quest_ID].End_Text;
                                result.Title = quests[ql.Quest_ID].Title;
                            }
                        }
                    }
                    //}
                }
            }
            if (result.Status == QuestStatus.None)
            {
                List<Quest> AvailableQuests = GetAvailableQuests(Character_ID);
                foreach (Quest q in AvailableQuests)
                {
                    if (GetNPCEntityID(q.NPC_End_ID) == NPC_Entity_ID || GetNPCEntityID(q.NPC_Start_ID) == NPC_Entity_ID)
                    {
                        bool found = false;
                        for (int i = 0; i < Quests_In_log.Count; i++)
                        {
                            if (q.ID == Quests_In_log[i])
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            result.Null = false;
                            result.Status = QuestStatus.Available;
                            result.NPC_ID = NPC_Entity_ID;
                            result.Quest_ID = q.ID;
                            result.Target = quests[q.ID].Objective_Target;
                            result.Text = quests[q.ID].End_Text;
                            result.Title = quests[q.ID].Title;
                            Create = true;
                            return result;
                        }
                    }
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
        public void BuildBuffer(ref ByteBuffer.ByteBuffer buffer)
        {
            buffer = new ByteBuffer.ByteBuffer();
            buffer.WriteString(DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            // Players
            buffer.WriteInteger(playersInWorld.Count);
            for (int i = 0; i < playersInWorld.Count; i++)
            {
                playersInWorld[i].BuildBuffer(ref buffer);
            }
            // NPCs
            buffer.WriteInteger(NPCsInWorld.Count);
            for (int i = 0; i < NPCsInWorld.Count; i++)
            {
                NPCsInWorld[i].BuildBuffer(ref buffer);
            }
        }
    }
}
