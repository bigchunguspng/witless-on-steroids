﻿using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;
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

        protected override string? DefaultOptions => Baka.Meme.Options?.Top;

        public ImageProcessor SetUp(int w, int h)
        {
            ImageSaver.PassQuality(Baka);

            return this;
        }

        protected override Task Run() => RunInternal("Подписанки", OPTIONS + "/op_top"); // 🔥🔥🔥✍️

        protected override void ParseOptions()
        {
            IFunnyApp.CustomColorOption.CheckAndCut(Request);

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
        private static readonly Regex _thin    = new(@"^\/top\S*m(m)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _thinner = new(@"^\/top\S*(mm)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _nowrap  = new(@"^\/top\S*(ww)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _colorPP = new(@"^\/top\S*(pp)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _colorFC = new(@"^\/top\S*(fc)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _blackBG = new(@"^\/top\S*(bbg)\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex _crop    = new(@"^\/top\S*?(-?\d{1,2})(%)\S*",   RegexOptions.IgnoreCase);
        private static readonly Regex _fontSM  = new(@"^\/top\S*?(\d{1,3})("")\S*",    RegexOptions.IgnoreCase);
        private static readonly Regex _fontMS  = new(@"^\/top\S*?(\d{1,3})(""ms)\S*",  RegexOptions.IgnoreCase);

        // LOGIC

        private static readonly IFunnyApp _ifunny = new();
        private static readonly SerialTaskQueue _queue = new();
        
        protected override Task<string> MakeMemeImage(string path, string text)
        {
            return _queue.Enqueue(() => _ifunny.MakeCaptionMeme(path, text));
        }

        protected override Task<string> MakeMemeStick(string path, string text, string extension)
        {
            //return MakeCaptionMeme(Convert(path, extension), text);
            return MakeMemeImage(path, text);
        }

        protected override Task<string> MakeMemeVideo(string path, string text)
        {
            return _queue.Enqueue(() =>
            {
                var size = SizeHelpers.GetImageSize_FFmpeg(path).GrowSize();
                _ifunny.SetUp(size);

                if      (IFunnyApp.CustomColorOption.IsActive) _ifunny.SetCustomColors();
                else if (IFunnyApp.PickColor) _ifunny.SetSpecialColors(Image.Load<Rgba32>(Memes.Snapshot(path)));
                else                          _ifunny.SetDefaultColors();

                return new F_Combine(path, _ifunny.BakeText(text))
                    .When(Memes.Quality, size, _ifunny.Cropping, _ifunny.Location, IFunnyApp.BlurImage)
                    .Output("-Top");
            });
        }
    }
}