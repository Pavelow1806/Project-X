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

        public Player(int ID, int _Character_ID, string _Name, int _Level, float _x, float _y, float _z) :
            base (ID, "", 0,0,0,0)
        {
            Character_ID = _Character_ID;
        }

        protected override void BuildTransmission(out byte[] result)
        {
            if (Changed || AnimState.Changed || QuestChanged())
            {
                base.BuildTransmission(out result);

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
    }
}
