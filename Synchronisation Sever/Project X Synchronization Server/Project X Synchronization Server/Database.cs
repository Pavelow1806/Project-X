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

        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        MySqlConnection Connection = null;
        MySqlCommand Command = null;
        MySqlDataReader reader = null;
        MySqlTransaction Transaction = null;
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
        private Response BulkUpdate(List<string> queries, int LineNumber)
        {
            lock (lockObj)
            {
                Transaction = Connection.BeginTransaction();

                try
                {
                    int QueryNumber = 0;
                    foreach (string query in queries)
                    {
                        Command = new MySqlCommand(query, Connection, Transaction);
                        Command.ExecuteNonQuery();
                        ++QueryNumber;
                        Log.log(LineNumber, "Synchronizing.. processed query " + QueryNumber.ToString() + "/" + queries.Count.ToString(), Log.LogType.SYNC);
                    }
                    Transaction.Commit();
                    return Response.SUCCESSFUL;
                }
                catch (MySqlException sqle)
                {
                    // Output error
                    Log.log("An SQL error when attempting to bulk update. > " + sqle.Message + ", Rolling back the commit.", Log.LogType.ERROR);
                    Transaction.Rollback();
                    return Response.ERROR;
                }
                catch (Exception e)
                {
                    // Output error
                    Log.log("A generic error occured when attempting to bulk update. > " + e.Message + ", Rolling back the commit.", Log.LogType.ERROR);
                    Transaction.Rollback();
                    return Response.ERROR;
                }
            }
        }
        private int Count(string query)
        {
            lock (lockObj)
            {
                try
                {
                    Command = new MySqlCommand(query, Connection);
                    reader = Command.ExecuteReader();
                    int count = -1;
                    if (reader.Read())
                    {
                        count = reader.GetInt32(0);
                        reader.Close();
                    }
                    else
                    {
                        Log.log("An error occurred when attempting to get count using query: " + query + " Returning default value of -1.", Log.LogType.ERROR);
                    }
                    return count;
                }
                catch (MySqlException sqle)
                {
                    // Output error
                    Log.log("An SQL error occured when attempting to query the database. > " + sqle.Message, Log.LogType.ERROR);
                    return -1;
                }
                catch (Exception e)
                {
                    // Output error
                    Log.log("An generic error occured when attempting to query the database. > " + e.Message, Log.LogType.ERROR);
                    return -1;
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

        public int RequestSynchronizationTime()
        {
            reader = QueryDatabase("SELECT SecondsBetweenSynchronizations FROM tbl_Synchronization_Settings;");
            if (reader != null)
            {
                if (reader.Read())
                {
                    int Seconds = reader.GetInt32("SecondsBetweenSynchronizations");
                    reader.Close();
                    return Seconds;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }
        
        public Response Load(int LineNumber)
        {
            try
            {
                int RecordCount = 0;
                int RecordNumber;
                int SubLineNumber;

                // tbl_Accounts
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_Accounts into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) FROM tbl_Accounts;");
                reader = QueryDatabase("SELECT * FROM tbl_Accounts;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_Accounts.Add(reader.GetInt32("Account_ID"), new _Accounts(reader.GetInt32("Account_ID"), reader.GetString("Username"), reader.GetString("Email"), reader.GetString("Password"), reader.GetBoolean("Logged_In")));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_Accounts, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_Accounts, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                // tbl_Activity
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_Activity into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) from tbl_Activity;");
                reader = QueryDatabase("SELECT * FROM tbl_Activity;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_Activity.Add(reader.GetInt32("Activity_ID"), new _Activity(reader.GetInt32("Activity_ID"), reader.GetInt32("Account_ID"), (Activity)reader.GetInt32("Activity_Type"), reader.GetDateTime("DTStamp"), reader.GetString("Session_ID"), false));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_Activity, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_Activity, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                // tbl_Characters
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_Characters into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) from tbl_Characters;");
                reader = QueryDatabase("SELECT * FROM tbl_Characters;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_Characters.Add(reader.GetInt32("Character_ID"), new _Characters(reader.GetInt32("Character_ID"), reader.GetInt32("Account_ID"), reader.GetString("Character_Name"), reader.GetInt32("Character_Level"), reader.GetInt32("Gender"),
                        reader.GetFloat("Pos_X"), reader.GetFloat("Pos_Y"), reader.GetFloat("Pos_Z"), reader.GetFloat("Rotation_Y"),
                        reader.GetFloat("Camera_Pos_X"), reader.GetFloat("Camera_Pos_Y"), reader.GetFloat("Camera_Pos_Z"), reader.GetFloat("Camera_Rotation_Y"), 
                        reader.GetInt32("Health"), reader.GetInt32("Strength"), reader.GetInt32("Agility"), reader.GetInt32("Experience")));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_Characters, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_Characters, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                // tbl_Quests
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_Quests into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) from tbl_Quests;");
                reader = QueryDatabase("SELECT * FROM tbl_Quests;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_Quests.Add(reader.GetInt32("Quest_ID"), new _Quests(reader.GetInt32("Quest_ID"), reader.GetString("Title"), reader.GetString("Start_Text"), reader.GetString("End_Text"),
                        reader.GetInt32("Reward_ID"), reader.GetInt32("NPC_Start_ID"), reader.GetInt32("NPC_End_ID"), reader.GetInt32("Objective_Target"), reader.GetInt32("Start_Requirement_Quest_ID"),
                        reader.GetInt32("Item_Objective_ID"), reader.GetInt32("NPC_Objective_ID")));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_Quests, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_Quests, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                // tbl_Quest_Log
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_Quest_Log into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) from tbl_Quest_Log;");
                reader = QueryDatabase("SELECT * FROM tbl_Quest_Log;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_Quest_Log.Add(reader.GetInt32("Log_ID"), new _Quest_Log(reader.GetInt32("Log_ID"), reader.GetInt32("Character_ID"), 
                        reader.GetInt32("Quest_ID"), reader.GetInt32("Quest_Status"), reader.GetInt32("Progress")));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_Quest_Log, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_Quest_Log, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                // tbl_NPC
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_NPC into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) from tbl_NPC;");
                reader = QueryDatabase("SELECT * FROM tbl_NPC;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_NPC.Add(reader.GetInt32("NPC_ID"), new _NPC(reader.GetInt32("NPC_ID"), reader.GetInt32("Status"), reader.GetString("Name"), reader.GetInt32("Level"),
                        reader.GetInt32("HP"), reader.GetInt32("Gender"), reader.GetInt32("Respawn_Time")));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_NPC, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_NPC, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                // tbl_Collectables
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_Collectables into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) from tbl_Collectables;");
                reader = QueryDatabase("SELECT * FROM tbl_Collectables;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_Collectables.Add(reader.GetInt32("Collectable_ID"), new _Collectables(reader.GetInt32("Collectable_ID"), reader.GetString("Collectable_Name"),
                        reader.GetInt32("Respawn_Time")));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_Collectables, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_Collectables, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                // tbl_Spawn_Positions
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_Spawn_Positions into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) from tbl_Spawn_Positions;");
                reader = QueryDatabase("SELECT * FROM tbl_Spawn_Positions;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_Spawn_Positions.Add(reader.GetInt32("Position_ID"), new _Spawn_Positions(reader.GetInt32("Position_ID"),
                        reader.GetFloat("Pos_X"), reader.GetFloat("Pos_Y"), reader.GetFloat("Pos_Z"), reader.GetFloat("Rotation_Y"),
                        reader.GetInt32("NPC_ID"), reader.GetInt32("Collectable_ID")));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_Spawn_Positions, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_Spawn_Positions, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                // tbl_Experience
                Log.log(LineNumber, "Performing initial synchronization of database.. Loading tbl_Experience into Cache..", Log.LogType.CACHE);
                RecordCount = Count("SELECT COUNT(*) from tbl_Experience;");
                reader = QueryDatabase("SELECT * FROM tbl_Experience;");
                RecordNumber = 0;
                SubLineNumber = -1;
                while (reader.Read())
                {
                    Data.tbl_Experience.Add(reader.GetInt32("XP_ID"), new _Experience(reader.GetInt32("XP_ID"), reader.GetInt32("Level"), reader.GetInt32("Experience"),
                        reader.GetInt32("Strength"), reader.GetInt32("Agility"), reader.GetInt32("HP")));
                    ++RecordNumber;
                    if (SubLineNumber == -1)
                    {
                        SubLineNumber = Log.log("Downloading data from tbl_Experience, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                    else
                    {
                        Log.log(SubLineNumber, "Downloading data from tbl_Experience, Record: " + RecordNumber.ToString() + " of " + RecordCount.ToString() + " (" +
                            ((RecordNumber == 0 || RecordCount == 0) ? "0.00%" : ((RecordCount / RecordNumber) * 100).ToString("0.00") + "%") + ")", Log.LogType.CACHE);
                    }
                }
                reader.Close();

                Log.log(LineNumber, "Initial synchronization of database successful, see below for a data summary report.", Log.LogType.CACHE);
                return Response.SUCCESSFUL;
            }
            catch (Exception e)
            {
                Log.log("An error occurred when attempting to load data from the database. > " + e.Message, Log.LogType.ERROR);
                return Response.ERROR;
            }
        }

        public Response Synchronize(int LineNumber)
        {
            List<string> Queries = Data.BuildQueryList(LineNumber);
            Log.log(LineNumber, "Starting synchronization of data.. Found " + Queries.Count.ToString() + " queries ready for updating.", Log.LogType.SYNC);
            return BulkUpdate(Queries, LineNumber);
        }

        public int Insert_Record(string query)
        {
            lock (lockObj)
            {
                reader = QueryDatabase(query);
                if (reader != null)
                {
                    if (reader.Read())
                    {
                        int ID = Convert.ToInt32(reader[0]);
                        reader.Close();
                        return ID;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}
