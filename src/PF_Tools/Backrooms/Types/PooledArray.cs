using System.Buffers;

namespace PF_Tools.Backrooms.Types;

/// Use it only on the stack!
public readonly struct PooledArray<T> : IDisposable
{
    private readonly ArrayPool<T> _pool;

    public T[] Array { get; }

    public PooledArray(int length)
    {
        _pool = ArrayPool<T>.Shared;
        Array = _pool.Rent(length);
    }

    public void Dispose()
    {
        _pool.Return(Array);
    }
}