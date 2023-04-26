using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class AddCaption : MakeMemeCore<string>, ImageProcessor
    {
        public AddCaption() : base(new Regex(@"^\/top\S* *", RegexOptions.IgnoreCase)) { }
    
        private static bool REPEAT_RX() => Text is not null && Regex.IsMatch(Text, @"^\/top\S*\d+\S*");
        private static string C_PHOTO(int x) => $"WHENTHE [{(x == 1 ? "_" : x)}]";

        private const string C_VIDEO = "WHENTHE [^] VID";
        private const string C_STICK = "WHENTHE [#] STICKER";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Подписанки"); // 🔥🔥🔥✍️

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, C_PHOTO, M.MakeWhenTheMeme, REPEAT_RX());
        public    override void ProcessStick(string fileID) => DoStick(fileID, C_STICK, M.MakeWhenTheMemeFromSticker);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, C_VIDEO, M.MakeVideoWhenTheMeme);

        protected override string GetMemeText(string text)
        {
            var empty = string.IsNullOrEmpty(Text);
            
            IFunnyApp.UseRegularFont   = !empty &&  _regular.IsMatch(Text);
            IFunnyApp.UseLeftAlignment = !empty &&  _left   .IsMatch(Text);
            IFunnyApp.MinimizeHeight   = !empty &&  _height .IsMatch(Text);
            IFunnyApp.WrapText         =  empty || !_nowrap .IsMatch(Text);
            
            return string.IsNullOrEmpty(text) ? Baka.Generate() : text;
        }

        private static readonly Regex _regular = new(@"^\/top\S*rg\S* *", RegexOptions.IgnoreCase);
        private static readonly Regex _left    = new(@"^\/top\S*la\S* *", RegexOptions.IgnoreCase);
        private static readonly Regex _height  = new(@"^\/top\S*mm\S* *", RegexOptions.IgnoreCase);
        private static readonly Regex _nowrap  = new(@"^\/top\S*ww\S* *", RegexOptions.IgnoreCase);
    }
}