using System.Buffers;

namespace PF_Tools.Backrooms.Types;

/// Use it only on the stack!
public readonly struct PooledArray<T> (int length) : IDisposable
{
    public T[] Array { get; } = ArrayPool<T>.Shared.Rent(length);

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(Array);
    }
}