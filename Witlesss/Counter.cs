namespace Witlesss
{
    public class Counter
    {
        private int _interval;
        private int _counter;
        private int _temp;
        
        public Counter(int interval)
        {
            _interval = interval;
        }


        public void Count()
        {
            _counter ++;
            _counter %= _interval;
        }

        public bool Done() => _counter == 0;

        public void Stop()
        {
            _temp = _interval;
            _interval = int.MaxValue;
        }

        public void Resume()
        {
            _interval = _temp;
            _counter = 0;
        }
    }
}