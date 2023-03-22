using System;

namespace Witlesss.X
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
    
    public class StopWatch
    {
        private DateTime _time;

        public StopWatch() => WriteTime();

        public void Log(string message)
        {
            Logger.Log($@"{CheckStopWatch()} {message}");
            WriteTime();
        }

        public void   WriteTime() => _time  = DateTime.Now;
        public string CheckStopWatch()
        {
            var time = DateTime.Now - _time;
            return time.Minutes > 1 ? $"{time:m' MINS'}" : $@"{time:s\.fff's'}";
        }
    }
}