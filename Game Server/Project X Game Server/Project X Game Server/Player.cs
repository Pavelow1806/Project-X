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

        public List<Quest> quests = new List<Quest>();

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

        private int _TargetID = -1;
        public int TargetID
        {
            get
            {
                return _TargetID;
            }
            set
            {
                if (_TargetID != value)
                {
                    Changed = true;
                    _TargetID = value;
                }
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
                }
            }
        }

        public Player(int _Character_ID, string _Name, int _Level, float _x, float _y, float _z, float _r) :
            base (_Character_ID, _Name, _Level, _x, _y, _z, _r)
        {
            Character_ID = _Character_ID;
        }

        protected override void BuildTransmission(out byte[] result)
        {
            if (Changed || AnimState.Changed || QuestChanged())
            {
                base.BuildTransmission(out result);

                buffer.WriteFloat(camera_Pos_X);
                buffer.WriteFloat(camera_Pos_Y);
                buffer.WriteFloat(camera_Pos_Z);
                buffer.WriteFloat(camera_Rotation_Y);

                buffer.WriteInteger(CountQuests());
                foreach (Quest quest in quests)
                {
                    buffer.WriteByte((byte)((quest.Complete) ? 1 : 0));
                    buffer.WriteByte((byte)((quest.TurnedIn) ? 1 : 0));
                    buffer.WriteByte((byte)((quest.Active) ? 1 : 0));
                    buffer.WriteInteger(quest.ObjectiveProgress);
                }

                Changed = false;
                AnimState.Changed = false;
                QuestChanged(false);
            }
            result = buffer.ToArray();
        }
        private int CountQuests()
        {
            int count = 0;
            foreach (Quest quest in quests)
            {
                ++count;
            }
            return count;
        }
        private bool QuestChanged()
        {
            foreach (Quest quest in quests)
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
            foreach (Quest quest in quests)
            {
                quest.Changed = status;
            }
        }
        public byte[] GetBuffer()
        {
            byte[] result;
            BuildTransmission(out result);
            return result;
        }
    }
}
