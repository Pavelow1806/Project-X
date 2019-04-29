using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class Player : Entity
    {
        public int Character_ID = -1;

        public List<Quest_Log> quests = new List<Quest_Log>();

        private float camera_Pos_X = 0.0f;
        public float Camera_Pos_X
        {
            get
            {
                return camera_Pos_X;
            }
            set
            {
                if (camera_Pos_X != value)
                {
                    Changed = true;
                    camera_Pos_X = value;
                }
            }
        }
        private float camera_Pos_Y = 0.0f;
        public float Camera_Pos_Y
        {
            get
            {
                return camera_Pos_Y;
            }
            set
            {
                if (camera_Pos_Y != value)
                {
                    Changed = true;
                    camera_Pos_Y = value;
                }
            }
        }
        private float camera_Pos_Z = 0.0f;
        public float Camera_Pos_Z
        {
            get
            {
                return camera_Pos_Z;
            }
            set
            {
                if (camera_Pos_Z != value)
                {
                    Changed = true;
                    camera_Pos_Z = value;
                }
            }
        }
        private float camera_Rotation_Y = 0.0f;
        public float Camera_Rotation_Y
        {
            get
            {
                return camera_Rotation_Y;
            }
            set
            {
                if (camera_Rotation_Y != value)
                {
                    Changed = true;
                    camera_Rotation_Y = value;
                }
            }
        }
        private int experience = 0;
        public int Experience
        {
            get
            {
                return experience;
            }
        }

        private bool _InWorld = false;
        public bool InWorld
        {
            get
            {
                return _InWorld;
            }
            set
            {
                if (_InWorld != value)
                {
                    Changed = true;
                    _InWorld = value;
                    if (_InWorld)
                    {
                        World.instance.playersInWorld.Add(this);
                    }
                    else
                    {
                        World.instance.playersInWorld.Remove(this);
                    }
                }
            }
        }

        public Player(int _Character_ID, string _Name, int _Level, Gender _gender, float _x, float _y, float _z, float _r,
            float vX, float vY, float vZ, int HP, int Strength, int Agility, int Experience) :
            base (_Character_ID, _Name, _Level, _gender, _x, _y, _z, _r, vX, vY, vZ, HP, Strength, Agility)
        {
            Character_ID = _Character_ID;
            experience = Experience;
        }

        protected override void BuildTransmission(ref ByteBuffer.ByteBuffer buffer)
        {
            base.BuildTransmission(ref buffer);

            buffer.WriteInteger(Character_ID);

            //Changed = false;
            //AnimState.Changed = false;
            //QuestChanged(false);
        }
        private int CountQuests()
        {
            return quests.Count;
        }
        private bool QuestChanged()
        {
            foreach (Quest_Log quest in quests)
            {
                if (quest.Changed)
                {
                    return true;
                }
            }
            return false;
        }
        private void QuestChanged(bool status)
        {
            foreach (Quest_Log quest in quests)
            {
                quest.Changed = status;
            }
        }
        public void BuildBuffer(ref ByteBuffer.ByteBuffer buffer)
        {
            BuildTransmission(ref buffer);
        }
    }
}
