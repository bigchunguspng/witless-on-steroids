using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Commands.Admin.Fun;

public class Tell : CommandHandlerBlocking_Admin
{
    protected override void Run()
    {
        if (Args is null)
        {
            SendManual("<code>/tell [chat|@chat|.] [text|message]</code>");
            return;
        }

        var args = Args.SplitN(2);
        var chat = args[0] is "." ? Chat : new ChatId(args[0]);

        var messageToCopy = Message.ReplyToMessage is { } reply ? reply.Id : -1;
        if (messageToCopy < 0)
        {
            Bot.SendMessage(chat, args[1], preview: true);
            var chatId = chat.Identifier ?? 0;
            if (chatId != 0 && ChatManager.Knowns(chatId)) PackManager.GetBaka(chatId).Eat(args[1]);
            LogTell(chat);
        }
        else
        {
            Bot.CopyMessage(chat, Chat, messageToCopy);
            LogTell(chat);
        }
    }

    private static void LogTell(ChatId chat) => Log($"TELL >> {chat}", LogLevel.Info, LogColor.Yellow);
}