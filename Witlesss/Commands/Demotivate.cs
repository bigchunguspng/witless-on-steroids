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
            else if (mess.Animation is { Duration: < 21 })
                SendDemotivatedVideo(mess.Animation.FileId);
            else if (mess.Sticker is { IsVideo: true })
                SendDemotivatedVideo(mess.Sticker.FileId, ".webm");
            else if (mess.Video is { Duration: < 21 })
                SendDemotivatedVideo(mess.Video.FileId);
            else if (mess.Sticker is { IsAnimated: false })
                SendDemotivatedSticker(mess.Sticker.FileId);
            else return false;
            
            return true;
        }

        public void SendDemotivator(string fileID)
        {
            var repeats = 1;
            var path = GetSource(fileID, ".jpg");
            if (Text != null)
            {
                var match = Regex.Match(Text.Split()[0], @"\d");
                if (match.Success && int.TryParse(match.Value, out int x)) repeats = x;
            }
            for (int i = 0; i < repeats; i++)
            {
                using var stream = File.OpenRead(Bot.MemeService.MakeDemotivator(path, Texts()));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> DEMOTIVATOR [{(repeats == 1 ? "_" : repeats)}]");
        }

        public void SendDemotivatedSticker(string fileID)
        {
            var path = GetSource(fileID, ".webp");
            var extension = ".png";
            if (Text != null && Text.Contains('x')) 
                extension = ".jpg";
            using var stream = File.OpenRead(Bot.MemeService.MakeStickerDemotivator(path, Texts(), extension));
            Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> DEMOTIVATOR [#] STICKER");
        }

        private void SendDemotivatedVideo(string fileID, string extension = ".mp4")
        {
            var time = DateTime.Now;
            var path = GetSource(fileID, extension);
            using var stream = File.OpenRead(Bot.MemeService.MakeVideoDemotivator(path, Texts()));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
            Log($@"{Title} >> DEMOTIVATOR [^] VID >> TIME: {DateTime.Now - time:s\.fff}");
        }

        private string GetSource(string fileID, string extension)
        {
            var path = UniquePath($@"{PICTURES_FOLDER}\{ShortID(fileID)}{extension}");
            Bot.DownloadFile(fileID, path, Chat).Wait();
            return path;
        }
        
        private DgText Texts() => GetDemotivatorText(Baka, RemoveDg(Text));

        private string RemoveDg(string text) => text == null ? null : _dg.Replace(text, "");
    }
}