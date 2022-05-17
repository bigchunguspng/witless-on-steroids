using System;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Also;

namespace Witlesss.Commands
{
    public class Demotivate : WitlessCommand
    {
        public void SetMode(DgMode mode = DgMode.Square) => Bot.MemeService.Mode = mode;

        public override void Run()
        {
            string fileID;
            if (Message.Photo != null)
                fileID = Message.Photo[^1].FileId;
            else if (Message.ReplyToMessage?.Photo != null)
                fileID = Message.ReplyToMessage.Photo[^1].FileId;
            else
            {
                if (Message.ReplyToMessage?.Animation != null)
                    fileID = Message.ReplyToMessage.Animation.FileId;
                else if (Message.Animation != null)
                    fileID = Message.Animation.FileId;
                else if (Message.ReplyToMessage?.Video != null)
                    fileID = Message.ReplyToMessage.Video.FileId;
                else if (Message.Video != null)
                    fileID = Message.Video.FileId;
                else
                {
                    if (Message.ReplyToMessage?.Sticker != null && Message.ReplyToMessage.Sticker.IsVideo)
                        fileID = Message.ReplyToMessage.Sticker.FileId;
                    else
                    {
                        if (Message.ReplyToMessage?.Sticker != null &&
                            Message.ReplyToMessage.Sticker.IsAnimated == false)
                            fileID = Message.ReplyToMessage.Sticker.FileId;
                        else
                        {
                            Bot.SendMessage(Chat, Strings.DG_MANUAL);
                            return;
                        }

                        SendDemotivatedSticker(fileID);
                        return;
                    }

                    SendAnimatedDemotivator(fileID, ".webm");
                    return;
                }

                SendAnimatedDemotivator(fileID);
                return;
            }

            SendDemotivator(fileID);
        }

        public void SendDemotivator(string fileID)
        {
            GetDemotivatorSources(fileID, ".jpg", out string a, out string b, out string path);
            using (var stream = File.OpenRead(Bot.MemeService.MakeDemotivator(path, a, b)))
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Logger.Log($"{Title} >> DEMOTIVATOR [_]");
        }

        public void SendDemotivatedSticker(string fileID)
        {
            GetDemotivatorSources(fileID, ".webp", out string a, out string b, out string path);
            string extension = Text == null ? ".png" :
                Text.Contains("-j") ? ".jpg" : ".png";
            using (var stream = File.OpenRead(Bot.MemeService.MakeStickerDemotivator(path, a, b, extension)))
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Logger.Log($"{Title} >> DEMOTIVATOR [#] STICKER");
        }

        private void SendAnimatedDemotivator(string fileID, string extension = ".mp4")
        {
            var time = DateTime.Now;
            GetDemotivatorSources(fileID, extension, out string a, out string b, out string path);
            string output = extension == ".mp4"
                ? Bot.MemeService.MakeAnimatedDemotivator(path, a, b)
                : Bot.MemeService.MakeVideoStickerDemotivator(path, a, b);
            using (var stream = File.OpenRead(output))
                Bot.SendAnimation(Chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
            Logger.Log($@"{Title} >> DEMOTIVATOR [^] VID >> TIME: {DateTime.Now - time:s\.fff}");
        }

        private void GetDemotivatorSources(string fileID, string extension, out string textA, out string textB, out string path)
        {
            Extension.GetDemotivatorText(Baka, Text, out textA, out textB);
            path = $@"{Environment.CurrentDirectory}\{Strings.PICTURES_FOLDER}\{Extension.ShortID(fileID)}{extension}";
            path = Extension.UniquePath(path, extension);
            Bot.DownloadFile(fileID, path).Wait();
        }
    }
}