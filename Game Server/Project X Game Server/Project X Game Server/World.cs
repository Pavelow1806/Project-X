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
        public Dictionary<int, NPC> NPCsInWorld = new Dictionary<int, NPC>();
        
        public Dictionary<int, Collectable> collectables = new Dictionary<int, Collectable>();
        public bool ReceivedCollectables = false;
        public Dictionary<int, Collectable> collectablesInWorld = new Dictionary<int, Collectable>();

        public Dictionary<int, Spawn> spawns = new Dictionary<int, Spawn>();
        public bool ReceivedSpawns = false;

        public Dictionary<int, Quest> quests = new Dictionary<int, Quest>();
        public bool ReceivedQuests = false;

        public World()
        {
            instance = this;
        }

        public void Initialise()
        {
            if (ReceivedPlayers && ReceivedNPCs && ReceivedCollectables && ReceivedSpawns && ReceivedQuests)
            {
                UpdateThread = new Thread(new ThreadStart(Start));
                UpdateThread.Start();
            }
        }

        public void Start()
        {
            Log.log("World Update Thread starting..", Log.LogType.SYSTEM);
            // Spawn all of the NPC's and Collectables
            foreach (KeyValuePair<int, Spawn> spawn in spawns)
            {
                if (!spawn.Value.InUse)
                {
                    if (spawn.Value.NPC_ID > -1)
                    {
                        int CurrentEntityNumber = EntityCounter;
                        NPCsInWorld.Add(CurrentEntityNumber, NPCs[spawn.Value.NPC_ID]);
                        NPCsInWorld[CurrentEntityNumber].x = spawn.Value.Pos_X;
                        NPCsInWorld[CurrentEntityNumber].y = spawn.Value.Pos_Y;
                        NPCsInWorld[CurrentEntityNumber].z = spawn.Value.Pos_Z;
                        NPCsInWorld[CurrentEntityNumber].r = spawn.Value.Rotation_Y;
                        NPCsInWorld[CurrentEntityNumber].Spawn_ID = spawn.Key;
                    }
                    else if (spawn.Value.Collectable_ID > -1)
                    {
                        int CurrentEntityNumber = EntityCounter;
                        collectablesInWorld.Add(CurrentEntityNumber, collectables[spawn.Value.Collectable_ID]);
                        collectablesInWorld[CurrentEntityNumber].x = spawn.Value.Pos_X;
                        collectablesInWorld[CurrentEntityNumber].y = spawn.Value.Pos_Y;
                        collectablesInWorld[CurrentEntityNumber].z = spawn.Value.Pos_Z;
                        collectablesInWorld[CurrentEntityNumber].r = spawn.Value.Rotation_Y;
                        collectablesInWorld[CurrentEntityNumber].Spawn_ID = spawn.Key;
                    }
                }
            }
            while (Running)
            {
                foreach (KeyValuePair<int, NPC> npc in NPCsInWorld)
                {
                    if (npc.Value.Active)
                    {
                        if (npc.Value.Current_HP <= 0)
                        {
                            npc.Value.AnimState.Dead = true;
                            npc.Value.Active = false;
                        }
                        else
                        {
                            npc.Value.AnimState.Dead = false;
                        }
                    }
                }
                foreach (KeyValuePair<int, Collectable> col in collectablesInWorld)
                {
                    if (col.Value.Active)
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
