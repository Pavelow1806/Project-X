using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class NPC : Entity
    {
        public List<Quest> quests = new List<Quest>();

        public NPC(int ID) :
            base (ID, "", 0,0,0,0)
        {
            type = EntityType.NPC;
        }

        protected override void BuildTransmission(out byte[] result)
        {
            if (Changed || AnimState.Changed)
            {
                base.BuildTransmission(out result);



                Changed = false;
                AnimState.Changed = false;
            }
            result = buffer.ToArray();
        }
        public byte[] GetBuffer()
        {
            byte[] result;
            BuildTransmission(out result);
            return result;
        }
    }
}
