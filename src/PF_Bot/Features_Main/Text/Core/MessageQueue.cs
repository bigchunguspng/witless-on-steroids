using System.Runtime.CompilerServices;

namespace PF_Bot.Features_Main.Text.Core;

public class MessageQueue
{
    private int _pendingTexts;
    private readonly Dictionary<long, Queue<string>> _queues = new();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Enqueue(long chat, string text)
    {
        if (_queues.TryGetValue_Failed(chat, out var queue))
        {
            queue = new Queue<string>();
            _queues[chat] = queue;
        }

        queue.Enqueue(text);
        _pendingTexts++;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public string? TryDequeue(long chat)
    {
        if (_pendingTexts <= 0)
            return null;

        if (_queues.TryGetValue_Failed(chat, out var queue))
            return null;

        _pendingTexts--;
        return queue.Dequeue();
    }
}