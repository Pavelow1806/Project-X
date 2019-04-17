using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    public enum NPCStatus
    {
        AGGRESSIVE,
        NEUTRAL,
        FRIENDLY,
        BOSS
    }
    class NPC : Entity
    {
        private NPCStatus status;
        public NPCStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status != value)
                {
                    Changed = true;
                    status = value;
                }
            }
        }

        public NPC(int ID, NPCStatus Status, string name, int level, Gender gender, float x, float y, float z, float r, int HP) :
            base (ID, name, level, gender, x, y, z, r, 0.0f, 0.0f, 0.0f, HP)
        {
            status = Status;
            type = EntityType.NPC;
        }

        protected override void BuildTransmission(out byte[] result)
        {
            if (Changed || AnimState.Changed)
            {
                base.BuildTransmission(out result);

                buffer.WriteInteger((int)status);

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
