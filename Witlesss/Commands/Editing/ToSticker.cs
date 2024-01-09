using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.Memes;

namespace Witlesss.Commands.Editing
{
    public class ToSticker : Command
    {
        private string _fileID;

        public override void Run()
        {
            if (NoPicture()) return;

            Bot.Download(_fileID, Chat, out string path);

            using var stream = File.OpenRead(Stickerize(path));
            Bot.SendSticker(Chat, new InputOnlineFile(stream));
            if (Text[^1] is 's' or 'S') Bot.SendMessage(Chat, "@Stickers");
            Log($"{Title} >> STICK [!]");
        }

        private bool NoPicture()
        {
            if (GetPicID(Message) || GetPicID(Message.ReplyToMessage)) return false;

            Bot.SendMessage(Chat, STICK_MANUAL);
            return true;
        }

        private bool GetPicID(Message mess)
        {
            if      (mess == null) return false;

            if      (mess.Photo    is { } p)                 _fileID = p[^1].FileId;
            else if (mess.Document is { } d && IsPicture(d)) _fileID = d    .FileId;
            else if (mess.Sticker  is { } s && IsStatic (s)) _fileID = s    .FileId;
            else return false;

            return true;
        }

        private static bool IsStatic (Sticker  s) => s is { IsVideo: false, IsAnimated: false };
        private static bool IsPicture(Document d) => d is { MimeType: "image/png" or "image/jpeg", Thumb: not null };
    }
}