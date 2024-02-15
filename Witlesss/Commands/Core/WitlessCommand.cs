using Telegram.Bot.Types;

namespace Witlesss.Commands.Core
{
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
        protected new static WitlessMessageData SnapshotMessageData() => new(Baka, Message, Chat, Text, Title);
    }

    public record WitlessMessageData(Witless Baka, Message Message, long Chat, string Text, string Title);
}