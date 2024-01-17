using System;
using Telegram.Bot.Types;

namespace Witlesss.Commands
{
    public abstract class Command
    {
        protected static Bot Bot => Bot.Instance;

        protected static Message Message { get; private set; }
        protected static string  Text    { get; private set; }
        protected static string  Title   { get; private set; }
        protected static long    Chat    { get; private set; }

        protected static bool ChatIsPrivate => Chat > 0;

        public static (long ID, string Title) LastChat => new(Chat, Title);
        
        public void Pass(Message message)
        {
            Message = message;
            Text    = message.Caption ?? message.Text;
            Chat    = message.Chat.Id;
            Title   = ChatTitle;
        }

        public abstract void Run();

        protected static DateTime MessageDateTime => Message.EditDate ?? Message.Date;

        protected static string SenderName => Message.SenderChat?.Title ?? GetUserFullName();
        private   static string ChatTitle => (ChatIsPrivate ? GetUserFullName() : Message.Chat.Title).Truncate(32);

        private static string GetUserFullName()
        {
            string name = Message.From?.FirstName;
            string last = Message.From?.LastName ?? "";
            return last == "" ? name : name + " " + last;
        }

        protected static string TextWithoutBotUsername => RemoveBotMention(Text);

        protected static string RemoveBotMention(string s) => s.ToLower().Replace(Config.BOT_USERNAME, "");

        /// <summary> Use this for async operations. </summary>
        protected static MessageData SnapshotMessageData() => new(Chat, Text, Title);
    }

    public abstract class WitlessCommand : Command
    {
        protected static Witless Baka { get; private set; }

        protected static void SetBaka(Witless witless)
        {
            Baka = witless;
        }

        protected static void DropBaka() // like it's hot 🔥
        {
            Baka.Unload();
            Baka = null;
        }

        /// <summary> Use this for async operations. </summary>
        protected new static WitlessMessageData SnapshotMessageData() => new(Baka, Chat, Text, Title);
    }
    
    public abstract class CallBackHandlingCommand : Command
    {
        public abstract void OnCallback(CallbackQuery query);
    }

    public record        MessageData              (long Chat, string Text, string Title);
    public record WitlessMessageData(Witless Baka, long Chat, string Text, string Title);
}