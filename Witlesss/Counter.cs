using System;

namespace Witlesss
{
    public class Counter
    {
        private readonly int _min, _max;
        private int _declared, _actual, _counter;

        public Counter(int min = 1, int max = 62)
        {
            _min = min;
            _max = max;
        }

        public int Interval
        {
            get => _declared;
            set
            {
                _declared = Math.Clamp(value, _min, _max);
                _actual = _declared;
            }
        }

        public void Count()
        {
            _counter ++;
            _counter %= _actual;
        }

        public bool Ready()  => _counter == 0;

        public void Stop()   => _actual = int.MaxValue;

        public void Resume() => _actual = _declared;

        public void Reset() => _counter = 0;
    }
}