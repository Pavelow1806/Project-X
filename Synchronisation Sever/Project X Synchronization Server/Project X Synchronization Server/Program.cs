﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Project_X_Synchronization_Server
{
    class Program
    {
        private static Thread consoleThread;
        private static bool consoleRunning;

        private static Database db = new Database();
        private static Network nw = new Network();
        private static Data cache = new Data();

        static void Main(string[] args)
        {
            Console.Title = "Project X - Synchronization Server";
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();

            Log.log("", Log.LogType.START);
            Log.log("", Log.LogType.START);
            Log.log("", Log.LogType.START);
            Log.log("                            Project X |", Log.LogType.START);
            Log.log("               Synchronization Server |", Log.LogType.START);
            Log.log("                                 v1.0 |", Log.LogType.START);
            Log.log("", Log.LogType.START);
            Log.log("                         Starting server..", Log.LogType.START);
            Log.log("", Log.LogType.START);

            if (!nw.LaunchServer())
            {
                Log.log("Server failed to launch, type launch to attempt to relaunch the server.", Log.LogType.SYSTEM);
            }
            else
            {
                Log.log("Server started.", Log.LogType.SYSTEM);
            }

            consoleThread = new Thread(new ThreadStart(ConsoleLoop));
            consoleThread.Start();
        }

        private static void ConsoleLoop()
        {
            string line;
            consoleRunning = true;

            while (consoleRunning)
            {
                line = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(line))
                {
                    Log.WriteUser();
                }
                else if (line == "?")
                {
                    // Output all commands available
                    Log.log("List of available commands:", Log.LogType.HELP);
                    Log.log("", Log.LogType.HELP);
                    Log.log("?", Log.LogType.HELP);
                    Log.log("  Displays all available commands.", Log.LogType.HELP);
                    Log.log("launch", Log.LogType.HELP);
                    Log.log("  Attempts the launch the server.", Log.LogType.HELP);
                    Log.log("initialise", Log.LogType.HELP);
                    Log.log("  Initialises the locally cached version of the database. [WARNING: This will overwrite all current data]", Log.LogType.HELP);
                    Log.log("sync", Log.LogType.HELP);
                    Log.log("  Calls on the synchronization of database data.", Log.LogType.HELP);
                }
                else if (line.ToLower() == "launch")
                {
                    if (!Network.Running)
                    {
                        Log.log("Launching server..", Log.LogType.SYSTEM);

                        if (!nw.LaunchServer())
                        {
                            Log.log("Server failed to launch, type launch to attempt to relaunch the server.", Log.LogType.SYSTEM);
                        }
                    }
                    else
                    {
                        Log.log("Server already running.", Log.LogType.WARNING);
                    }
                }
                else if (line.ToLower() == "initialise")
                {
                    Data.Initialise();
                }
                else if (line.ToLower() == "sync")
                {
                    SynchronizationScheduler.instance.SynchronizeNow();
                }
                else
                {
                    Log.WriteUser();
                }
            }
        }
    }
}
