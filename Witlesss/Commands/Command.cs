using Telegram.Bot.Types;

namespace Witlesss.Commands
{
    public abstract class Command
    {
        public static Bot Bot;

        public static Message Message { get; private set; }
        public static string  Text    { get; private set; }
        public static string  Title   { get; private set; }
        public static long    Chat    { get; private set; }

        public static bool    ChatIsPrivate => Chat > 0;
        
        public void Pass(Message message)
        {
            Message = message;
            Text    = message.Caption ?? message.Text;
            Chat    = message.Chat.Id;
            Title   = TitleOrUsername;
        }

        public abstract void Run();
        
        public static string SenderName => Message.SenderChat?.Title  ?? UserFullName();
        public static string TitleOrUsername => Truncate(ChatIsPrivate ? UserFullName() : Message.Chat.Title, 32);

        private static string UserFullName()
        {
            string name = Message.From?.FirstName;
            string last = Message.From?.LastName ?? "";
            return last == "" ? name : name + " " + last;
        }
        
        public static string RemoveBotMention() => Text.ToLower().Replace(Config.BOT_USERNAME, "");
    }

    public abstract class WitlessCommand : Command
    {
        protected static Witless Baka { get; private set; }

        protected static void SetBaka(Witless witless) => Baka = witless;

        protected static void DropBaka()
        {
            Baka.Unload();
            Baka = null;
        }
    }
}