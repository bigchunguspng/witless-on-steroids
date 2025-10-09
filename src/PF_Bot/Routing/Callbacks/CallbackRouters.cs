using PF_Bot.Backrooms.Helpers;
using PF_Bot.Routing_Legacy;
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
        if (query.Data == null) return;

        LogDebug($"[Callback] {query.From.Id,14}.u {query.Message!.Chat.Id,14}.c {query.Message.Id,8}.M  |  {query.Data}");

        var (key, content) = query.ParseData();

        if (key == null) return;

        var handler = registry.Resolve(key);
        if (handler != null)
            handler.Invoke().Handle(new CallbackContext(query, key, content));
    }
}