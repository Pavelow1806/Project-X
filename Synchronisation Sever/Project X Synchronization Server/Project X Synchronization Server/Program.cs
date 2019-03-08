using System;
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

        static void Main(string[] args)
        {
            Console.Title = "Project X - Synchronization Server";
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("                            Project X |");
            Console.WriteLine("               Synchronization Server |");
            Console.WriteLine("                                 v1.0 |");
            Console.WriteLine();
            Console.WriteLine("                         Starting server..");
            Console.WriteLine();

            if (!nw.LaunchServer())
            {
                Log.log("Server failed to launch, type launch to attempt to relaunch the server.", Log.LogType.SYSTEM);
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

                }
                else if (line == "?")
                {
                    // Output all commands available
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
            }
        }
    }
}
