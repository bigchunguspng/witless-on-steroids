using System;

namespace Witlesss.Services.Technical
{
    public static class Logger
    {
        static Logger()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console. InputEncoding = System.Text.Encoding.Unicode;
        }

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