using Telegram.Bot.Types;
using Witlesss.Commands.Generation;

namespace Witlesss.Commands.Messaging;

public class QueueMessage : SyncCommand
{
    protected override void Run()
    {
        if (!Message.SenderIsBotAdmin())
        {
            Bot.SendMessage(Origin, FORBIDDEN.PickAny());
            return;
        }

        if (Args is null)
        {
            Bot.SendMessage(Origin, "<code>/que [chat|.] [text]</code>");
            return;
        }

        var args = Args.SplitN(2);
        var chat = args[0] is "." ? Chat : long.Parse(args[0]);
        var text = args[1];

        PoopText.Enqueue(chat, text);
        if (ChatService.Knowns (chat))
            ChatService.GetBaka(chat).Eat(text);
        LogQueue(chat);
    }

    private static void LogQueue(ChatId chat) => Log($"QUEUE >> {chat}", LogLevel.Info, LogColor.Yellow);
}