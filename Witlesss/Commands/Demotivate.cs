using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;
using File = System.IO.File;

namespace Witlesss.Commands
{
    public class Demotivate : WitlessCommand
    {
        private readonly Regex _dg = new(@"^\/d[vg]\S* *", RegexOptions.IgnoreCase);
        
        public void SetMode(DgMode mode = DgMode.Square) => Bot.MemeService.Mode = mode;
        public void PassQuality(Witless witless) => DemotivatorDrawer.PassQuality(witless.JpgQuality);

        public override void Run()
        {
            var x = Message.ReplyToMessage;
            if (ProcessMessage(x) || ProcessMessage(Message)) return;

            Bot.SendMessage(Chat, DG_MANUAL);
        }

        private bool ProcessMessage(Message mess)
        {
            if (mess == null) return false;
            
            if      (mess.Photo != null)
                SendDemotivator(mess.Photo[^1].FileId);
            else if (mess.Sticker != null && !(mess.Sticker.IsVideo || mess.Sticker.IsAnimated))
                SendDemotivatedSticker(mess.Sticker.FileId);
            else if (mess.Animation != null)
                SendAnimatedDemotivator(mess.Animation.FileId);
            else if (mess.Sticker is { IsVideo: true })
                SendAnimatedDemotivator(mess.Sticker.FileId, ".webm");
            else if (mess.Video != null)
                SendAnimatedDemotivator(mess.Video.FileId);
            else return false;
            
            return true;
        }

        public void SendDemotivator(string fileID)
        {
            GetDemotivatorSources(fileID, ".jpg", out string a, out string b, out string path);
            using (var stream = File.OpenRead(Bot.MemeService.MakeDemotivator(path, a, b)))
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> DEMOTIVATOR [_]");
        }

        public void SendDemotivatedSticker(string fileID)
        {
            GetDemotivatorSources(fileID, ".webp", out string a, out string b, out string path);
            string extension = Text == null ? ".png" : Text.Contains("-j") ? ".jpg" : ".png";
            using (var stream = File.OpenRead(Bot.MemeService.MakeStickerDemotivator(path, a, b, extension)))
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> DEMOTIVATOR [#] STICKER");
        }

        private void SendAnimatedDemotivator(string fileID, string extension = ".mp4")
        {
            var time = DateTime.Now;
            GetDemotivatorSources(fileID, extension, out string a, out string b, out string path);
            string output = extension == ".mp4"
                ? Bot.MemeService.MakeVideoDemotivator(path, a, b)
                : Bot.MemeService.MakeVideoStickerDemotivator(path, a, b);
            using (var stream = File.OpenRead(output))
                Bot.SendAnimation(Chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
            Log($@"{Title} >> DEMOTIVATOR [^] VID >> TIME: {DateTime.Now - time:s\.fff}");
        }

        private void GetDemotivatorSources(string fileID, string extension, out string textA, out string textB, out string path)
        {
            GetDemotivatorText(Baka, RemoveDg(Text), out textA, out textB);
            path = $@"{PICTURES_FOLDER}\{ShortID(fileID)}{extension}";
            path = UniquePath(path, extension);
            Bot.DownloadFile(fileID, path, Chat).Wait();
        }

        private string RemoveDg(string text) => text == null ? null : _dg.Replace(text, "");
    }
}