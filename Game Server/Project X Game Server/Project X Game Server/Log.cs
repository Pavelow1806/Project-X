using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_X_Game_Server
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
            GENERAL,
            START,
            HELP,
            CACHE,
            SYNC
        }
        #region Locking
        private static readonly object lockObj = new object();
        #endregion
        private const ConsoleColor DefaultColour = ConsoleColor.Black;

        private const ConsoleColor ErrorColour = ConsoleColor.Red;
        private const ConsoleColor WarningColour = ConsoleColor.DarkRed;
        private const ConsoleColor SentColour = ConsoleColor.DarkCyan;
        private const ConsoleColor ReceivedColour = ConsoleColor.Magenta;
        private const ConsoleColor ConnectionColour = ConsoleColor.DarkYellow;
        private const ConsoleColor SystemColour = ConsoleColor.DarkGray;
        private const ConsoleColor SuccessColour = ConsoleColor.DarkGreen;
        private const ConsoleColor StartColour = ConsoleColor.Black;
        private const ConsoleColor HelpColour = ConsoleColor.DarkYellow;
        private const ConsoleColor CacheColour = ConsoleColor.DarkBlue;
        private const ConsoleColor SyncColour = ConsoleColor.Blue;

        public static int log(string Message, LogType type = LogType.GENERAL)
        {
            lock (lockObj)
            {
                if (type != LogType.GENERAL)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
                if (type != LogType.START)
                {
                    Console.Write("     " + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss") + " : ");
                }
                Console.ForegroundColor = SetColour(type);
                int currentLine = Console.CursorTop;
                Console.WriteLine(Message);
                Console.ForegroundColor = DefaultColour;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("     " + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss") + " : Admin>");
                return currentLine;
            }
        }
        public static void log(int LineNumber, string Message, LogType type = LogType.GENERAL)
        {
            lock (lockObj)
            {
                int x = Console.CursorLeft;
                int y = Console.CursorTop;
                if (type != LogType.GENERAL)
                {
                    Console.SetCursorPosition(0, LineNumber);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, LineNumber);
                }
                if (type != LogType.START)
                {
                    Console.Write("     " + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss") + " : ");
                }
                Console.ForegroundColor = SetColour(type);
                Console.WriteLine(Message);
                Console.ForegroundColor = DefaultColour;
                Console.SetCursorPosition(x, y);
            }
        }
        public static void WriteUser()
        {
            Console.Write("     " + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss") + " : Admin>");
        }
        private static ConsoleColor SetColour(LogType type)
        {
            switch (type)
            {
                case LogType.ERROR:
                    return ErrorColour;
                case LogType.WARNING:
                    return WarningColour;
                case LogType.SENT:
                    return SentColour;
                case LogType.RECEIVED:
                    return ReceivedColour;
                case LogType.CONNECTION:
                    return ConnectionColour;
                case LogType.SYSTEM:
                    return SystemColour;
                case LogType.SUCCESS:
                    return SuccessColour;
                case LogType.START:
                    return StartColour;
                case LogType.HELP:
                    return HelpColour;
                case LogType.CACHE:
                    return CacheColour;
                case LogType.SYNC:
                    return SyncColour;
                default:
                    return DefaultColour;
            }
        }
    }
}
