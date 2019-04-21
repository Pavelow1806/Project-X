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
        public static float GlobalAttackSpeed = 1.0f;
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

        public Dictionary<int, Quest_Log> quest_log = new Dictionary<int, Quest_Log>();
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
                                NPCs[spawn.Value.NPC_ID].Max_HP
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
            while (Running)
            {
                foreach (NPC npc in NPCsInWorld)
                {
                    if (npc.Active)
                    {
                        if (npc.Current_HP <= 0)
                        {
                            npc.AnimState.Dead = true;
                            npc.Active = false;
                        }
                        else
                        {
                            npc.AnimState.Dead = false;
                        }
                    }
                }
                foreach (Collectable col in collectablesInWorld)
                {
                    if (col.Active)
                    {
                        //if (col.val)
                        //{

                        //}
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
            foreach (KeyValuePair<int, Quest_Log> ql in quest_log)
            {
                if (ql.Value.Character_ID == Character_ID)
                {
                    result.Add(ql.Value);
                }
            }
            return result;
        }
        public List<Quest> GetAvailableQuests(int Character_ID)
        {
            List<Quest> result = new List<Quest>();
            int MaxCompletedQuestID = 0;
            foreach (KeyValuePair<int, Quest_Log> ql in quest_log)
            {
                if (ql.Value.Character_ID == Character_ID && 
                    ql.Value.Status == QuestStatus.Complete && 
                    ql.Value.Quest_ID > MaxCompletedQuestID)
                {
                    MaxCompletedQuestID = ql.Value.Quest_ID;
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
            foreach (KeyValuePair<int, NPC> npc in NPCs)
            {
                if (npc.Value.Entity_ID == Entity_ID)
                {
                    return npc.Value;
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
            foreach (KeyValuePair<int, Quest_Log> questlog in quest_log)
            {
                if (questlog.Value.Character_ID == Character_ID)
                {
                    Character_Quest_Log.Add(questlog.Value);
                }
            }
            QuestStatus result = QuestStatus.None;
            foreach (Quest q in NPC_Quests)
            {
                foreach (Quest_Log ql in Character_Quest_Log)
                {
                    if (q.ID == ql.Quest_ID)
                    {
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
                            result = ql.Status;
                        }
                        else if (ql.Status == QuestStatus.InProgress && 
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
            return result;
        }
        public QuestReturn GetQuestContentByNPC(int Character_ID, int NPC_ID)
        {
            List<Quest> NPCQuests = new List<Quest>();
            foreach (KeyValuePair<int, Quest> q in quests)
            {
                if (q.Value.NPC_Start_ID == NPC_ID || q.Value.NPC_End_ID == NPC_ID)
                {
                    NPCQuests.Add(q.Value);
                }
            }
            foreach (Quest qu in NPCQuests)
            {
                foreach (KeyValuePair<int, Quest_Log> ql in quest_log)
                {
                    if (qu.ID == ql.Value.Quest_ID)
                    {
                        if (ql.Value.Status == QuestStatus.Available)
                        {
                            return new QuestReturn(false, ql.Value.Status, qu.ID, qu.NPC_Start_ID, qu.Title, qu.Start_Text, qu.Objective_Target);
                        }
                        else if (ql.Value.Status == QuestStatus.Finished)
                        {
                            return new QuestReturn(false, ql.Value.Status, qu.ID, qu.NPC_End_ID, qu.Title, qu.End_Text, qu.Objective_Target);
                        }
                    }
                }
            }
            return new QuestReturn(true, QuestStatus.None, -1, -1, "", "", -1);
        }
        public Quest_Log GetQuestLog(int Character_ID, int Quest_ID)
        {
            foreach (KeyValuePair<int, Quest_Log> ql in quest_log)
            {
                if (ql.Value.Quest_ID == Quest_ID && ql.Value.Character_ID == Character_ID)
                {
                    return ql.Value;
                }
            }
            return null;
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
