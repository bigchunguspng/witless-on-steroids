namespace Witlesss.Commands.Messaging;

public class Tell : SyncCommand
{
    private readonly Regex _replyId = new(@"\d+");

    protected override void Run()
    {
        if (!Message.SenderIsBotAdmin())
        {
            Bot.SendMessage(Chat, "LOL XD)0)");
            return;
        }

        var args = Args?.Split(' ', 2);
        var messageId = Message.ReplyToMessage is { } reply ? reply.Id : -1;

        var chatProvided = args is not null && args.Length > 0;
        var textProvided = args is not null && args!.Length >= 2;
        var copyProvided = messageId >= 0;

        if (!chatProvided || !textProvided && !copyProvided)
        {
            Bot.SendMessage(Chat, "<code>/tell[replyTo] [chat] [message]</code>");
            return;
        }
        
        var chat = long.TryParse(args![0], out var x) ? x : 0;
        if (chat != 0)
        {
            var replyTo = _replyId.ExtractGroup(0, Command!, int.Parse, null);
            if (textProvided)
            {
                Bot.SendMessage(chat, args[1], preview: true, replyTo);
                if (ChatService.Knowns(chat)) ChatService.GetBaka(chat).Eat(args[1]);
            }
            else
                Bot.CopyMessage(chat, Chat, messageId, replyTo);
        }
    }
}