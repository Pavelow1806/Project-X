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

        public int EntityCounter = 0;
        public List<Entity> entities = new List<Entity>();

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
            EntityCounter = 0;
            entities.Clear();
            quests.Clear();
        }
    }
}
