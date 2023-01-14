using System;
using System.Drawing;
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

        protected string Sender => ValidFileName(SenderName(Message));

        protected bool NothingToProcess()
        {
            if (GetMediaFileID(Message.ReplyToMessage) || GetMediaFileID(Message)) return false;
            
            Bot.SendMessage(Chat, DAMN_MANUAL);
            return true;
        }
        
        private bool GetMediaFileID(Message mess)
        {
            if (mess == null) return false;

            if      (mess.Audio != null)
                FileID = mess.Audio.FileId;
            else if (mess.Video != null)
                FileID = mess.Video.FileId;
            else if (mess.Animation != null)
                FileID = mess.Animation.FileId;
            else if (mess.Sticker is { IsVideo: true })
            {
                FileID = mess.Sticker.FileId;
                Memes.SourceSize = new Size(mess.Sticker.Width, mess.Sticker.Height);
            }
            else if (mess.Voice != null)
                FileID = mess.Voice.FileId;
            else if (mess.Document?.MimeType?.StartsWith("audio") == true)
                FileID = mess.Document.FileId;
            else return false;

            return true;
        }

        protected void SendResult(string result, MediaType type, Func<string> video, Func<string> audio)
        {
            using var stream = File.OpenRead(result);
            if      (type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, audio()));
            else if (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, video()));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, video()));
        }
    }
}