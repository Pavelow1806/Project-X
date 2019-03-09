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
        public void DownloadCache()
        {
            Database.instance.Load();
        }

        // Accounts table
        public static List<_Accounts> tbl_Accounts = new List<_Accounts>();

        //Activity table
        public static List<_Activity> tbl_Activity = new List<_Activity>();

        // Characters table
        public static List<_Characters> tbl_Characters = new List<_Characters>();
        #endregion
    }
    class _Accounts
    {
        public int Account_ID = 0;
        public string Username = "";
        public string Email = "";
        public string Password = "";
        public bool Logged_In = false;
        public _Accounts(int account_ID, string username, string email, string password, bool logged_In)
        {
            Account_ID = account_ID;
            Username = username;
            Email = email;
            Password = password;
            Logged_In = logged_In;
        }
    }
    class _Activity
    {
        public int Activity_ID = 0;
        public int Account_ID = 0;
        public int Activity_Type = 0;
        public DateTime DTStamp = default(DateTime);
        public string Session_ID = "";
        public _Activity(int activity_ID, int account_ID, int activity_Type, DateTime dTStamp, string session_ID)
        {
            Activity_ID = activity_ID;
            Account_ID = account_ID;
            Activity_Type = activity_Type;
            DTStamp = dTStamp;
            Session_ID = session_ID;
        }
    }
    class _Characters
    {
        public int Character_ID = 0;
        public int Account_ID = 0;
        public string Character_Name = "";
        public int Character_Level = 0;
        public float Pos_X = 0.0f;
        public float Pos_Y = 0.0f;
        public float Pos_z = 0.0f;
        public _Characters(int character_ID, int account_ID, string character_Name, int character_Level, float pos_X, float pos_Y, float pos_Z)
        {
            Character_ID = character_ID;
            Account_ID = account_ID;
            Character_Name = character_Name;
            Character_Level = character_Level;
            Pos_X = pos_X;
            Pos_Y = pos_Y;
            Pos_z = pos_Z;
        }
    }
}
