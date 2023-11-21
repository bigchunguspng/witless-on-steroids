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

        public void   WriteTime() => _time  = DateTime.Now;
        public string CheckElapsed()
        {
            var time = DateTime.Now - _time;
            return time.Minutes > 1 ? $"{time:m' MINS'}" : $@"{time:s\.fff's'}";
        }
    }
}