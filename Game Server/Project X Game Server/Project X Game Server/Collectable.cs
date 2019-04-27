using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class Collectable : Entity
    {
        private int collectable_ID = 0;
        public int Collectable_ID
        {
            get
            {
                return collectable_ID;
            }
        }
        public int Respawn_Time;
        private bool PreviousActiveState = true;
        public bool Active
        {
            get
            {
                if (DateTime.Now > NextSpawnTime)
                {
                    if (PreviousActiveState != true)
                    {
                        SendData.CollectableToggle(Entity_ID, true);
                    }
                    PreviousActiveState = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == false)
                {
                    if (PreviousActiveState != false)
                    {
                        SendData.CollectableToggle(Entity_ID, true);
                    }
                    PreviousActiveState = false;
                    NextSpawnTime = DateTime.Now.AddSeconds(Respawn_Time);
                }
            }
        }
        private DateTime NextSpawnTime = default(DateTime);
        public int Spawn_ID = -1;

        public Collectable(int ID, string name, int respawn_Time, float x, float y, float z, float r) : 
            base (ID, name, 0, Gender.NA, x,y,z,r,0,0,0,0,0,0)
        {
            collectable_ID = ID;
            Respawn_Time = respawn_Time;
            NextSpawnTime = DateTime.Now;
        }
    }
}
