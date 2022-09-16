using System;

namespace Witlesss
{
    public class Counter
    {
        private readonly int _min, _max;
        private int _declaredInterval, _actualInterval, _counter;

        public Counter(int min = 1, int max = 62)
        {
            _min = min;
            _max = max;
        }

        public int Interval
        {
            get => _declaredInterval;
            set
            {
                _declaredInterval = Math.Clamp(value, _min, _max);
                _actualInterval = _declaredInterval;
            }
        }


        public void Count()
        {
            _counter ++;
            _counter %= _actualInterval;
        }

        public bool Ready()  => _counter == 0;

        public void Stop()   => _actualInterval = int.MaxValue;

        public void Resume() => _actualInterval = Interval;
    }
}