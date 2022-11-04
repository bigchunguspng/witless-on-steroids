using System;

namespace Witlesss
{
    public static class Logger
    {
        static Logger() => Console.OutputEncoding = System.Text.Encoding.UTF8;

        public static void Log(string message) => Console.WriteLine(message);

        public static void Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Log(message);
            Console.ResetColor();
        }

        public static void LogError(string message) => Log(message, ConsoleColor.Red);
    }
}