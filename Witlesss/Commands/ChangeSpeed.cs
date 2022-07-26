using System;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using static System.Environment;
using static Witlesss.Also.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

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
            if (HasDoubleArgument(Text, out double value))
                speed = Mode == SpeedMode.Fast ? Math.Clamp(value, 0.5, 94) : Math.Clamp(value, 0.0107, 2);

            string shortID = ShortID(fileID);
            string extension = ExtensionFromID(shortID);
            var type = MediaTypeFromID(shortID);
            var path = $@"{CurrentDirectory}\{PICTURES_FOLDER}\{shortID}{extension}";
            path = UniquePath(path, extension);
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
            Log($"{Title} >> {(Mode == SpeedMode.Fast ? "FAST" : "SLOW" )} [>>]");

            string AudioFilename() => Message.Audio?.FileName ?? Message.Document?.FileName ?? $"Lmao, {ValidFileName(SenderName(Message))}.mp3";
            string VideoFilename() => $"piece_fap_club-{speed}.mp4";
        }
    }
}