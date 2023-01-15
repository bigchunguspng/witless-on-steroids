using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.JpegCoder;

namespace Witlesss.Commands
{
    public class Demotivate : WitlessCommand, ImageProcessor
    {
        private readonly Regex _dg = new(@"^\/d[vg]\S* *", RegexOptions.IgnoreCase);
        
        public ImageProcessor SetUp(Message message, Witless witless, int w, int h)
        {
            Pass(message);
            Pass(witless);
            SelectModeAuto(w, h);
            PassQuality(witless);
            
            return this;
        }

        private void SelectModeAuto(float w, float h) => SetMode(w / h > 1.6 ? DgMode.Wide : DgMode.Square);
        private void SetMode(DgMode mode = DgMode.Square) => Bot.MemeService.Mode = mode;

        public Demotivate SetUp(DgMode mode)
        {
            SetMode(mode);

            return this;
        }

        public override void Run()
        {
            PassQuality(Baka);
            
            var x = Message.ReplyToMessage;
            if (ProcessMessage(Message) || ProcessMessage(x)) return;

            Bot.SendMessage(Chat, DG_MANUAL);
        }

        private bool ProcessMessage(Message mess)
        {
            if (mess == null) return false;
            
            if      (mess.Photo != null)
                ProcessPhoto(mess.Photo[^1].FileId);
            else if (mess.Animation is { })
                ProcessVideo(mess.Animation.FileId);
            else if (mess.Sticker is { IsVideo: true })
                ProcessVideo(mess.Sticker.FileId);
            else if (mess.Video is { })
                ProcessVideo(mess.Video.FileId);
            else if (mess.Sticker is { IsAnimated: false })
                ProcessSticker(mess.Sticker.FileId);
            else return false;
            
            return true;
        }

        public void ProcessPhoto(string fileID)
        {
            var repeats = 1;
            Bot.Download(fileID, Chat, out string path);
            if (Text != null && Regex.IsMatch(Text, @"^\/d[vg]\S*\d+\S*"))
            {
                var match = Regex.Match(Text, @"\d");
                if (match.Success && int.TryParse(match.Value, out int x)) repeats = x;
            }
            for (int i = 0; i < repeats; i++)
            {
                using var stream = File.OpenRead(Bot.MemeService.MakeDemotivator(path, Texts()));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> DEMOTIVATOR [{(repeats == 1 ? "_" : repeats)}]");
        }

        public void ProcessSticker(string fileID)
        {
            Bot.Download(fileID, Chat, out string path);
            var extension = ".png";
            if (Text != null && Text.Contains('x')) 
                extension = ".jpg";
            using var stream = File.OpenRead(Bot.MemeService.MakeStickerDemotivator(path, Texts(), extension));
            Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> DEMOTIVATOR [#] STICKER");
        }

        private void ProcessVideo(string fileID)
        {
            if (Bot.ChatIsBanned(Baka)) return;
            
            var time = DateTime.Now;
            Bot.Download(fileID, Chat, out string path);
            using var stream = File.OpenRead(Bot.MemeService.MakeVideoDemotivator(path, Texts()));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
            Log($@"{Title} >> DEMOTIVATOR [^] VID >> TIME: {DateTime.Now - time:s\.fff}");
        }

        private DgText Texts() => GetDemotivatorText(RemoveDg(Text));

        private string RemoveDg(string text) => text == null ? null : _dg.Replace(text, "");

        private DgText GetDemotivatorText(string text)
        {
            string a, b = Baka.TryToGenerate();
            if (b.Length > 1) b = b[0] + b[1..].ToLower(); // lower text can't be UPPERCASE
            if (string.IsNullOrEmpty(text)) a = Baka.TryToGenerate();
            else
            {
                var s = text.Split('\n', 2);
                a = s[0];
                if (s.Length > 1) b = s[1];
            }
            return new DgText(a, b);
        }
    }
}