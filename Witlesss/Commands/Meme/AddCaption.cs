using System.Text.RegularExpressions;

namespace Witlesss.Commands.Meme
{
    public class AddCaption : MakeMemeCore<string>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/top(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"WHENTHE [{(x == 1 ? "=" : x)}]";
        protected override string Log_STICK(int x) => $"WHENTHE [{(x == 1 ? "$" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "WHENTHE [%] VID";
        protected override string VideoName => $"piece_fap_club-top-{IFunnyApp.FontSize}.mp4";

        protected override string Command { get; } = "/top";

        protected override string? DefaultOptions => Baka.Meme.OptionsT;

        public ImageProcessor SetUp(int w, int h)
        {
            ImageSaver.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Подписанки", OPTIONS + "/op_top"); // 🔥🔥🔥✍️

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.MakeCaptionMeme);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.MakeCaptionMemeFromSticker);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.MakeVideoCaptionMeme);

        protected override void ParseOptions()
        {
            /*IFunnyApp.UseGivenColor = !empty && _colorXD.IsMatch(dummy);
            if (IFunnyApp.UseGivenColor)
            {
                MakeMeme.ParseColorOption(_colorXD, ref dummy, ref IFunnyApp.GivenColor, ref IFunnyApp.UseGivenColor);
            }*/

            IFunnyApp.ExtraFonts.CheckKey(Request.Empty, ref Request.Dummy);
            IFunnyApp.BackInBlack      = !Request.Empty &&  _blackBG.IsMatch(Request.Dummy);
            IFunnyApp.PickColor        = !Request.Empty &&  _colorPP.IsMatch(Request.Dummy);
            IFunnyApp.ForceCenter      = !Request.Empty &&  _colorFC.IsMatch(Request.Dummy);
            IFunnyApp.UseLeftAlignment = !Request.Empty &&  _left   .IsMatch(Request.Dummy);
            IFunnyApp.ThinCard         = !Request.Empty &&  _thin   .IsMatch(Request.Dummy);
            IFunnyApp.UltraThinCard    = !Request.Empty &&  _thinner.IsMatch(Request.Dummy);
            IFunnyApp.BlurImage        = !Request.Empty &&  _blur   .IsMatch(Request.Dummy);
            IFunnyApp.WrapText         =  Request.Empty || !_nowrap .IsMatch(Request.Dummy);

            IFunnyApp.CropPercent = !Request.Empty &&   _crop.IsMatch(Request.Dummy) ? GetInt(  _crop) : 100;
            IFunnyApp.MinFontSize = !Request.Empty && _fontMS.IsMatch(Request.Dummy) ? GetInt(_fontMS) :  10;
            IFunnyApp.DefFontSize = !Request.Empty && _fontSS.IsMatch(Request.Dummy) ? GetInt(_fontSS) :  48;
        }

        protected override string GetMemeText(string? text)
        {
            var caption = string.IsNullOrEmpty(text) ? Baka.Generate() : text;

            IFunnyApp.PreferSegoe = IsMostlyCyrillic(caption);
            /*
            if (Request.Empty || !IFunnyApp.UseSegoe && !ExtraFonts.UseOtherFont)
            {
                var cyrillic = IsMostlyCyrillic(caption);
                IFunnyApp.UseSegoe = cyrillic;
                if (!cyrillic)
                {
                    ExtraFonts.UseOtherFont = true;
                    ExtraFonts.OtherFontKey = "ft";
                }
            }
            */

            return caption;
        }

        private int GetInt(Regex x) => int.Parse(x.Match(Request.Dummy).Groups[1].Value);

        private static readonly Regex _left    = new(@"^\/top\S*la\S*",            RegexOptions.IgnoreCase);
        private static readonly Regex _blur    = new(@"^\/top\S*blur\S*",          RegexOptions.IgnoreCase);
        private static readonly Regex _thin    = new(@"^\/top\S*mm\S*",            RegexOptions.IgnoreCase);
        private static readonly Regex _thinner = new(@"^\/top\S*mmm\S*",           RegexOptions.IgnoreCase);
        private static readonly Regex _nowrap  = new(@"^\/top\S*ww\S*",            RegexOptions.IgnoreCase);
        private static readonly Regex _colorPP = new(@"^\/top\S*pp\S*",            RegexOptions.IgnoreCase);
        private static readonly Regex _colorFC = new(@"^\/top\S*fc\S*",            RegexOptions.IgnoreCase);
        private static readonly Regex _blackBG = new(@"^\/top\S*bb\S*",            RegexOptions.IgnoreCase);
        private static readonly Regex _colorXD = new(@"^\/top\S*#([A-Za-z]+)#\S*", RegexOptions.IgnoreCase);
        private static readonly Regex _crop    = new(@"^\/top\S*?(-?\d{1,2})%\S*", RegexOptions.IgnoreCase);
        private static readonly Regex _fontMS  = new(@"^\/top\S*?ms(\d{1,3})\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex _fontSS  = new(@"^\/top\S*?ss(\d{1,3})\S*",  RegexOptions.IgnoreCase);
    }
}