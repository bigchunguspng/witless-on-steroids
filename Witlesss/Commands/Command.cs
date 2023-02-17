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
            Title   = TitleOrUsername(message);
        }

        public abstract void Run();
    }

    public abstract class WitlessCommand : Command
    {
        public static Witless Baka { get; private set; }

        protected static void SetBaka(Witless witless) => Baka = witless;

        protected static void DropBaka()
        {
            Baka.Unload();
            Baka = null;
        }
    }
}