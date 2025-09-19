using PF_Bot.Core;
using PF_Bot.Telegram;
using Telegram.Bot.Types;

namespace PF_Bot.Routing_New.Routers;

public abstract class CallbackHandler
{
    protected static Bot Bot => App.Bot;

    // todo: 1. make it a context, 2. move GetPagination() here
    protected CallbackQuery Query   { get; private set; } = null!;
    protected string        Key     { get; private set; } = null!;
    protected string        Content { get; private set; } = null!;

    public async void Handle(CallbackQuery query, string key, string content)
    {
        try
        {
            Query = query;
            Key = key;
            Content = content;

            await Run();
        }
        catch (Exception e)
        {
            Bot.LogError_ToFile(e, query, "Callback");
        }
    }

    protected abstract Task Run();
}