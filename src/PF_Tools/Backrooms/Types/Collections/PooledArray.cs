using System.Buffers;

namespace PF_Tools.Backrooms.Types.Collections;

/// Wrapper for shared <see cref="ArrayPool&lt;T&gt;"/>.
/// Use it only on the stack!
public readonly struct PooledArray<T> (int length) : IDisposable
{
    public T[] Array { get; } = ArrayPool<T>.Shared.Rent(length);

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(Array);
    }
}