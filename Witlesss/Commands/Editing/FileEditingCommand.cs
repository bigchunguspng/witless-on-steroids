using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public abstract class VideoCommand : FileEditingCommand
    {
        protected override string Manual { get; } = G_MANUAL;

        protected override bool MessageContainsFile(Message m) => GetVideoFileID(m);
    }

    public abstract class FileEditingCommand : Command
    {
        protected string      FileID;
        private   string PhotoFileID;

        protected virtual string Manual { get; } = DAMN_MANUAL;

        protected bool NothingToProcess()
        {
            if (GetFileFrom(Message) || GetFileFrom(Message.ReplyToMessage)) return false;

            SendManual();
            return true;
        }

        protected virtual void SendManual() => Bot.SendMessage(Chat, Manual);

        private bool GetFileFrom(Message m)
        {
            return m is not null && MessageContainsFile(m);
        }

        protected virtual bool MessageContainsFile(Message m)
        {
            return GetVideoFileID(m) || GetAudioFileID(m);
        }

        protected bool GetVideoFileID(Message m)
        {
            if      (m.Video     is not null)          FileID = m.Video    .FileId;
            else if (m.Animation is not null)          FileID = m.Animation.FileId;
            else if (m.Sticker   is { IsVideo: true }) FileID = m.Sticker  .FileId;
            else if (m.VideoNote is not null)          FileID = m.VideoNote.FileId;
            else return false;

            return true;
        }
        protected bool GetAudioFileID(Message m)
        {
            if      (m.Audio     is not null)                  FileID = m.Audio   .FileId;
            else if (m.Voice     is not null)                  FileID = m.Voice   .FileId;
            else if (m.Document  is not null && MightBeWav(m)) FileID = m.Document.FileId;
            else return false;

            return true;
        }
        protected bool GetPhotoFileID(Message m)
        {
            if      (m.Photo    is not null)                              FileID = m.Photo[^1].FileId;
            else if (m.Sticker  is { IsVideo: false, IsAnimated: false }) FileID = m.Sticker  .FileId;
            else if (m.Document is not null && IsPicture(m.Document))     FileID = m.Document .FileId;
            else return false;

            PhotoFileID = FileID;

            return true;
        }
        private static bool IsPicture(Document d) => d is { MimeType: "image/png" or "image/jpeg", Thumb: not null };
        private static bool MightBeWav(Message m) => m.Document!.FileName!.EndsWith(".wav");


        protected void SendResult(string result, MediaType type)
        {
            using var stream = File.OpenRead(result);
            if        (FileID == PhotoFileID) Bot.SendPhoto    (Chat, new InputOnlineFile(stream));
            else if (type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, AudioFileName));
            else if (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, VideoFileName));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }

        protected virtual string VideoFileName { get; } = "piece_fap_club.mp3";
        protected virtual string AudioFileName { get; } = "piece_fap_club.mp4";

        protected static string Sender => ValidFileName(SenderName);

        protected static string SongNameOr(string  s) => SongNameIn(Message) ?? SongNameIn(Message.ReplyToMessage) ?? s;
        private   static string SongNameIn(Message m) => m?.Audio?.FileName ?? m?.Document?.FileName;
    }
}