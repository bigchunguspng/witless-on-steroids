namespace Witlesss.Commands.Messaging;

public class Reply : SyncCommand
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
            Bot.SendMessage(Origin, "<code>/rep [message_url] [text|message]</code>");
            return;
        }

        var args = Args.SplitN(2);
        var (chat, message) = args[0].GetChatIdAndMessage();

        var messageToCopy = Message.ReplyToMessage is { } reply ? reply.Id : -1;
        if (messageToCopy < 0)
        {
            Bot.SendMessage(chat, args[1], preview: true, replyTo: message);
            var chatId = chat.Identifier ?? 0;
            if (chatId != 0 && ChatService.Knowns(chatId)) ChatService.GetBaka(chatId).Eat(args[1]);
        }
        else
            Bot.CopyMessage(chat, Chat, messageToCopy, replyTo: message);
    }
}