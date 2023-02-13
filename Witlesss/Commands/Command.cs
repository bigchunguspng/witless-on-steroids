using Telegram.Bot.Types;

namespace Witlesss.Commands
{
    public abstract class Command
    {
        public static Bot Bot;

        protected static Message Message;
        protected static string  Text, Title;
        protected static long    Chat;

        protected bool ChatIsPrivate => Chat > 0;
        
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
        protected static Witless Baka;

        protected static void DropBaka()
        {
            Baka.Unload();
            Baka = null;
        }
    }
}