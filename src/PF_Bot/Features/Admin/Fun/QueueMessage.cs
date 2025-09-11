using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Chats;
using PF_Bot.Features.Generate.Text;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Admin.Fun;

public class QueueMessage : SyncCommand
{
    protected override void Run()
    {
        if (Message.SenderIsBotAdmin().Janai())
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
        if (ChatManager.KnownsChat(chat))
            ChatManager.GetBaka   (chat).Eat(text);
        LogQueue(chat);
    }

    private static void LogQueue(ChatId chat) => Log($"QUEUE >> {chat}", LogLevel.Info, LogColor.Yellow);
}