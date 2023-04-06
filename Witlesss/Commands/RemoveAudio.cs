using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.Memes;

namespace Witlesss.Commands
{
    public class RemoveAudio : RemoveBitrate
    {
        public override void Run()
        {
            if (NoVideo()) return;

            Bot.Download(FileID, Chat, out string path, out var type);
            
            if (type == MediaType.Round) path = CropVideoNote(path);

            using var stream = File.OpenRead(RemoveAudio(path));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFilename()));
            Log($"{Title} >> GIF [~]");

            string VideoFilename() => "gif_fap_club.mp4";
        }

        protected virtual int VideoNoteSize() => 272;

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
                PassSize(v);
            }
            else if (mess.Animation is { } a)
            {
                FileID = a.FileId;
                PassSize(a);
            }
            else if (mess.Sticker is { IsVideo: true } s)
            {
                FileID = s.FileId;
                PassSize(s);
            }
            else if (mess.VideoNote is { } n)
            {
                FileID = n.FileId;
                PassSize(VideoNoteSize());
            }
            else return false;

            return true;
        }
    }
}