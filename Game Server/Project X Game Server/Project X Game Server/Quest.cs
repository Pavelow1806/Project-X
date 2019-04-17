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
        InProgress,
        Complete,
        Finished
    }
    class Quest_Log
    {
        #region Progress
        public int Quest_ID;
        public QuestStatus Status;
        public int ObjectiveProgress = 0;
        #endregion

        public Quest_Log(int Character_ID)
        {
            Status = QuestStatus.InProgress;
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
