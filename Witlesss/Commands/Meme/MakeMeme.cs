﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;
using static Witlesss.Backrooms.OptionsParsing;

namespace Witlesss.Commands.Meme
{
    public class MakeMeme : MakeMemeCore<DgText>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/meme(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"MEME [{(x == 1 ? "M" : x)}]";
        protected override string Log_STICK(int x) => $"MEME [{(x == 1 ? "M" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "MEME [M] VID";
        protected override string VideoName { get; } = "piece_fap_club-meme.mp4";

        protected override string Command { get; } = "/meme";

        protected override string? DefaultOptions => Baka.Meme.OptionsM;

        public ImageProcessor SetUp(int w, int h)
        {
            ImageSaver.PassQuality(Baka);

            return this;
        }

        protected override Task Run() => RunInternal("Мемы", OPTIONS + "/op_meme");

        protected override void ParseOptions()
        {
            MemeGenerator.CustomColorOption.CheckAndCut(Request);
            MemeGenerator.ExtraFonts.CheckAndCut(Request);

            MemeGenerator.FontMultiplier =            GetInt(Request, _fontSM, 10);
            MemeGenerator.ShadowOpacity  = Math.Clamp(GetInt(Request, _shadow, 100), 0, 100);
            MemeGenerator.WrapText       =      !CheckAndCut(Request, _nowrap);
            MemeGenerator.ColorText      =       CheckAndCut(Request, _colorText);
        }

        protected override DgText GetMemeText(string? text)
        {
            var generate = text.IsNullOrEmpty();
            var capitalize = Check(Request, _caps) && (generate || _caps.IsMatch(Request.Command));

            var add_bottom_text  = Check(Request,  _add_bottom);
            var only_bottom_text = Check(Request, _only_bottom);
            var only_top_text    = Check(Request,    _top_only);

            string a, b;

            if (generate)
            {
                var (genA, genB) = (true, true);

                var chance = Random.Shared.Next(10);
                if      (only_top_text)                   genB = false;
                else if (only_bottom_text || chance == 0) genA = false; // 1/10 >> bottom only

                a = genA ? Baka.Generate() : "";
                b = genB ? Baka.Generate() : "";

                if (genA && genB && a.Length > 25) // upper text is too big
                {
                    (a, b) = chance > 5
                        ? ("", a)  // 4/10 >> bottom only
                        : (a, ""); // 5/10 >> top    only
                }
            }
            else
            {
                if (text!.Contains('\n'))
                {
                    var separator = text.Contains("\n\n") ? "\n\n" : "\n";
                    var s = text.Split(separator, 2);
                    (a, b) = (s[0], s[1]);
                }
                else
                {
                    a = text;
                    b = add_bottom_text ? Baka.Generate() : "";
                }
            }
            return new DgText(AdjustCase(a), AdjustCase(b));

            string AdjustCase(string s) => capitalize ? s.ToLetterCase(LetterCaseMode.Upper) : s;
        }

        private static readonly Regex  _add_bottom = new(@"^\/meme\S*(s)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex _only_bottom = new(@"^\/meme\S*(d)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex    _top_only = new(@"^\/meme\S*(t)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex        _caps = new(@"^\/meme\S*(u)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex      _nowrap = new(@"^\/meme\S*(w)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex   _colorText = new(@"^\/meme\S*(c)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex      _fontSM = new(@"^\/meme\S*?(\d{1,3})("")\S*", RegexOptions.IgnoreCase);
        private static readonly Regex      _shadow = new(@"^\/meme\S*?(\d{1,3})(%)\S*",  RegexOptions.IgnoreCase);

        // LOGIC

        private static readonly MemeGenerator _imgflip = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override Task<string> MakeMemeImage(string path, DgText text)
        {
            return _queue.Enqueue(() => _imgflip.MakeMeme(path, text));
        }

        protected override Task<string> MakeMemeStick(string path, DgText text, string extension)
        {
            //return MakeMeme(Convert(path, extension), text);
            return MakeMemeImage(path, text);
        }

        protected override Task<string> MakeMemeVideo(string path, DgText text)
        {
            return _queue.Enqueue(() =>
            {
                Memes.Sticker = false;
                var size = SizeHelpers.GetImageSize_FFmpeg(path).GrowSize().ValidMp4Size();
                _imgflip.SetUp(size);

                return new F_Combine(path, _imgflip.MakeCaption(text)).Meme(Memes.Quality, size).Output("-M");
            });
        }
    }

    public interface ImageProcessor
    {
        ImageProcessor SetUp(int w, int h); // todo something

        void Pass(WitlessContext context);

        Task ProcessPhoto(string fileID);
        Task ProcessStick(string fileID);
    }
}