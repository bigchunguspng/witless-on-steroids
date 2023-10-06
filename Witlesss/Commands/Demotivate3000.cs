using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class Demotivate3000 : MakeMemeCore<string>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/dp(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"DEMOTIVATOR-B [{(x == 1 ? "_" : x)}]";

        protected override string Log_VIDEO { get; } = "DEMOTIVATOR-B [^] VID";
        protected override string Log_STICK { get; } = "DEMOTIVATOR-B [#] STICKER";
        protected override string VideoName { get; } = "piece_fap_club-dp.mp4";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);
            
            return this;
        }

        public override void Run() => Run("Демотиваторы-B");

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.MakeDemotivatorB);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.MakeStickerDemotivatorB);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.MakeVideoDemotivatorB);

        protected override string GetMemeText(string text)
        {
            var empty = Text is null && Baka.Meme.OptionsD is null;
            var input = Text is null ? "" : Text.Replace(Config.BOT_USERNAME, "");
            var cmd   = input.Split(split_chars, 2)[0].ToLower();
            var topxd = cmd.Length > 3 && cmd.StartsWith("/dp");
            var dummy = empty ? "" : topxd ? input : Baka.Meme.OptionsD ?? input;
            
            DynamicDemotivatorDrawer.UseGivenColor    = !empty &&  _colorXD.IsMatch(dummy);

            if (DynamicDemotivatorDrawer.UseGivenColor)
            {
                var c = _colorXD.Match(dummy).Groups[1].Value;
                dummy = dummy.Replace(c, "");
                if (c == c.ToLower() || c == c.ToUpper()) c = c.ToLetterCase(LetterCaseMode.Sentence);
                var b = Enum.IsDefined(typeof(KnownColor), c);
                if (b) DynamicDemotivatorDrawer.   GivenColor = Color.FromName(c);
                else   DynamicDemotivatorDrawer.UseGivenColor = false;
            }
            
            DynamicDemotivatorDrawer.UseRoboto   = !empty &&  _roboto.IsMatch(dummy);
            DynamicDemotivatorDrawer.UseImpact   = !empty &&  _impact.IsMatch(dummy);
            DynamicDemotivatorDrawer.UseBoldFont = !empty &&    _bold.IsMatch(dummy);
            DynamicDemotivatorDrawer.CropEdges   = !empty &&    _crop.IsMatch(dummy);
            
            var caps                             = !empty &&    _caps.IsMatch(dummy);

            var txt = string.IsNullOrEmpty(text) ? Baka.Generate() : text;

            return caps ? txt.ToLetterCase(LetterCaseMode.Upper) : txt;
        }

        private static readonly Regex _roboto  = new(@"^\/dp\S*rg\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _impact  = new(@"^\/dp\S*im\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _bold    = new(@"^\/dp\S*bb\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _crop    = new(@"^\/dp\S*cp\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _caps    = new(@"^\/dp\S*up\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _colorXD = new(@"^\/dp\S*#([A-Za-z]+)#\S* *", RegexOptions.IgnoreCase);
    }
}