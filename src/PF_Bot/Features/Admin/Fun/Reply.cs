using PF_Bot.Backrooms.Helpers;
using PF_Bot.Routing.Commands;
using PF_Bot.State.Chats;
using PF_Bot.Tools_Legacy.Technical;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Admin.Fun;

public class Reply : SyncCommand
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
            if (chatId != 0 && ChatManager.KnownsChat(chatId)) ChatManager.GetBaka(chatId).Eat(args[1]);
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