using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Synchronization_Server
{
    public enum CachedTables
    {
        tbl_Characters
    }
    class Data
    {
        #region Table Storage
        // Accounts table
        public static Dictionary<int, _Accounts> tbl_Accounts = new Dictionary<int, _Accounts>();
        //Activity table
        public static Dictionary<int, _Activity> tbl_Activity = new Dictionary<int, _Activity>();
        // Characters table
        public static Dictionary<int, _Characters> tbl_Characters = new Dictionary<int, _Characters>();
        // Quests table
        public static Dictionary<int, _Quests> tbl_Quests = new Dictionary<int, _Quests>();
        // Quest Log table
        public static Dictionary<int, _Quest_Log> tbl_Quest_Log = new Dictionary<int, _Quest_Log>();
        // NPC table
        public static Dictionary<int, _NPC> tbl_NPC = new Dictionary<int, _NPC>();
        // Collectable table
        public static Dictionary<int, _Collectables> tbl_Collectables = new Dictionary<int, _Collectables>();
        #endregion

        private static List<string> UpdateQueries;

        public static void Initialise()
        {
            int LineNumber = Log.log("Performing initial synchronization of database..", Log.LogType.CACHE);

            Response r = Database.instance.Load(LineNumber);
            switch (r)
            {
                case Response.SUCCESSFUL:
                    Log.log("Initial synchronization of database successful.", Log.LogType.SUCCESS);
                    break;
                case Response.UNSUCCESSFUL:
                    Log.log("Initial synchronization of database unsuccessful.", Log.LogType.ERROR);
                    break;
                case Response.ERROR:
                    Log.log("Initial synchronization of database unsuccessful, fix errors and try again.", Log.LogType.ERROR);
                    break;
                default:
                    break;
            }
        }
        public static List<string> BuildQueryList(int LineNumber)
        {
            Log.log(LineNumber, "Starting synchronization of data.. Building query list from cache..", Log.LogType.SYNC);
            UpdateQueries = new List<string>();
            int NumberQueries = 0;
            int SubLineNumber = -1;
            foreach (KeyValuePair<int, _Accounts> account in tbl_Accounts)
            {
                if (account.Value.SQL != "")
                {
                    UpdateQueries.Add(account.Value.SQL);
                    account.Value.SQL = "";
                    ++NumberQueries;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                }
            }
            foreach (KeyValuePair<int, _Activity> activity in tbl_Activity)
            {
                if (activity.Value.SQL != "")
                {
                    UpdateQueries.Add(activity.Value.SQL);
                    activity.Value.SQL = "";
                    ++NumberQueries;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                }
            }
            foreach (KeyValuePair<int, _Characters> character in tbl_Characters)
            {
                if (character.Value.SQL != "")
                {
                    UpdateQueries.Add(character.Value.SQL);
                    character.Value.SQL = "";
                    ++NumberQueries;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                }
            }
            foreach (KeyValuePair<int, _Quest_Log> log in tbl_Quest_Log)
            {
                if (log.Value.SQL != "")
                {
                    UpdateQueries.Add(log.Value.SQL);
                    log.Value.SQL = "";
                    ++NumberQueries;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                }
            }
            foreach (KeyValuePair<int, _NPC> npc in tbl_NPC)
            {
                if (npc.Value.SQL != "")
                {
                    UpdateQueries.Add(npc.Value.SQL);
                    npc.Value.SQL = "";
                    ++NumberQueries;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Found " + NumberQueries.ToString() + " queries so far..", Log.LogType.SYNC);
                    }
                }
            }
            return UpdateQueries;
        }

        public static _Quest_Log ContainsKey(int character_ID, int quest_ID)
        {
            foreach (KeyValuePair<int, _Quest_Log> ql in tbl_Quest_Log)
            {
                if (ql.Value.Character_ID == character_ID && ql.Value.Quest_ID == quest_ID)
                {
                    return ql.Value;
                }
            }
            return null;
        }
    }

    class Record
    {
        public bool Changed = false;
        public bool New = false;
        public bool Deleted = false;

        public string SQL = "";
    }
    class _Accounts : Record
    {
        private int account_ID = 0;
        public int Account_ID
        {
            get
            {
                return account_ID;
            }
        }
        private string username = "";
        public string Username
        {
            get
            {
                return username;
            }
        }
        private string email = "";
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                if (email != value)
                {
                    Changed = true;
                    email = value;
                    SQL = CreateSQL();
                }
            }
        }
        private string password = "";
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                if (password != value)
                {
                    Changed = true;
                    password = value;
                    SQL = CreateSQL();
                }
            }
        }
        private bool logged_In = false;
        public bool Logged_In
        {
            get
            {
                return logged_In;
            }
            set
            {
                if (logged_In != value)
                {
                    Changed = true;
                    logged_In = value;
                    SQL = CreateSQL();
                }
            }
        }

        public _Accounts(int Account_ID, string Username, string Email, string Password, bool Logged_In)
        {
            account_ID = Account_ID;
            username = Username;
            email = Email;
            password = Password;
            logged_In = Logged_In;
            New = false;
        }
        private string CreateSQL()
        {
            if (Changed)
            {
                return "UPDATE tbl_Accounts SET Email = '" + email + "', Password = '" + password + "', Logged_In = " + ((logged_In) ? "1" : "0") + " WHERE Account_ID = " + account_ID.ToString() + ";";
            }
            else
            {
                return "";
            }
        }
    }
    class _Activity : Record
    {
        private int activity_ID = 0;
        public int Activity_ID
        {
            get
            {
                return activity_ID;
            }
        }
        private int account_ID = 0;
        public int Account_ID
        {
            get
            {
                return account_ID;
            }
            set
            {
                if (account_ID != value)
                {
                    Changed = true;
                    account_ID = value;
                    SQL = CreateSQL();
                }
            }
        }
        private Activity activity_Type;
        public Activity Activity_Type
        {
            get
            {
                return activity_Type;
            }
            set
            {
                if (activity_Type != value)
                {
                    Changed = true;
                    activity_Type = value;
                    SQL = CreateSQL();
                }
            }
        }
        private DateTime dTStamp = default(DateTime);
        public DateTime DTStamp
        {
            get
            {
                return dTStamp;
            }
            set
            {
                if (dTStamp != value)
                {
                    Changed = true;
                    dTStamp = value;
                    SQL = CreateSQL();
                }
            }
        }
        private string session_ID = "";
        public string Session_ID
        {
            get
            {
                return session_ID;
            }
            set
            {
                if (session_ID != value)
                {
                    Changed = true;
                    session_ID = value;
                    SQL = CreateSQL();
                }
            }
        }

        public _Activity(int Activity_ID, int Account_ID, Activity Activity_Type, DateTime DTStamp, string Session_ID, bool _New)
        {
            activity_ID = Activity_ID;
            account_ID = Account_ID;
            activity_Type = Activity_Type;
            dTStamp = DTStamp;
            session_ID = Session_ID;
            New = _New;
            if (_New) SQL = CreateSQL();
        }
        private string CreateSQL()
        {
            if (New)
            {
                return "INSERT INTO tbl_Activity (Account_ID, Activity_Type, DTStamp, Session_ID) SELECT " + account_ID.ToString() + ", " + ((int)activity_Type).ToString() + ", '" + dTStamp.ToString("yyyy/MM/dd hh:mm:ss") + "', '" + session_ID + "';";
            }
            else
            {
                return "";
            }
        }
    }
    class _Characters : Record
    {
        private int character_ID = 0;
        public int Character_ID
        {
            get
            {
                return character_ID;
            }
        }
        private int account_ID = 0;
        public int Account_ID
        {
            get
            {
                return account_ID;
            }
        }
        private string character_Name = "";
        public string Character_Name
        {
            get
            {
                return character_Name;
            }
        }
        private int character_Level = 0;
        public int Character_Level
        {
            get
            {
                return character_Level;
            }
            set
            {
                if (character_Level != value)
                {
                    Changed = true;
                    character_Level = value;
                    SQL = CreateSQL();
                }
            }
        }
        private float pos_X = 0.0f;
        public float Pos_X
        {
            get
            {
                return pos_X;
            }
            set
            {
                if (pos_X != value)
                {
                    Changed = true;
                    pos_X = value;
                    SQL = CreateSQL();
                }
            }
        }
        private float pos_Y = 0.0f;
        public float Pos_Y
        {
            get
            {
                return pos_Y;
            }
            set
            {
                if (pos_Y != value)
                {
                    Changed = true;
                    pos_Y = value;
                    SQL = CreateSQL();
                }
            }
        }
        private float pos_Z = 0.0f;
        public float Pos_Z
        {
            get
            {
                return pos_Z;
            }
            set
            {
                if (pos_Z != value)
                {
                    Changed = true;
                    pos_Z = value;
                    SQL = CreateSQL();
                }
            }
        }
        private float rotation_Y = 0.0f;
        public float Rotation_Y
        {
            get
            {
                return rotation_Y;
            }
            set
            {
                if (rotation_Y != value)
                {
                    Changed = true;
                    rotation_Y = value;
                    SQL = CreateSQL();
                }
            }
        }
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
                    SQL = CreateSQL();
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
                    SQL = CreateSQL();
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
                    SQL = CreateSQL();
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
                    SQL = CreateSQL();
                }
            }
        }
        private int gender;
        public int Gender
        {
            get
            {
                return gender;
            }
        }
        private int health;
        public int Health
        {
            get
            {
                return health;
            }
        }
        private int strength;
        public int Strength
        {
            get
            {
                return strength;
            }
        }
        private int agility;
        public int Agility
        {
            get
            {
                return agility;
            }
        }

        public bool In_World = false;

        public _Characters(int Character_ID, int Account_ID, string Character_Name, int Character_Level, int Gender,
            float Pos_X, float Pos_Y, float Pos_Z, float Rotation_Y, 
            float Camera_Pos_X, float Camera_Pos_Y, float Camera_Pos_Z, float Camera_Rotation_Y,
            int Health, int Strength, int Agility)
        {
            character_ID = Character_ID;
            account_ID = Account_ID;
            character_Name = Character_Name;
            character_Level = Character_Level;
            gender = Gender;
            pos_X = Pos_X;
            pos_Y = Pos_Y;
            pos_Z = Pos_Z;
            rotation_Y = Rotation_Y;
            camera_Pos_X = Camera_Pos_X;
            camera_Pos_Y = Camera_Pos_Y;
            camera_Pos_Z = Camera_Pos_Z;
            camera_Rotation_Y = Camera_Rotation_Y;
            health = Health;
            strength = Strength;
            agility = Agility;
            New = false;
        }
        private string CreateSQL()
        {
            if (Changed)
            {
                return "UPDATE tbl_Characters SET Character_Level = " + character_Level.ToString() + 
                    ", Pos_X = " + pos_X.ToString() + ", Pos_Y = " + pos_Y.ToString() + ", Pos_Z = " + pos_Z.ToString() + ", Rotation_Y = " + rotation_Y.ToString() +
                    ", Camera_Pos_X = " + camera_Pos_X.ToString() + ", Camera_Pos_Y = " + camera_Pos_Y.ToString() + ", Camera_Pos_Z = " + camera_Pos_Z.ToString() + ", Camera_Rotation_Y = " + camera_Rotation_Y.ToString() +
                    " WHERE Account_ID = " + account_ID.ToString() + " AND Character_Name = '" + character_Name + "';";
            }
            else
            {
                return "";
            }
        }
    }
    class _Quests : Record
    {
        private int quest_ID;
        public int Quest_ID
        {
            get
            {
                return quest_ID;
            }
        }
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
        }
        private string start_Text;
        public string Start_Text
        {
            get
            {
                return start_Text;
            }
        }
        private string end_Text;
        public string End_Text
        {
            get
            {
                return end_Text;
            }
        }
        private int reward_ID;
        public int Reward_ID
        {
            get
            {
                return reward_ID;
            }
        }
        private int nPC_Start_ID;
        public int NPC_Start_ID
        {
            get
            {
                return nPC_Start_ID;
            }
        }
        private int nPC_End_ID;
        public int NPC_End_ID
        {
            get
            {
                return nPC_End_ID;

            }
        }
        private int objective_Target;
        public int Objective_Target
        {
            get
            {
                return objective_Target;
            }
        }
        private int start_Requirement_Quest_ID;
        public int Start_Requirement_Quest_ID
        {
            get
            {
                return start_Requirement_Quest_ID;
            }
        }
        private int item_Objective_ID;
        public int Item_Objective_ID
        {
            get
            {
                return item_Objective_ID;
            }
        }
        private int nPC_Objective_ID;
        public int NPC_Objective_ID
        {
            get
            {
                return nPC_Objective_ID;
            }
        }

        public _Quests(int Quest_ID, string Title, string Start_Text, string End_Text, int Reward_ID, 
            int NPC_Start_ID, int NPC_End_ID, int Objective_Target, int Start_Requirement_Quest_ID, 
            int Item_Objective_ID, int NPC_Objective_ID)
        {
            quest_ID = Quest_ID;
            title = Title;
            start_Text = Start_Text;
            end_Text = End_Text;
            reward_ID = Reward_ID;
            nPC_Start_ID = NPC_Start_ID;
            nPC_End_ID = NPC_End_ID;
            objective_Target = Objective_Target;
            start_Requirement_Quest_ID = Start_Requirement_Quest_ID;
            item_Objective_ID = Item_Objective_ID;
            nPC_Objective_ID = NPC_Objective_ID;

            New = false;
        }
    }
    class _Quest_Log : Record
    {
        private int log_ID;
        public int Log_ID
        {
            get
            {
                return log_ID;
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
        private int quest_ID;
        public int Quest_ID
        {
            get
            {
                return quest_ID;
            }
        }
        private int quest_Status;
        public int Quest_Status
        {
            get
            {
                return quest_Status;
            }
            set
            {
                if (quest_Status != value)
                {
                    Changed = true;
                    quest_Status = value;
                    SQL = CreateSQL();
                }
            }
        }
        private int progress;
        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                if (progress != value)
                {
                    Changed = true;
                    progress = value;
                    SQL = CreateSQL();
                }
            }
        }

        public _Quest_Log(int Log_ID, int Character_ID, int Quest_ID, int Quest_Status, int Progress, bool _New = false)
        {
            log_ID = Log_ID;
            character_ID = Character_ID;
            quest_ID = Quest_ID;
            quest_Status = Quest_Status;
            progress = Progress;
            if (_New)
            {
                log_ID = Database.instance.Insert_Record("INSERT INTO tbl_Quest_Log (Character_ID, Quest_ID, Quest_Status) SELECT " + character_ID + ", " + quest_ID + ", " + quest_Status + ";");
            }
            New = false;
        }

        private string CreateSQL()
        {
            if (Changed)
            {
                return "UPDATE tbl_Quest_Log SET Character_ID = " + character_ID + ", Quest_ID = " + quest_ID + ", Quest_Status = " + quest_Status + ", Progress = " + progress + " WHERE Log_ID = " + log_ID + ";";
            }
            else
            {
                return "";
            }
        }
    }
    class _NPC : Record
    {
        private int nPC_ID;
        public int NPC_ID
        {
            get
            {
                return nPC_ID;
            }
        }
        private int status;
        public int Status
        {
            get
            {
                return status;
            }
        }
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
        }
        private int level;
        public int Level
        {
            get
            {
                return level;
            }
        }
        private float pos_X;
        public float Pos_X
        {
            get
            {
                return pos_X;
            }
            set
            {
                if (pos_X != value)
                {
                    Changed = true;
                    pos_X = value;
                    SQL = CreateSQL();
                }
            }
        }
        private float pos_Y;
        public float Pos_Y
        {
            get
            {
                return pos_Y;
            }
            set
            {
                if (pos_Y != value)
                {
                    Changed = true;
                    pos_Y = value;
                    SQL = CreateSQL();
                }
            }
        }
        private float pos_Z;
        public float Pos_Z
        {
            get
            {
                return pos_Z;
            }
            set
            {
                if (pos_Z != value)
                {
                    Changed = true;
                    pos_Z = value;
                    SQL = CreateSQL();
                }
            }
        }
        private float rotation_Y;
        public float Rotation_Y
        {
            get
            {
                return rotation_Y;
            }
            set
            {
                if (rotation_Y != value)
                {
                    Changed = true;
                    rotation_Y = value;
                    SQL = CreateSQL();
                }
            }
        }
        private int hP;
        public int HP
        {
            get
            {
                return hP;
            }
        }
        private int gender;
        public int Gender
        {
            get
            {
                return gender;
            }
        }

        public _NPC(int NPC_ID, int Status, string Name, int Level, 
            float Pos_X, float Pos_Y, float Pos_Z, float Rotation_Y, 
            int HP, int Gender)
        {
            nPC_ID = NPC_ID;
            status = Status;
            name = Name;
            level = Level;
            pos_X = Pos_X;
            pos_Y = Pos_Y;
            pos_Z = Pos_Z;
            rotation_Y = Rotation_Y;
            hP = HP;
            gender = Gender;
            New = false;
        }

        private string CreateSQL()
        {
            if (Changed)
            {
                return "UPDATE tbl_NPC SET Pos_X = " + pos_X + ", Pos_Y = " + pos_Y + ", Pos_Z = " + pos_Z + ", Rotation_Y = " + rotation_Y + ", HP = " + hP + " WHERE NPC_ID = " + nPC_ID + ";";
            }
            else
            {
                return "";
            }
        }
    }
    class _Collectables
    {
        private int collectable_ID;
        public int Collectable_ID
        {
            get
            {
                return collectable_ID;
            }
        }
        private string collectable_Name;
        public string Collectable_Name
        {
            get
            {
                return collectable_Name;
            }
        }

        public _Collectables(int Collectable_ID, string Collectable_Name)
        {
            collectable_ID = Collectable_ID;
            collectable_Name = Collectable_Name;
        }
    }
}
