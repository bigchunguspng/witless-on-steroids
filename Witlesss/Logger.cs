using System;

namespace Witlesss
{
    public static class Logger
    {
        public static void Log(string message) => Console.WriteLine(message);

        public static void Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Log(message);
            Console.ResetColor();
        }
    }
}