using PF_Bot.Core;
using PF_Bot.Features_Aux.Listing;
using PF_Bot.Telegram;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Callbacks;

public abstract class CallbackHandler
{
    protected static Bot Bot => App.Bot;

    private CallbackContext Context { get;  set; } = null!;

    protected CallbackQuery Query   => Context.Query;
    protected Message       Message => Context.Message;
    protected long          Chat    => Context.Chat;
    protected string        Title   => Context.Title;
    protected string        Key     => Context.Key;
    protected string        Content => Context.Content;

    protected MessageOrigin Origin => Context.Origin;

    public async void Handle(CallbackContext context)
    {
        try
        {
            Context = context;
            await Run();
        }
        catch (Exception e)
        {
            Bot.LogError_ToFile(e, Context, "Callback");
        }
    }

    protected abstract Task Run();

    protected ListPagination GetPagination(string content)
    {
        var numbers = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var    page = int.Parse(numbers[0]);
        var perPage = int.Parse(numbers[1]);
        return new ListPagination(Origin, Message.Id, page, perPage);
    }
}