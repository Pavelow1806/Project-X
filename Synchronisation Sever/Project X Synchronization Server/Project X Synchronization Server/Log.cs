using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Synchronization_Server
{
    class Log
    {
        public enum LogType
        {
            ERROR,
            WARNING,
            SENT,
            RECEIVED,
            CONNECTION,
            SYSTEM,
            SUCCESS,
            GENERAL
        }
        #region Locking
        private static readonly object lockObj = new object();
        #endregion
        private const ConsoleColor DefaultColour = ConsoleColor.Black;

        private const ConsoleColor ErrorColour = ConsoleColor.Red;
        private const ConsoleColor WarningColour = ConsoleColor.DarkRed;
        private const ConsoleColor SentColour = ConsoleColor.Cyan;
        private const ConsoleColor ReceivedColour = ConsoleColor.DarkMagenta;
        private const ConsoleColor ConnectionColour = ConsoleColor.DarkYellow;
        private const ConsoleColor SystemColour = ConsoleColor.Gray;
        private const ConsoleColor SuccessColour = ConsoleColor.DarkGreen;

        public static void log(string Message, LogType type = LogType.GENERAL)
        {
            lock (lockObj)
            {
                if (type != LogType.GENERAL)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
                Console.Write(DateTime.Now.ToString() + " : ");
                switch (type)
                {
                    case LogType.ERROR:
                        Console.ForegroundColor = ErrorColour;
                        break;
                    case LogType.WARNING:
                        Console.ForegroundColor = WarningColour;
                        break;
                    case LogType.SENT:
                        Console.ForegroundColor = SentColour;
                        break;
                    case LogType.RECEIVED:
                        Console.ForegroundColor = ReceivedColour;
                        break;
                    case LogType.CONNECTION:
                        Console.ForegroundColor = ConnectionColour;
                        break;
                    case LogType.SYSTEM:
                        Console.ForegroundColor = SystemColour;
                        break;
                    case LogType.SUCCESS:
                        Console.ForegroundColor = SuccessColour;
                        break;
                }
                Console.WriteLine(Message);
                Console.ForegroundColor = DefaultColour;
                Console.Write(DateTime.Now.ToString() + " : Admin>");
            }
        }
    }
}
