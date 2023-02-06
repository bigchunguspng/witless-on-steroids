using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.Memes;

namespace Witlesss.Commands
{
    public class ToSticker : Command
    {
        private string _fileID;
        public override void Run()
        {
            if (NoPicture()) return;

            Bot.Download(_fileID, Chat, out string path);

            Bot.SendSticker(Chat, new InputOnlineFile(File.OpenRead(Bot.MemeService.Stickerize(path))));
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
            if (mess == null) return false;

            if (mess.Photo is { } p)
            {
                _fileID = p[^1].FileId;
                PassSize (p[^1]);
            }
            else if (mess.Document is { MimeType: "image/png" or "image/jpeg", Thumb: { } } d)
            {
                _fileID = d.FileId;
                PassSize (d.Thumb);
            }
            else if (mess.Sticker is { IsVideo: false, IsAnimated: false } s)
            {
                _fileID = s.FileId;
                PassSize (s);
            }
            else return false;

            return true;
        }
    }
}