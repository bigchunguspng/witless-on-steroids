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

            string result = Bot.MemeService.RemoveBitrate(path, ref value, type);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> DAMN [*]");

            string AudioFilename() => MediaFileName($"Damn, {Sender}.mp3");
            string VideoFilename() => $"piece_fap_club-{value}.mp4";
        }

        protected string MediaFileName(string s) => Message.Audio?.FileName ?? Message.Document?.FileName ?? s;

        protected string Sender => ValidFileName(SenderName);

        protected bool NothingToProcess()
        {
            if (GetMediaFileID(Message) || GetMediaFileID(Message.ReplyToMessage)) return false;
            
            Bot.SendMessage(Chat, DAMN_MANUAL);
            return true;
        }
        
        private bool GetMediaFileID(Message mess)
        {
            if (mess == null) return false;

            if      (mess.Audio is { } a)
                FileID = a.FileId;
            else if (mess.Video is { } v)
                FileID = v.FileId;
            else if (mess.Animation is { } g)
                FileID = g.FileId;
            else if (mess.Sticker is { IsVideo: true } s)
            {
                FileID = s.FileId;
                Memes.PassSize(s);
            }
            else if (mess.Voice is { } c)
                FileID = c.FileId;
            else if (mess.VideoNote is { } n)
                FileID = n.FileId;
            else if (mess.Document is { MimeType: "audio/x-wav"} d)
                FileID = d.FileId;
            else return false;

            return true;
        }

        protected void SendResult(string result, MediaType type, Func<string> video, Func<string> audio)
        {
            using var stream = File.OpenRead(result);
            if      (type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, audio()));
            else if (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, video()));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, video()));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }
    }
}