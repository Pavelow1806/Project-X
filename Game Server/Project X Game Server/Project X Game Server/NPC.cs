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
        private int nPC_ID = 0;
        public int NPC_ID
        {
            get
            {
                return nPC_ID;
            }
        }
        public int Respawn_Time;
        public bool Active
        {
            get
            {
                if (NextSpawnTime > DateTime.Now)
                {
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
                    NextSpawnTime = DateTime.Now.AddSeconds(Respawn_Time);
                }
            }
        }
        private DateTime NextSpawnTime = default(DateTime);
        public int Spawn_ID = -1;

        public NPC(int ID, NPCStatus Status, string name, int respawn_Time, int level, Gender gender, int HP) :
            base(ID, name, level, gender, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, HP, 100, 10)
        {
            status = Status;
            type = EntityType.NPC;
            Respawn_Time = respawn_Time;
            NextSpawnTime = DateTime.Now;
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
