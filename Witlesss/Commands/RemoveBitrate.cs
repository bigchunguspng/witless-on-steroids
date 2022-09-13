using System;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using static System.Environment;
using static Witlesss.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class RemoveBitrate : Command
    {
        public override void Run()
        {
            string fileID = GetVideoOrAudioID();
            if (fileID == null) return;

            var bitrate = 0;
            if (HasIntArgument(Text, out int value))
                bitrate = value;

            Download(fileID, out string path, out var type);

            string result = Bot.MemeService.RemoveBitrate(path, bitrate, out value);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> DAMN [*]");

            string AudioFilename() => MediaFileName($"Damn, {Sender()}.mp3");
            string VideoFilename() => $"piece_fap_club-{value}.mp4";
        }

        protected string MediaFileName(string s) => Message.Audio?.FileName ?? Message.Document?.FileName ?? s;
        protected string Sender() => ValidFileName(SenderName(Message));

        protected string GetVideoOrAudioID()
        {
            var fileID = "";
            var mess = Message.ReplyToMessage ?? Message;
            for (int cycle = Message.ReplyToMessage != null ? 0 : 1; cycle < 2; cycle++)
            {
                if      (mess.Audio != null)
                    fileID = mess.Audio.FileId;
                else if (mess.Video != null)
                    fileID = mess.Video.FileId;
                else if (mess.Animation != null)
                    fileID = mess.Animation.FileId;
                else if (mess.Sticker != null && mess.Sticker.IsVideo)
                    fileID = mess.Sticker.FileId;
                else if (mess.Voice != null)
                    fileID = mess.Voice.FileId;
                else if (mess.Document?.MimeType != null && mess.Document.MimeType.StartsWith("audio"))
                    fileID = mess.Document.FileId;
                
                if (fileID.Length > 0)
                    break;
                else if (cycle == 1)
                {
                    Bot.SendMessage(Chat, DAMN_MANUAL);
                    return null;
                }
                else mess = Message;
            }
            return fileID;
        }

        protected void Download(string fileID, out string path, out MediaType type)
        {
            string shortID = ShortID(fileID);
            string extension = ExtensionFromID(shortID);
            type = MediaTypeFromID(shortID);
            path = $@"{CurrentDirectory}\{PICTURES_FOLDER}\{shortID}{extension}";
            path = UniquePath(path, extension);
            Bot.DownloadFile(fileID, path, Chat).Wait();
        }

        protected void SendResult(string result, MediaType type, Func<string> video, Func<string> audio)
        {
            using var stream = File.OpenRead(result);
            switch (type)
            {
                case MediaType.Audio:
                    Bot.SendAudio(Chat, new InputOnlineFile(stream, audio()));
                    break;
                case MediaType.Video:
                    Bot.SendAnimation(Chat, new InputOnlineFile(stream, video()));
                    break;
                case MediaType.AudioVideo:
                    Bot.SendVideo(Chat, new InputOnlineFile(stream, video()));
                    break;
            }
        }
    }
}