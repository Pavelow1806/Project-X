using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    public struct QuestReturn
    {
        public bool Null;
        public QuestStatus Status;
        public int Quest_ID;
        public int NPC_ID;
        public string Title;
        public string Text;
        public int Target;
        public QuestReturn(bool _Null, QuestStatus status, int quest_ID, int npc_ID, string title, string text, int target)
        {
            Null = _Null;
            Status = status;
            Quest_ID = quest_ID;
            NPC_ID = npc_ID;
            Title = title;
            Text = text;
            Target = target;
        }
    }
    class Quest
    {
        #region General
        private int iD = -1;
        public int ID
        {
            get
            {
                return iD;
            }
        }
        private string title = "";
        public string Title
        {
            get
            {
                return title;
            }
        }
        private string start_Text = "";
        public string Start_Text
        {
            get
            {
                return start_Text;
            }
        }
        private string end_Text = "";
        public string End_Text
        {
            get
            {
                return end_Text;
            }
        }
        private int reward = -1;
        public int Reward
        {
            get
            {
                return reward;
            }
        }
        private int nPC_Start_ID = -1;
        public int NPC_Start_ID
        {
            get
            {
                return nPC_Start_ID;
            }
        }
        private int nPC_End_ID = -1;
        public int NPC_End_ID
        {
            get
            {
                return nPC_End_ID;
            }
        }
        private int objective_Target = -1;
        public int Objective_Target
        {
            get
            {
                return objective_Target;
            }
        }
        private int start_Requirement_Quest_ID = -1;
        public int Start_Requirement_Quest_ID
        {
            get
            {
                return start_Requirement_Quest_ID;
            }
        }
        #endregion


        protected bool Changed = false;

        protected ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();

        public Quest(int id, string Title, string Start_text, string End_text, int Reward, 
            int Npc_start_id, int Npc_end_id, int Objective_target, int Start_requirement_quest_id,
            int Item_objective_id, int Npc_objective_id)
        {
            iD = id;
            title = title;

        }
    }
    public enum QuestStatus
    {
        None,
        InProgress,
        Complete,
        Finished,
        Available
    }
    class Quest_Log
    {
        private int quest_Log_ID;
        public int Quest_Log_ID
        {
            get
            {
                return quest_Log_ID;

            }
        }
        private int character_ID;
        public int Character_ID
        {
            get
            {
                return character_ID;
            }
        }
        private int quest_ID = 0;
        public int Quest_ID
        {
            get
            {
                return quest_ID;
            }
        }
        private QuestStatus status;
        public QuestStatus Status
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
        private int objectiveProgress;
        public int ObjectiveProgress
        {
            get
            {
                return objectiveProgress;
            }
            set
            {
                if (objectiveProgress != value)
                {
                    Changed = true;
                    objectiveProgress = value;
                }
            }
        }

        public bool Changed = false;

        public Quest_Log(int Quest_Log_ID, int Character_ID, int Quest_ID, QuestStatus Status, int Progress)
        {
            quest_Log_ID = Quest_Log_ID;
            character_ID = Character_ID;
            quest_ID = Quest_ID;
            status = Status;
            objectiveProgress = Progress;
        }

        public void Increment(int Character_ID)
        {
            if (ObjectiveProgress + 1 < World.instance.quests[Quest_ID].Objective_Target)
            {
                ++ObjectiveProgress;
            }
            else if (ObjectiveProgress + 1 >= World.instance.quests[Quest_ID].Objective_Target)
            {
                ++ObjectiveProgress;
                Status = QuestStatus.Finished;
            }
        }

        public void TurnIn(int Character_ID)
        {
            if (ObjectiveProgress >= World.instance.quests[Quest_ID].Objective_Target)
            {
                Status = QuestStatus.Complete;
            }
        }
    }
}
