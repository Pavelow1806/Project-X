using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Synchronization_Server
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
        ACCOUNTCREATED
    }
    class Database
    {
        public static Database instance;

        MySqlConnection Connection = null;
        MySqlCommand Command = null;
        MySqlDataReader reader = null;
        string ConnectionString = "Server=projectx.cqekxmwdej63.us-east-2.rds.amazonaws.com;Port=3306;Database=Pavelow;Uid=Pavelow;Pwd=asdfgh147856;";

        public Database()
        {
            instance = this;
            Connection = new MySqlConnection(ConnectionString);
            Connection.Open();
        }

        private MySqlDataReader QueryDatabase(string query)
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
                Log.log("An generic error occured when attempting to query the database. > " + e.Message, Log.LogType.ERROR);
                return null;
            }
        }

        public string RequestAuthenticationCode()
        {
            MySqlDataReader reader = QueryDatabase("SELECT Authentication_Code FROM tbl_Authentication;");

            if (reader.Read())
            {
                return reader["Authentication_Code"].ToString();
            }
            else
            {
                return "";
            }
        }
        
        public Response Load()
        {
            try
            {
                reader = QueryDatabase("SELECT * FROM tbl_Accounts;");
                while (reader.Read())
                {
                    Data.tbl_Accounts.Add(new _Accounts(reader.GetInt32("Account_ID"), reader.GetString("Username"), reader.GetString("Email"), reader.GetString("Password"), reader.GetBoolean("Logged_In")));
                }
                reader = QueryDatabase("SELECT * FROM tbl_Activity;");
                while (reader.Read())
                {
                    Data.tbl_Activity.Add(new _Activity(reader.GetInt32("Activity_ID"), reader.GetInt32("Account_ID"), reader.GetInt32("Activity_Type"), reader.GetDateTime("DTStamp"), reader.GetString("Session_ID")));
                }
                reader = QueryDatabase("SELECT * FROM tbl_Characters;");
                while (reader.Read())
                {
                    Data.tbl_Characters.Add(new _Characters(reader.GetInt32("Character_ID"), reader.GetInt32("Account_ID"), reader.GetString("Character_Name"), reader.GetInt32("Character_Level"),
                    reader.GetFloat("Pos_X"), reader.GetFloat("Pos_Y"), reader.GetFloat("Pos_Z")));
                }
                return Response.SUCCESSFUL;
            }
            catch (Exception e)
            {
                Log.log("An error occurred when attempting to load data from the database. > " + e.Message, Log.LogType.ERROR);
                return Response.ERROR;
            }
        }
    }
}
