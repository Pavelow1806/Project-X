using System;
using System.Collections.Concurrent;
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
                if (NextSpawnTime < DateTime.Now && NextSpawnTime != default(DateTime))
                {
                    Log.log("Entity: " + Name + " with ID: " + Entity_ID.ToString() + " has become active.", Log.LogType.GENERAL);
                    NextSpawnTime = default(DateTime);
                    Current_HP = Max_HP;
                    position = spawn;
                    return true;
                }
                else if (NextSpawnTime < DateTime.Now)
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
                    Log.log("Entity: " + Name + " with ID: " + Entity_ID.ToString() + " has become non-active.", Log.LogType.GENERAL);
                    NextSpawnTime = DateTime.Now.AddSeconds(Respawn_Time);
                }
            }
        }
        private DateTime NextSpawnTime = default(DateTime);
        public int Spawn_ID = -1;
        public int Experience;
        public ConcurrentBag<int> PlayerCredit = new ConcurrentBag<int>();

        public NPC(int ID, NPCStatus Status, string name, int respawn_Time, int level, Gender gender, int HP,
            int Strength, int Agility, int experience) :
            base(ID, name, level, gender, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, HP, Strength, Agility)
        {
            nPC_ID = ID;
            status = Status;
            type = EntityType.NPC;
            Respawn_Time = respawn_Time;
            NextSpawnTime = DateTime.Now;
            Experience = experience;
        }

        protected override void BuildTransmission(ref ByteBuffer.ByteBuffer buffer)
        {
            base.BuildTransmission(ref buffer);

            buffer.WriteInteger(NPC_ID);

            //Changed = false;
            //AnimState.Changed = false;
        }
        public void BuildBuffer(ref ByteBuffer.ByteBuffer buffer)
        {
            BuildTransmission(ref buffer);
        }
    }
}
