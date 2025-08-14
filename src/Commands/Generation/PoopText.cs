namespace Witlesss.Commands.Generation;

public class PoopText : WitlessAsyncCommand
{
    protected override async Task Run()
    {
        await Task.Delay(GetRealisticResponseDelay(Text));

        var text = (_pendingTexts > 0 ? TryDequeue(Origin.Chat) : null) ?? Baka.Generate();

        Bot.SendMessage(Origin, text, preview: true);
        Log($"{Title} >> FUNNY");
    }

    private static int GetRealisticResponseDelay(string? text)
    {
        return text is null
            ? 150
            : Math.Min(text.Length, 120) * 25; // 1 second / 40 characters, 3 seconds max
    }


    // MESSAGE QUEUEING

    private static int _pendingTexts;
    private static readonly Dictionary<long, Queue<string>> _queues = new();

    public static void Enqueue(long chat, string text)
    {
        if (!_queues.TryGetValue(chat, out var queue))
        {
            queue = new Queue<string>();
            _queues[chat] = queue;
        }

        queue.Enqueue(text);
        _pendingTexts++;
    }

    private static string? TryDequeue(long chat)
    {
        if (!_queues.TryGetValue(chat, out var queue))
            return null;

        _pendingTexts--;
        return queue.Dequeue();
    }
}