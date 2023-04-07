using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands
{
    public class RemoveBitrate : Command
    {
        protected string FileID;
        
        public override void Run()
        {
            if (NothingToProcess()) return;

            var value = 15;
            if (HasIntArgument(Text, out int b)) value = Math.Clamp(b, 0, 21);

            Bot.Download(FileID, Chat, out string path, out var type);

            string result = Memes.RemoveBitrate(path, value + 30); // 30 - 51
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> DAMN [*]");

            string AudioFilename() => SongNameOr($"Damn, {Sender}.mp3");
            string VideoFilename() => $"piece_fap_club-{value}.mp4";
        }

        protected static string SongNameOr(string s) => Message.Audio?.FileName ?? Message.Document?.FileName ?? s;

        protected static string Sender => ValidFileName(SenderName);

        protected bool NothingToProcess()
        {
            if (GetMediaFileID(Message) || GetMediaFileID(Message.ReplyToMessage)) return false;
            
            Bot.SendMessage(Chat, DAMN_MANUAL);
            return true;
        }
        
        private bool GetMediaFileID(Message mess)
        {
            if (mess == null) return false;

            if      (mess.Audio     is not null)                   FileID = mess.Audio    .FileId;
            else if (mess.Video     is not null)                   FileID = mess.Video    .FileId;
            else if (mess.Animation is not null)                   FileID = mess.Animation.FileId;
            else if (mess.Sticker   is { IsVideo: true })          FileID = mess.Sticker  .FileId;
            else if (mess.Voice     is not null)                   FileID = mess.Voice    .FileId;
            else if (mess.VideoNote is not null)                   FileID = mess.VideoNote.FileId;
            else if (mess.Document  is { MimeType: "audio/x-wav"}) FileID = mess.Document .FileId;
            else return false;

            return true;
        }

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