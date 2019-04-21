using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class Experience
    {
        public int XP_ID;
        public int Level;
        public int experience;
        public int Strength;
        public int Agility;
        public int HP;

        public Experience(int xp_id, int level, int _experience, 
            int strength, int agility, int hp)
        {
            XP_ID = xp_id;
            Level = level;
            experience = _experience;
            Strength = strength;
            Agility = agility;
            HP = hp;
        }
    }
}
