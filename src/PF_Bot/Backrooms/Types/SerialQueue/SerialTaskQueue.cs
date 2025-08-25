using static System.Threading.Tasks.TaskContinuationOptions;

namespace PF_Bot.Backrooms.Types.SerialQueue
{
    /// <summary>
    /// Sauce: https://github.com/gentlee/SerialQueue
    /// </summary>
    public class SerialTaskQueue
    {
        private readonly object _lock = new();
        private Task? _lastTask;

        public Task Enqueue(Action action)
        {
            lock (_lock)
            {
                _lastTask = _lastTask is null
                    ? Task.Run(action)
                    : _lastTask.ContinueWith(_ => action(), ExecuteSynchronously);

                return _lastTask;
            }
        }

        public Task<T> Enqueue<T>(Func<T> function)
        {
            lock (_lock)
            {
                _lastTask = _lastTask is null
                    ? Task.Run(function)
                    : _lastTask.ContinueWith(_ => function(), ExecuteSynchronously);

                return (Task<T>)_lastTask;
            }
        }

        public Task Enqueue(Func<Task> asyncAction)
        {
            lock (_lock)
            {
                _lastTask = _lastTask is null
                    ? Task.Run(asyncAction)
                    : _lastTask.ContinueWith(_ => asyncAction(), ExecuteSynchronously).Unwrap();

                return _lastTask;
            }
        }

        public Task<T> Enqueue<T>(Func<Task<T>> asyncFunction)
        {
            lock (_lock)
            {
                _lastTask = _lastTask is null
                    ? Task.Run(asyncFunction)
                    : _lastTask.ContinueWith(_ => asyncFunction(), ExecuteSynchronously).Unwrap();

                return (Task<T>)_lastTask;
            }
        }
    }
}