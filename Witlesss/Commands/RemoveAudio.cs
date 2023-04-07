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

        protected bool NoVideo()
        {
            if (GetMediaFileID(Message) || GetMediaFileID(Message.ReplyToMessage)) return false;

            Bot.SendMessage(Chat, G_MANUAL);
            return true;
        }

        private bool GetMediaFileID(Message mess)
        {
            if (mess == null) return false;

            if      (mess.Video     is not null)          FileID = mess.Video    .FileId;
            else if (mess.Sticker   is { IsVideo: true }) FileID = mess.Sticker  .FileId;
            else if (mess.VideoNote is not null)          FileID = mess.VideoNote.FileId;
            else if (mess.Animation is not null)          FileID = mess.Animation.FileId;
            else return false;

            return true;
        }
    }
}