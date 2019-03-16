using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    class Quest
    {
        #region General
        public int ID = -1;
        public string Title = "";
        public string StartText = "";
        public string EndText = "";
        public int Reward = -1;
        public int NPCStartID = -1;
        public int NPCEndID = -1;
        public int ObjectiveTarget = -1;
        public int StartRequirementQuestID = -1;
        #endregion

        #region Progress
        private bool _Complete = false;
        public bool Complete
        {
            get
            {
                return _Complete;
            }
            set
            {
                if (_Complete != value)
                {
                    Changed = true;
                    _Complete = value;
                }
            }
        }
        private bool _TurnedIn = false;
        public bool TurnedIn
        {
            get
            {
                return _TurnedIn;
            }
            set
            {
                if (_TurnedIn != value)
                {
                    Changed = true;
                    _TurnedIn = value;
                }
            }
        }
        private bool _Active = false;
        public bool Active
        {
            get
            {
                return _Active;
            }
            set
            {
                if (_Active != value)
                {
                    Changed = true;
                    _Active = value;
                }
            }
        }
        private int _ObjectiveProgress = 0;
        public int ObjectiveProgress
        {
            get
            {
                return _ObjectiveProgress;
            }
            set
            {
                if (_ObjectiveProgress != value)
                {
                    Changed = true;
                    _ObjectiveProgress = value;
                }
            }
        }

        public bool Changed = false;
        #endregion
    }
}
