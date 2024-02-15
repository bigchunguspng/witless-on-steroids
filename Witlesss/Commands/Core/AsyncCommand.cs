using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Witlesss.Commands.Core
{
    public abstract class AsyncCommand
    {
        protected static Bot Bot => Bot.Instance;

        protected readonly MessageData MessageData;
        
        protected Message Message { get; private set; }
        protected string  Text    { get; private set; }
        protected string  Title   { get; private set; }
        protected long    Chat    { get; private set; }

        protected AsyncCommand(MessageData message)
        {
            MessageData = message;
            Message     = message.Message;
            Text        = message.Text;
            Chat        = message.Chat;
            Title       = message.Title;
        }

        public abstract Task RunAsync();
    }
}