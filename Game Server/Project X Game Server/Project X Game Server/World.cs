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
        public List<Player> players = new List<Player>();
        public List<NPC> NPCs = new List<NPC>();
        
        public List<Quest> quests = new List<Quest>();

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
    }
}
