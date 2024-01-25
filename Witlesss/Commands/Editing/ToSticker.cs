using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.Memes;

namespace Witlesss.Commands.Editing
{
    public class ToSticker : FileEditingCommand
    {
        public override void Run()
        {
            if (NothingToProcess()) return;

            Bot.Download(FileID, Chat, out var path);

            using var stream = File.OpenRead(Stickerize(path));
            Bot.SendSticker(Chat, new InputOnlineFile(stream));
            if (Text[^1] is 's' or 'S') Bot.SendMessage(Chat, "@Stickers");
            Log($"{Title} >> STICK [!]");
        }

        protected override string Manual { get; } = STICK_MANUAL;

        protected override bool MessageContainsFile(Message m) => GetPhotoFileID(m);

        protected override bool ChatIsBanned() => false;
    }
}