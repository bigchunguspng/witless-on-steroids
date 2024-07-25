namespace Witlesss.Commands.Messaging;

public class Tell : SyncCommand
{
    protected override void Run()
    {
        if (Message.From?.Id != Config.AdminID)
        {
            Bot.SendMessage(Chat, "LOL XD)0)");
            return;
        }

        var args = Args?.Split(' ', 2);
        var messageId = Message.ReplyToMessage is { } reply ? reply.MessageId : -1;

        var chatProvided = args is not null && args.Length > 0;
        var textProvided = args is not null && args!.Length >= 2;
        var copyProvided = messageId >= 0;

        if (!chatProvided || !textProvided && !copyProvided)
        {
            Bot.SendMessage(Chat, "<code>/tell [chat] [message]</code>");
            return;
        }
        
        var chat = long.TryParse(args![0], out var x) ? x : 0;
        if (chat != 0)
        {
            if (textProvided)
            {
                var text = args[1];
                Bot.SendMessage(chat, text, preview: false);
                if (ChatsDealer.WitlessExist(chat, out var baka)) baka.Eat(text);
            }
            else
                Bot.CopyMessage(chat, Chat, messageId);
        }
    }
}