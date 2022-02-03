using System;

namespace Witlesss
{
    public class Counter
    {
        private readonly int _min, _max;
        private int _interval, _actualInterval, _counter;

        public Counter(int interval, int min = 1, int max = 62)
        {
            _min = min;
            _max = max;
            
            Interval = interval;
        }

        public int Interval
        {
            get => _interval;
            set
            {
                _interval = Math.Clamp(value, _min, _max);
                _actualInterval = _interval;
            }
        }


        public void Count()
        {
            _counter ++;
            _counter %= _actualInterval;
        }

        public bool Ready() => _counter == 0;

        public void Stop() => _actualInterval = int.MaxValue;

        public void Resume() => _actualInterval = Interval;
    }
}