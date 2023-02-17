using System;

namespace Witlesss
{
    public class Counter
    {
        private readonly int _min, _max;
        private int _interval, _counter;

        public Counter(int min = 1, int max = 62)
        {
            _min = min;
            _max = max;
        }

        public int Interval
        {
            get => _interval;
            set => _interval = Math.Clamp(value, _min, _max);
        }

        public void Count() => _counter = (_counter + 1) % _interval;

        public bool Ready() => _counter == 0;

        public void Reset() => _counter = 0;
    }
}