using System.IO;
using System.Linq;
using Telegram.Bot.Types.InputFiles;
using static System.Environment;
using static Witlesss.Also.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class Reverse : Command
    {
        public override void Run()
        {
            string fileID = Bot.GetVideoOrAudioID(Message, Chat);
            if (fileID == null) return;
            
            string shortID = ShortID(fileID);
            string extension = ExtensionFromID(shortID);
            var type = MediaTypeFromID(shortID);
            var path = $@"{CurrentDirectory}\{PICTURES_FOLDER}\{shortID}{extension}";
            path = UniquePath(path, extension);
            Bot.DownloadFile(fileID, path, Chat).Wait();
            
            string result = Bot.MemeService.Reverse(path);
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
            Log($"{Title} >> REVERSED [<<]");

            string AudioFilename() => $"Kid Named {new string(ValidFileName(SenderName(Message)).Reverse().ToArray())}.mp3";
            string VideoFilename() => "piece_fap_club-R.mp4";
        }
    }
}