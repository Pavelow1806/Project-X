using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Login_Server
{
    class Character
    {
        public string Name = "";
        public int Level = 1;
        public int Gender = 0;

        public Character(string name, int level, int gender)
        {
            Name = name;
            Level = level;
            Gender = gender;
        }
    }
}
