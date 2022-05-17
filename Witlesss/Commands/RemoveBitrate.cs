using System;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Also;

namespace Witlesss.Commands
{
    public class RemoveBitrate : Command
    {
        public override void Run()
        {
            string fileID = Bot.GetVideoOrAudioID(Message, Chat);
            if (fileID == null) return;

            var bitrate = 0;
            if (Extension.HasIntArgument(Text, out int value))
                bitrate = value;

            string shortID = Extension.ShortID(fileID);
            string extension = Extension.ExtensionFromID(shortID);
            var path = $@"{Environment.CurrentDirectory}\{Strings.PICTURES_FOLDER}\{shortID}{extension}";
            path = Extension.UniquePath(path, extension);
            Bot.DownloadFile(fileID, path, Chat).Wait();

            string result = Bot.MemeService.RemoveBitrate(path, bitrate, out value);
            extension = Extension.GetFileExtension(result);
            using (var stream = File.OpenRead(result))
                switch (extension)
                {
                    case ".mp4":
                        if (shortID.StartsWith("BA")) 
                            Bot.SendVideo(Chat, new InputOnlineFile(stream, VideoFilename()));
                        else
                            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFilename()));
                        break;
                    case ".mp3":
                        Bot.SendAudio(Chat, new InputOnlineFile(stream, $"Damn, {Extension.ValidFileName(Extension.SenderName(Message))}.mp3"));
                        break;
                }
            Logger.Log($"{Title} >> DAMN [*]");
                    
            string VideoFilename() => $"piece_fap_club-{value}.mp4";
        }
    }
}