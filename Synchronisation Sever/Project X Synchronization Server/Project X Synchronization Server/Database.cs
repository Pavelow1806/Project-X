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
            // Get database tables

            // For each table
            // Get tables fields and data types

            // Load data
        }
    }
}
