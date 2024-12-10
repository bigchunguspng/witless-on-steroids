using Telegram.Bot.Types;

namespace Witlesss.Commands.Messaging;

public class Tell : SyncCommand
{
    protected override void Run()
    {
        if (!Message.SenderIsBotAdmin())
        {
            Bot.SendMessage(Origin, "LOL XD)0)");
            return;
        }

        if (Args is null)
        {
            Bot.SendMessage(Origin, "<code>/tell [chat|@chat|.] [text|message]</code>");
            return;
        }

        var args = Args.SplitN(2);
        var chat = args[0] is "." ? Chat : new ChatId(args[0]);

        var messageToCopy = Message.ReplyToMessage is { } reply ? reply.Id : -1;
        if (messageToCopy < 0)
        {
            Bot.SendMessage(chat, args[1], preview: true);
            var chatId = chat.Identifier ?? 0;
            if (chatId != 0 && ChatService.Knowns(chatId)) ChatService.GetBaka(chatId).Eat(args[1]);
        }
        else
            Bot.CopyMessage(chat, Chat, messageToCopy);
    }
}