using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class World
    {
        public static World instance;

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
        public List<Player> playersInWorld = new List<Player>();

        public Dictionary<int, NPC> NPCs = new Dictionary<int, NPC>();
        
        public Dictionary<int, Quest> quests = new Dictionary<int, Quest>();

        public World()
        {
            instance = this;
        }

        public void Initialise()
        {

        }

        public void Start()
        {

        }

        public void Reset()
        {
            entityCounter = 0;
            players.Clear();
            NPCs.Clear();
            quests.Clear();
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
