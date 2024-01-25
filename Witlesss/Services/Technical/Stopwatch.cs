using System;

namespace Witlesss.Services.Technical
{
    public class Stopwatch
    {
        private DateTime _time;

        public Stopwatch() => WriteTime();

        public void Log(string message)
        {
            Logger.Log($@"{CheckElapsed()} {message}");
            WriteTime();
        }

        public void      WriteTime() => _time = DateTime.Now;
        public TimeSpan GetElapsed() => DateTime.Now - _time;
        public string CheckElapsed()
        {
            var time = GetElapsed();
            return time.Minutes > 1 ? $"{time:m' MINS'}" : $@"{time:s\.fff's'}";
        }
    }
}