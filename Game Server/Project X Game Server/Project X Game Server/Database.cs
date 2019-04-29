using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
{
    public enum Response
    {
        SUCCESSFUL,
        UNSUCCESSFUL,
        ERROR
    }
    public enum Activity
    {
        LOGIN,
        LOGOUT,
        DISCONNECT,
        KICK,
        ACCOUNTCREATED,
        AUTHENTICATED
    }
    class Database
    {
        public static Database instance;

        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        MySqlConnection Connection = null;
        MySqlCommand Command = null;
        MySqlDataReader reader = null;
        MySqlTransaction Transaction = null;
        string ConnectionString = "Server=projectxmain.cviytaoeskbp.eu-west-2.rds.amazonaws.com; Port=3306;Database=Pavelow;Uid=Pavelow;Pwd=asdfgh147856;";

        public Database()
        {
            instance = this;
            Connection = new MySqlConnection(ConnectionString);
            Connection.Open();
        }

        private MySqlDataReader QueryDatabase(string query)
        {
            lock (lockObj)
            {
                try
                {
                    Command = new MySqlCommand(query, Connection);
                    return Command.ExecuteReader();
                }
                catch (MySqlException sqle)
                {
                    // Output error
                    Log.log("An SQL error occured when attempting to query the database. > " + sqle.Message, Log.LogType.ERROR);
                    return null;
                }
                catch (Exception e)
                {
                    // Output error
                    Log.log("A generic error occured when attempting to query the database. > " + e.Message, Log.LogType.ERROR);
                    return null;
                }
            }
        }

        public string RequestAuthenticationCode()
        {
            reader = QueryDatabase("SELECT Authentication_Code FROM tbl_Authentication;");
            if (reader != null)
            {
                if (reader.Read())
                {
                    string Authentication_Code = reader["Authentication_Code"].ToString();
                    reader.Close();
                    return Authentication_Code;
                }
                else
                {
                    return "";
                }

            }
            else
            {
                return "";
            }
        }
    }
}
