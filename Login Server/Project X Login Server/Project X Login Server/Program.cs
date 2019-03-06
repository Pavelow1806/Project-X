using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Project_X_Login_Server
{
    class Program
    {
        private static Thread consoleThread;
        private static bool consoleRunning;

        private static Database db = new Database();
        private static Network nw = new Network();

        static void Main(string[] args)
        {
            Console.Title = "Project X - Login Server";

            nw.LaunchServer();

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
            }
        }
    }
}
