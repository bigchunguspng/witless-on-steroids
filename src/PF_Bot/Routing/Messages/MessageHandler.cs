using Telegram.Bot.Types;

namespace PF_Bot.Routing.Messages;

public abstract class MessageHandler
{
    protected MessageContext Context = null!;

    protected Message       Message => Context.Message;
    protected MessageOrigin Origin  => Context.Origin;
    protected long          Chat    => Context.Chat;
    protected string        Title   => Context.Title;
    protected string?       Text    => Context.Text;

    // Implement handling logic on your own lol.
}