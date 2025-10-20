using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Messages.Auto;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Messages;

public interface IMessageRouter
{
    void Route(Message message);
}

public class MessageRouter_Skip : MessageHandler, IMessageRouter
{
    public void Route(Message message)
    {
        Context = new MessageContext(message);

        Print($"{Title} >> {Text}", ConsoleColor.Gray);
    }
}

public class MessageRouter_Default
    (CommandRegistry<Func<CommandHandler>> registry) : MessageHandler, IMessageRouter
{
    private readonly MessageRouter_KnownChat _branch_KnownChat = new(registry);

    public void Route(Message message)
    {
        Context = new MessageContext(message);

        var messageIsCommand = Text != null && Text.StartsWith('/');
        if (messageIsCommand && HandleCommand())
            return;

        if (ChatManager.Knowns(Chat, out var settings))
            _branch_KnownChat.Route(Context, settings);
    }

    private bool HandleCommand()
    {
        var handler = registry.Resolve(Text, out var command, offset: 1);
        if (handler != null)
        {
            var context = CommandContext.CreateOrdinary(Context.Message, command!);

            var forMe = CommandIsForMe(context);
            if (forMe)
            {
                _ = handler.Invoke().Handle(context);
            }

            return forMe;
        }

        return false;
    }

    private bool CommandIsForMe(CommandContext context)
    {
        var options = context.Options;
        if (options == null) 
            return true; // no bot mentioned

        var mention_start = options.IndexOf("@", StringComparison.Ordinal);
        if (mention_start < 0) 
            return true; // no bot mentioned x2

        var mention_tail = options.LastIndexOf("bot", StringComparison.OrdinalIgnoreCase);
        if (mention_tail < mention_start)
            return true; // Options: ……@…… / ……bot…@……

        return false; // some other bot mentioned
    }
}