using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public abstract class VideoCommand : Command
    {
        protected string FileID;

        protected bool NoVideo()
        {
            if (GetMediaFileID(Message) || GetMediaFileID(Message.ReplyToMessage)) return false;

            Bot.SendMessage(Chat, G_MANUAL);
            return true;
        }

        private bool GetMediaFileID(Message m)
        {
            if      (m == null) return false;

            if      (m.Video     is not null)          FileID = m.Video    .FileId;
            else if (m.Sticker   is { IsVideo: true }) FileID = m.Sticker  .FileId;
            else if (m.VideoNote is not null)          FileID = m.VideoNote.FileId;
            else if (m.Animation is not null)          FileID = m.Animation.FileId;
            else return false;

            return true;
        }
    }

    public abstract class AudioVideoCommand : Command
    {
        protected string FileID;

        protected static string SongNameOr(string  s) => SongNameIn(Message) ?? SongNameIn(Message.ReplyToMessage) ?? s;
        private   static string SongNameIn(Message m) => m?.Audio?.FileName ?? m?.Document?.FileName;

        protected static string Sender => ValidFileName(SenderName);

        protected bool NothingToProcess()
        {
            if (GetMediaFileID(Message) || GetMediaFileID(Message.ReplyToMessage)) return false;

            Bot.SendMessage(Chat, DAMN_MANUAL);
            return true;
        }

        private bool GetMediaFileID(Message m)
        {
            if      (m == null) return false;

            if      (m.Audio     is not null)                   FileID = m.Audio    .FileId;
            else if (m.Video     is not null)                   FileID = m.Video    .FileId;
            else if (m.Animation is not null)                   FileID = m.Animation.FileId;
            else if (m.Sticker   is { IsVideo: true })          FileID = m.Sticker  .FileId;
            else if (m.Voice     is not null)                   FileID = m.Voice    .FileId;
            else if (m.VideoNote is not null)                   FileID = m.VideoNote.FileId;
            else if (m.Document  is not null && MightBeWav(m))  FileID = m.Document .FileId;
            else return false;

            return true;
        }

        private bool MightBeWav(Message m) => m.Document!.FileName!.EndsWith(".wav");

        protected static void SendResult(string result, MediaType type, Func<string> video, Func<string> audio)
        {
            using var stream = File.OpenRead(result);
            if      (type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, audio()));
            else if (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, video()));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, video()));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }
    }
}