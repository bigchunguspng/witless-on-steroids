using PF_Bot.Features_Aux.Packs;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Commands.Admin.Fun;

public class Reply : CommandHandlerBlocking_Admin
{
    protected override void Run()
    {
        if (Args is null)
        {
            SendManual("<code>/rep [message_url] [text|message]</code>");
            return;
        }

        var args = Args.SplitN(2);
        var (chat, message) = args[0].GetChatIdAndMessage();

        var messageToCopy = Message.ReplyToMessage is { } reply ? reply.Id : -1;
        if (messageToCopy < 0)
        {
            Bot.SendMessage(chat, args[1], preview: true, replyTo: message);
            var chatId = chat.Identifier ?? 0;
            if (chatId != 0 && ChatManager.Knowns(chatId)) PackManager.GetBaka(chatId).Eat(args[1]);
            LogReply(chat);
        }
        else
        {
            Bot.CopyMessage(chat, Chat, messageToCopy, replyTo: message);
            LogReply(chat);
        }
    }

    private static void LogReply(ChatId chat) => Log($"REPLY >> {chat}", LogLevel.Info, LogColor.Yellow);
}