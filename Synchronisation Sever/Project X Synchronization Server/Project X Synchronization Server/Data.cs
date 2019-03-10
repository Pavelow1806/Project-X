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
        #region Data Send/Receive Information
        protected static ByteBuffer.ByteBuffer buffer = new ByteBuffer.ByteBuffer();
        public static byte[] data = null;
        protected static int Index = -1;
        #endregion

        public static void Reset()
        {
            buffer = new ByteBuffer.ByteBuffer();
            data = null;
            Index = -1;
        }

        #region Table Storage
        // Accounts table
        public static List<_Accounts> tbl_Accounts = new List<_Accounts>();

        //Activity table
        public static List<_Activity> tbl_Activity = new List<_Activity>();

        // Characters table
        public static List<_Characters> tbl_Characters = new List<_Characters>();
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
            foreach (_Accounts account in tbl_Accounts)
            {
                if (account.SQL != "")
                {
                    UpdateQueries.Add(account.SQL);
                    account.SQL = "";
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
            foreach (_Activity activity in tbl_Activity)
            {
                if (activity.SQL != "")
                {
                    UpdateQueries.Add(activity.SQL);
                    activity.SQL = "";
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
            foreach (_Characters character in tbl_Characters)
            {
                if (character.SQL != "")
                {
                    UpdateQueries.Add(character.SQL);
                    character.SQL = "";
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
            New = true;
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

        public _Characters(int Character_ID, int Account_ID, string Character_Name, int Character_Level, float Pos_X, float Pos_Y, float Pos_Z)
        {
            character_ID = Character_ID;
            account_ID = Account_ID;
            character_Name = Character_Name;
            character_Level = Character_Level;
            pos_X = Pos_X;
            pos_Y = Pos_Y;
            pos_Z = Pos_Z;
            New = false;
        }
        private string CreateSQL()
        {
            if (Changed)
            {
                return "UPDATE tbl_Characters SET Character_Level = " + character_Level.ToString() + ", Pos_X = " + pos_X.ToString() + ", Pos_Y = " + pos_Y.ToString() + ", Pos_Z = " + pos_Z.ToString() +
                    " WHERE Account_ID = " + account_ID.ToString() + " AND Character_Name = '" + character_Name + "';";
            }
            else
            {
                return "";
            }
        }
    }
}
