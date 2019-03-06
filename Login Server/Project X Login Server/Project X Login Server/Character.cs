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

        public Character(string name, int level)
        {
            Name = name;
            Level = level;
        }
    }
}
