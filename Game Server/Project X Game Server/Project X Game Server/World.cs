using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    public enum BufferReturn
    {
        Player,
        NPC
    }
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
        public Dictionary<int, Player> players = new Dictionary<int, Player>();
        private ByteBuffer.ByteBuffer playerBuffer = new ByteBuffer.ByteBuffer();

        public Dictionary<int, NPC> NPCs = new Dictionary<int, NPC>();
        private ByteBuffer.ByteBuffer npcBuffer = new ByteBuffer.ByteBuffer();
        
        public Dictionary<int, Quest> quests = new Dictionary<int, Quest>();
        private ByteBuffer.ByteBuffer questBuffer = new ByteBuffer.ByteBuffer();

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
        public byte[] PullBuffer(BufferReturn request)
        {
            switch (request)
            {
                case BufferReturn.Player:
                    playerBuffer = null;
                    foreach (KeyValuePair<int, Player> player in players)
                    {
                        if (player.Value.InWorld)
                        {
                            playerBuffer.WriteBytes(player.Value.GetBuffer());
                        }
                    }
                    if (playerBuffer != null)
                    {
                        return playerBuffer.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                case BufferReturn.NPC:
                    npcBuffer = null;
                    foreach (KeyValuePair<int, NPC> npc in NPCs)
                    {
                        npcBuffer.WriteBytes(npc.Value.GetBuffer());
                    }
                    if (npcBuffer != null)
                    {
                        return npcBuffer.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }
    }
}
