using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands
{
    public class RemoveAudio : RemoveBitrate
    {
        public override void Run()
        {
            if (NoVideo()) return;

            Bot.Download(FileID, Chat, out string path, out var type);
            
            if (type == MediaType.Round) path = Bot.MemeService.CropVideoNote(path);

            using var stream = File.OpenRead(Bot.MemeService.RemoveAudio(path));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFilename()));
            Log($"{Title} >> GIF [~]");

            string VideoFilename() => "gif_fap_club.mp4";
        }

        protected bool NoVideo()
        {
            if (GetMediaFileID(Message) || GetMediaFileID(Message.ReplyToMessage)) return false;

            Bot.SendMessage(Chat, G_MANUAL);
            return true;
        }

        private bool GetMediaFileID(Message mess)
        {
            if (mess == null) return false;

            if (mess.Video is { } v)
            {
                FileID = v.FileId;
                Memes.PassSize(v);
            }
            else if (mess.Animation is { } a)
            {
                FileID = a.FileId;
                Memes.PassSize(a);
            }
            else if (mess.Sticker is { IsVideo: true } s)
            {
                FileID = s.FileId;
                Memes.PassSize(s);
            }
            else if (mess.VideoNote is { } n)
            {
                FileID = n.FileId;
                Memes.PassSize(272);
            }
            else return false;

            return true;
        }
    }
}