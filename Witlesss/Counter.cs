namespace Witlesss
{
    public class Counter
    {
        private int _interval;
        private int _counter;
        private int _temp;

        private readonly int _min, _max;
        
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
            }
        }


        public void Count()
        {
            _counter ++;
            _counter %= Interval;
        }

        public bool Ready() => _counter == 0;

        public void Stop()
        {
            _temp = Interval;
            _interval = int.MaxValue; //ага, в обход сеттера
        }

        public void Resume()
        {
            Interval = _temp;
            _counter = 0;
        }
    }
}