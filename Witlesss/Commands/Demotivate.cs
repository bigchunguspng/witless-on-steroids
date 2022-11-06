using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands
{
    public class Demotivate : WitlessCommand
    {
        private readonly Regex _dg = new(@"^\/d[vg]\S* *", RegexOptions.IgnoreCase);
        
        public  void SelectModeAuto(float w, float h) => SetMode(w / h > 1.6 ? DgMode.Wide : DgMode.Square);
        private void SetMode(DgMode mode = DgMode.Square) => Bot.MemeService.Mode = mode;
        public  void PassQuality(Witless witless) => DemotivatorDrawer.PassQuality(witless.JpgQuality);

        public Demotivate SetUp(Witless witless, DgMode mode)
        {
            SetMode(mode);
            PassQuality(witless);

            return this;
        }

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
            else if (mess.Animation != null)
                SendAnimatedDemotivator(mess.Animation.FileId);
            else if (mess.Sticker is { IsVideo: true })
                SendAnimatedDemotivator(mess.Sticker.FileId, ".webm");
            else if (mess.Video != null)
                SendAnimatedDemotivator(mess.Video.FileId);
            else if (mess.Sticker is { IsAnimated: false })
                SendDemotivatedSticker(mess.Sticker.FileId);
            else return false;
            
            return true;
        }

        public void SendDemotivator(string fileID)
        {
            GetSources(fileID, ".jpg", out var text, out var path);
            using (var stream = File.OpenRead(Bot.MemeService.MakeDemotivator(path, text)))
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> DEMOTIVATOR [_]");
        }

        public void SendDemotivatedSticker(string fileID)
        {
            GetSources(fileID, ".webp", out var text, out var path);
            string extension = Text == null ? ".png" : Text.Contains("-j") ? ".jpg" : ".png";
            using (var stream = File.OpenRead(Bot.MemeService.MakeStickerDemotivator(path, text, extension)))
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> DEMOTIVATOR [#] STICKER");
        }

        private void SendAnimatedDemotivator(string fileID, string extension = ".mp4")
        {
            var time = DateTime.Now;
            GetSources(fileID, extension, out var text, out var path);
            string output = extension == ".mp4"
                ? Bot.MemeService.MakeVideoDemotivator       (path, text)
                : Bot.MemeService.MakeVideoStickerDemotivator(path, text);
            using (var stream = File.OpenRead(output))
                Bot.SendAnimation(Chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
            Log($@"{Title} >> DEMOTIVATOR [^] VID >> TIME: {DateTime.Now - time:s\.fff}");
        }

        private void GetSources(string fileID, string extension, out DgText text, out string path)
        {
            GetDemotivatorText(Baka, RemoveDg(Text), out text);
            path = UniquePath($@"{PICTURES_FOLDER}\{ShortID(fileID)}{extension}");
            Bot.DownloadFile(fileID, path, Chat).Wait();
        }

        private string RemoveDg(string text) => text == null ? null : _dg.Replace(text, "");
    }
}