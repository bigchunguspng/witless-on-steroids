using System.Buffers;

namespace PF_Tools.Backrooms.Types.Collections;

/// Wrapper for shared <see cref="ArrayPool&lt;T&gt;"/>.
public readonly struct PooledArray<T> : IDisposable
{
    public T[] Array { get; }

    public PooledArray(int length, bool clear = false)
    {
        Array = ArrayPool<T>.Shared.Rent(length);
        if (clear) Array.AsSpan(0, length).Clear();
    }

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(Array);
    }
}