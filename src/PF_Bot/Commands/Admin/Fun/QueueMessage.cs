using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Commands.Admin.Fun;

public class QueueMessage : CommandHandlerBlocking_Admin
{
    protected override void Run()
    {
        if (Args is null)
        {
            SendManual("<code>/que [chat|.] [text]</code>");
            return;
        }

        var args = Args.SplitN(2);
        var chat = args[0] is "." ? Chat : long.Parse(args[0]);
        var text = args[1];

        App.FunnyMessages.Enqueue(chat, text);

        if (ChatManager.Knowns (chat))
            PackManager.GetBaka(chat).Eat(text);

        LogQueue(chat);
    }

    private static void LogQueue(ChatId chat) => Log($"QUEUE >> {chat}", LogLevel.Info, LogColor.Yellow);
}