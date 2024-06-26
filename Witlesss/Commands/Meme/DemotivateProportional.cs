﻿using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Meme
{
    public class DemotivateProportional : MakeMemeCore<string>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/dp(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"DEMOTIVATOR-B [{(x == 1 ? "_" : x)}]";
        protected override string Log_STICK(int x) => $"DEMOTIVATOR-B [{(x == 1 ? "#" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "DEMOTIVATOR-B [^] VID";
        protected override string VideoName { get; } = "piece_fap_club-dp.mp4";
        
        protected override string Command { get; } = "/dp";
        protected override string Suffix  { get; } = "-Dp";

        protected override string? DefaultOptions => Baka.Meme.Options?.Dp;


        protected override Task Run() => RunInternal("Демотиваторы👌", DP_OPTIONS);

        protected override void ParseOptions()
        {
            DynamicDemotivatorDrawer.CustomColorOption.CheckAndCut(Request);
            DynamicDemotivatorDrawer.ExtraFonts.CheckAndCut(Request);
            DynamicDemotivatorDrawer.CropEdges = OptionsParsing.CheckAndCut(Request, _crop);
        }

        protected override string GetMemeText(string? text)
        {
            var matchCaps = OptionsParsing.Check(Request, _caps);

            var gen = string.IsNullOrEmpty(text);
            var caps = matchCaps && (gen || _caps.IsMatch(Request.Command)); // command, not chat defaults

            var txt = gen ? Baka.Generate() : text;

            return caps ? txt.ToLetterCase(LetterCaseMode.Upper) : txt;
        }

        private static readonly Regex _crop    = new(@"^\/dp\S*cp\S*", RegexOptions.IgnoreCase);
        private static readonly Regex _caps    = new(@"^\/dp\S*up\S*", RegexOptions.IgnoreCase);

        // LOGIC

        private static readonly DynamicDemotivatorDrawer  _dp = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override Task<string> MakeMemeImage(MemeFileRequest request, string text)
        {
            return _queue.Enqueue(() => _dp.DrawDemotivator(request, text));
        }

        protected override async Task<string> MakeMemeStick(MemeFileRequest request, string text)
        {
            if (request.ConvertSticker)
                request.SourcePath = await Memes.Convert(request.SourcePath, ".jpg");
            return await MakeMemeImage(request, text);
        }

        protected override Task<string> MakeMemeVideo(MemeFileRequest request, string text)
        {
            return _queue.Enqueue(() =>
            {
                _dp.PassTextLength(text);

                var size = SizeHelpers.GetImageSize_FFmpeg(request.SourcePath).GrowSize().ValidMp4Size();
                _dp.SetUp(size);
                _dp.SetColor();

                var frame = _dp.BakeFrame(text);
                var full_size = SizeHelpers.GetImageSize_FFmpeg(frame).FitSize(720);

                return new F_Combine(request.SourcePath, frame).D300(GetCRF(), size, _dp.Location, full_size).Output(Suffix);
            });
        }
    }
}