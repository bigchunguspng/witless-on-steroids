using Telegram.Bot.Types;

namespace PF_Bot.Routing.Callbacks;

public interface ICallbackRouter
{
    void Route(CallbackQuery query);
}

public class CallbackRouter_Skip : ICallbackRouter
{
    public void Route(CallbackQuery query)
    {
        Print(query.Data ?? "-", ConsoleColor.Yellow);
    }
}

public class CallbackRouter_Default
    (CommandRegistry<Func<CallbackHandler>> registry) : ICallbackRouter
{
    public void Route(CallbackQuery query)
    {
        var data = query.Data;
        if (data == null) 
            return;

        var message = query.Message!;

        LogDebug($"[Callback] {query.From.Id,14}.u {message.Chat.Id,14}.c {message.Id,8}.M  |  {data}");

        var parts = data.Contains(SEPARATOR) ? data.Split(SEPARATOR, 2) : [];
        if (parts.Length != 2) 
            return;

        var (key, content) = (parts[0], parts[1]);

        var handler = registry.Resolve(key);
        if (handler == null) return;

        _ = handler.Invoke().Handle(new CallbackContext(query, key, content));
    }

    private const string SEPARATOR = " - ";
}