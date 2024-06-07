﻿using System.Text.RegularExpressions;
using static Witlesss.Backrooms.OptionsParsing;

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

            IFunnyApp.BackInBlack      =  CheckAndCut(Request, _blackBG);
            IFunnyApp.PickColor        =  CheckAndCut(Request, _colorPP);
            IFunnyApp.ForceCenter      =  CheckAndCut(Request, _colorFC);
            IFunnyApp.UseLeftAlignment =  CheckAndCut(Request, _left   );
            IFunnyApp.ThinCard         =  CheckAndCut(Request, _thin   );
            IFunnyApp.UltraThinCard    =  CheckAndCut(Request, _thinner);
            IFunnyApp.BlurImage        =  CheckAndCut(Request, _blur   );
            IFunnyApp.WrapText         = !CheckAndCut(Request, _nowrap );

            IFunnyApp.CropPercent        = GetInt(Request, _crop,  100);
            IFunnyApp.MinFontSize        = GetInt(Request, _fontMS, 10);
            IFunnyApp.FontSizeMultiplier = GetInt(Request, _fontSM, 10);

            IFunnyApp.ExtraFonts.CheckAndCut(Request);
        }

        protected override string GetMemeText(string? text)
        {
            var caption = string.IsNullOrEmpty(text) ? Baka.Generate() : text;

            IFunnyApp.PreferSegoe = IsMostlyCyrillic(caption);

            return caption;
        }

        private static readonly Regex _left    = new(@"^\/top\S*(la)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _blur    = new(@"^\/top\S*(blur)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex _thin    = new(@"^\/top\S*(mm)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _thinner = new(@"^\/top\S*(mmm)\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex _nowrap  = new(@"^\/top\S*(ww)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _colorPP = new(@"^\/top\S*(pp)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _colorFC = new(@"^\/top\S*(fc)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _blackBG = new(@"^\/top\S*(bbg)\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex _colorXD = new(@"^\/top\S*#([A-Za-z]+)#\S*",     RegexOptions.IgnoreCase);
        private static readonly Regex _crop    = new(@"^\/top\S*?(-?\d{1,2})(%)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _fontSM  = new(@"^\/top\S*?(\d{1,3})("")\S*",    RegexOptions.IgnoreCase);
        private static readonly Regex _fontMS  = new(@"^\/top\S*?(\d{1,3})(""ms)\S*",  RegexOptions.IgnoreCase);
    }
}