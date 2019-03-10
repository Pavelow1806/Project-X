using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Login_Server
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
        string ConnectionString = "Server=projectx.cqekxmwdej63.us-east-2.rds.amazonaws.com;Port=3306;Database=Pavelow;Uid=Pavelow;Pwd=asdfgh147856;";

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
                    if (reader != null)
                    {
                        reader.Close();
                        reader = null;
                    }
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
        }

        public string RequestAuthenticationCode()
        {
            MySqlDataReader reader = QueryDatabase("SELECT Authentication_Code FROM tbl_Authentication;");
            if (reader.Read())
            {
                string AuthenticationCode = reader["Authentication_Code"].ToString();
                reader.Close();
                reader = null;
                return AuthenticationCode;
            }
            else
            {
                reader.Close();
                reader = null;
                return "";
            }
        }

        public Response Login(string username, string password)
        {
            reader = QueryDatabase("CALL CheckLoginDetails('" + username + "', '" + password + "');");
            if (reader != null)
            {
                if (reader.Read())
                {
                    reader.Close();
                    reader = null;
                    return Response.SUCCESSFUL;
                }
                else
                {
                    reader.Close();
                    reader = null;
                    return Response.UNSUCCESSFUL;
                }
            }
            else
            {
                return Response.ERROR;
            }
        }

        public Response RequestRegistration(string username, string password, string email, out string response)
        {
            reader = QueryDatabase("CALL RegisterAccount('" + username + "', '" + password + "', '" + email + "');");
            if (reader != null)
            {
                if (reader.Read())
                {
                    response = reader["Result"].ToString();
                    reader.Close();
                    reader = null;
                    return Response.SUCCESSFUL;
                }
                else
                {
                    response = "An error occured when attempting to register.";
                    reader.Close();
                    reader = null;
                    return Response.UNSUCCESSFUL;
                }
            }
            else
            {
                response = "An error occured when attempting to register.";
                return Response.ERROR;
            }
        }

        public Response GetCharacters(string username, int index)
        {
            MySqlDataReader reader = QueryDatabase("CALL GetCharacterList('" + username + "');");

            if (reader != null)
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Network.instance.Clients[index].Characters.Add
                        (
                            new Character
                            (
                                reader["Character_Name"].ToString(), 
                                Convert.ToInt32(reader["Character_Level"])
                            )
                        );
                    }
                    reader.Close();
                    reader = null;
                    return Response.SUCCESSFUL;
                }
                else
                {
                    reader.Close();
                    reader = null;
                    return Response.UNSUCCESSFUL;
                }
            }
            else
            {
                return Response.ERROR;
            }
        }

        public Response LogActivity(string username, Activity activity, string sessionid)
        {
            MySqlDataReader reader = QueryDatabase(@"CALL LogAccountActivity(""" + username + @""", " + (int)activity + @", """ + sessionid + @""", 1);");

            if (reader != null)
            {
                if (reader.Read())
                {
                    reader.Close();
                    reader = null;
                    return Response.SUCCESSFUL;
                }
                else
                {
                    reader.Close();
                    reader = null;
                    return Response.UNSUCCESSFUL;
                }
            }
            else
            {
                return Response.ERROR;
            }
        }
    }
}
