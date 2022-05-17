using System;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Also;

namespace Witlesss.Commands
{
    public class ChangeSpeed : Command
    {
        public SpeedMode Mode;

        public override void Run()
        {
            string fileID = Bot.GetVideoOrAudioID(Message, Chat);
            if (fileID == null) return;

            var speed = 2D;
            if (Extension.HasDoubleArgument(Text, out double value))
                speed = Mode == SpeedMode.Fast ? Math.Clamp(value, 0.5, 94) : Math.Clamp(value, 0.0107, 2);

            string shortID = Extension.ShortID(fileID);
            string extension = Extension.ExtensionFromID(shortID);
            var type = Extension.MediaTypeFromID(shortID);
            var path = $@"{Environment.CurrentDirectory}\{Strings.PICTURES_FOLDER}\{shortID}{extension}";
            path = Extension.UniquePath(path, extension);
            Bot.DownloadFile(fileID, path, Chat).Wait();
                    
            string result = Bot.MemeService.ChangeSpeed(path, speed, Mode, type);
            using (var stream = File.OpenRead(result))
                switch (type)
                {
                    case MediaType.Audio:
                        Bot.SendAudio(Chat, new InputOnlineFile(stream, AudioFilename()));
                        break;
                    case MediaType.Video:
                        Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFilename()));
                        break;
                    case MediaType.AudioVideo:
                        Bot.SendVideo(Chat, new InputOnlineFile(stream, VideoFilename()));
                        break;
                }
            Logger.Log($"{Title} >> {(Mode == SpeedMode.Fast ? "FAST" : "SLOW" )} [>>]");

            string AudioFilename() => Message.Audio?.FileName ?? Message.Document?.FileName ?? $"Lmao, {Extension.ValidFileName(Extension.SenderName(Message))}.mp3";
            string VideoFilename() => $"piece_fap_club-{speed}.mp4";
        }
    }
}