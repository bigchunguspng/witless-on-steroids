﻿using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class AddCaption : MakeMemeCore<string>, ImageProcessor
    {
        public AddCaption() : base(new Regex(@"^\/top(\S*) *", RegexOptions.IgnoreCase)) { }

        protected override string Log_PHOTO(int x) => $"WHENTHE [{(x == 1 ? "=" : x)}]";

        protected override string Log_VIDEO { get; } = "WHENTHE [%] VID";
        protected override string Log_STICK { get; } = "WHENTHE [$] STICKER";
        protected override string VideoName => $"piece_fap_club-top-{IFunnyApp.FontSize}.mp4";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Подписанки"); // 🔥🔥🔥✍️

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.MakeCaptionMeme);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.MakeCaptionMemeFromSticker);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.MakeVideoCaptionMeme);

        protected override string GetMemeText(string text)
        {
            var empty = string.IsNullOrEmpty(Text);
            var dummy = empty ? "" : Text.Replace(Config.BOT_USERNAME, "");

            IFunnyApp.UseGivenColor    = !empty &&  _colorXD.IsMatch(dummy);

            if (IFunnyApp.UseGivenColor)
            {
                var c = _colorXD.Match(dummy).Groups[1].Value;
                dummy = dummy.Replace(c, "");
                if (c == c.ToLower() || c == c.ToUpper()) c = c.ToLetterCase(LetterCaseMode.Sentence);
                var b = Enum.IsDefined(typeof(KnownColor), c);
                if (b) IFunnyApp.   GivenColor = Color.FromName(c);
                else   IFunnyApp.UseGivenColor = false;
            }

            IFunnyApp.BackInBlack      = !empty &&  _blackBG.IsMatch(dummy);
            IFunnyApp.PickColor        = !empty &&  _colorPP.IsMatch(dummy);
            IFunnyApp.UseRegularFont   = !empty &&  _regular.IsMatch(dummy);
            IFunnyApp.UseLeftAlignment = !empty &&  _left   .IsMatch(dummy);
            IFunnyApp.MinimizeHeight   = !empty &&  _height .IsMatch(dummy);
            IFunnyApp.WrapText         =  empty || !_nowrap .IsMatch(dummy);

            IFunnyApp.CropPercent = !empty && _crop.IsMatch(Text) ? int.Parse(_crop.Match(Text).Groups[1].Value) : 100;
            IFunnyApp.MinFontSize = !empty && _font.IsMatch(Text) ? int.Parse(_font.Match(Text).Groups[1].Value) : 10;

            return string.IsNullOrEmpty(text) ? Baka.Generate() : text;
        }

        private static readonly Regex _regular = new(@"^\/top\S*rg\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _left    = new(@"^\/top\S*la\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _height  = new(@"^\/top\S*mm\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _nowrap  = new(@"^\/top\S*ww\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _colorPP = new(@"^\/top\S*pp\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _blackBG = new(@"^\/top\S*bb\S* *",            RegexOptions.IgnoreCase);
        private static readonly Regex _colorXD = new(@"^\/top\S*#([A-Za-z]+)#\S* *", RegexOptions.IgnoreCase);
        private static readonly Regex _crop    = new(@"^\/top\S*?(-?\d{1,2})%\S* *", RegexOptions.IgnoreCase);
        private static readonly Regex _font    = new(@"^\/top\S*?ms(\d{1,3})\S* *",  RegexOptions.IgnoreCase);
    }
}