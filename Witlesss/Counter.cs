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
                if (value < _min)
                    _interval = _min;
                else if (value > _max)
                    _interval = _max;
                else
                    _interval = value;
                
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