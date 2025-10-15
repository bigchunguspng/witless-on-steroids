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

    private CommandResultStatus Status = CommandResultStatus.OK;

    public async Task Handle(CallbackContext context)
    {
        Context = context;
        try
        {
            await Run();
        }
        catch (Exception exception)
        {
            Status = CommandResultStatus.FAIL;

            Unluckies.Handle(exception, Context, $"CALLBACK H. | {Title}");
        }
        finally
        {
            BigBrother.LogCallback(Chat, Status, Query.Data);
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