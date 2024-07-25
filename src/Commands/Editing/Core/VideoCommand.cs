using Telegram.Bot.Types;

namespace Witlesss.Commands.Editing.Core
{
    public abstract class VideoCommand : FileEditingCommand
    {
        protected override string Manual { get; } = G_MANUAL;

        protected override bool MessageContainsFile(Message m) => GetVideoFileID(m);
    }
}