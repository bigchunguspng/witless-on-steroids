using System;
using Telegram.Bot.Types;

namespace Witlesss.Commands.Core
{
    public abstract class Command
    {
        protected static Bot Bot => Bot.Instance;

        protected static Message Message { get; private set; }
        protected static string  Text    { get; private set; }
        protected static string  Title   { get; private set; }
        protected static long    Chat    { get; private set; }

        protected static bool ChatIsPrivate => ChatIsPrivate(Chat);

        public static (long ID, string Title) LastChat => new(Chat, Title);
        
        public void Pass(Message message)
        {
            Message = message;
            Text    = message.Caption ?? message.Text;
            Chat    = message.Chat.Id;
            Title   = GetChatTitle(Message);
        }

        public abstract void Run();

        protected static DateTime MessageDateTime => Message.EditDate ?? Message.Date;

        protected static string TextWithoutBotUsername => RemoveBotMention(Text);

        protected static string RemoveBotMention(string s) => s.ToLower().Replace(Config.BOT_USERNAME, "");

        /// <summary> Use this for async operations. </summary>
        protected static MessageData SnapshotMessageData() => new(Message, Chat, Text, Title);
    }

    public abstract class CallBackHandlingCommand : Command
    {
        public abstract void OnCallback(CallbackQuery query);
    }

    public record MessageData(Message Message, long Chat, string Text, string Title);
}