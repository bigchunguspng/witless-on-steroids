using System;
using System.Drawing;
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

        protected override string Options => Baka.Meme.OptionsT;
        protected override string Command { get; } = "/top";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Подписанки", TOP_OPTIONS); // 🔥🔥🔥✍️

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.MakeCaptionMeme);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.MakeCaptionMemeFromSticker);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.MakeVideoCaptionMeme);

        protected override string GetMemeText(string text)
        {
            var caption = string.IsNullOrEmpty(text) ? Baka.Generate() : text;

            var dummy = GetDummy(out var empty);

            if (!empty && _colorXD.IsMatch(dummy))
            {
                IFunnyApp.UseGivenColor = true;
                MakeMeme.ParseColorOption(_colorXD, ref dummy, ref IFunnyApp.GivenColor, ref IFunnyApp.UseGivenColor);
            }

            IFunnyApp.ExtraFonts.CheckKey(empty, ref dummy);
            IFunnyApp.UseSegoe         = !empty &&  _segoe  .IsMatch(dummy);
            IFunnyApp.BackInBlack      = !empty &&  _blackBG.IsMatch(dummy);
            IFunnyApp.PickColor        = !empty &&  _colorPP.IsMatch(dummy);
            IFunnyApp.ForceCenter      = !empty &&  _colorFC.IsMatch(dummy);
            IFunnyApp.UseLeftAlignment = !empty &&  _left   .IsMatch(dummy);
            IFunnyApp.ThinCard         = !empty &&  _thin   .IsMatch(dummy);
            IFunnyApp.UltraThinCard    = !empty &&  _thinner.IsMatch(dummy);
            IFunnyApp.BlurImage        = !empty &&  _blur   .IsMatch(dummy);
            IFunnyApp.WrapText         =  empty || !_nowrap .IsMatch(dummy);

            if (empty || !IFunnyApp.UseSegoe && !ExtraFonts.UseOtherFont)
            {
                var cyrillic = IsMostlyCyrillic(caption);
                IFunnyApp.UseSegoe = cyrillic;
                if (!cyrillic)
                {
                    ExtraFonts.UseOtherFont = true;
                    ExtraFonts.OtherFontKey = "ft";
                }
            }

            IFunnyApp.CropPercent = !empty &&   _crop.IsMatch(dummy) ? GetInt(  _crop) : 100;
            IFunnyApp.MinFontSize = !empty && _fontMS.IsMatch(dummy) ? GetInt(_fontMS) :  10;
            IFunnyApp.DefFontSize = !empty && _fontSS.IsMatch(dummy) ? GetInt(_fontSS) :  36;

            return caption;

            int GetInt(Regex x) => int.Parse(x.Match(dummy).Groups[1].Value);
        }

        private static readonly Regex _segoe   = new(@"^\/top\S*sg\S*",            RegexOptions.IgnoreCase);
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