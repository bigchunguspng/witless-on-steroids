﻿using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Meme
{
    public class MemeDeepFryer : MakeMemeCore<int>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/nuke(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"NUKED [{(x == 1 ? "*" : x)}]";
        protected override string Log_STICK(int x) => $"NUKED [{(x == 1 ? "*" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "NUKED [*] VID";
        protected override string VideoName { get; } = "nuke_fap_club.mp4";

        protected override string Command { get; } = "/nuke";
        protected override string Suffix  { get; } = "-Nuked";

        protected override string? DefaultOptions => Baka.Meme.Options?.Nuke;


        protected override Task Run() => RunInternal("Ядерные отходы");

        protected override void ParseOptions() { }

        protected override int GetMemeText(string? text) => 0; // Needs more nuking!

        protected override bool CropVideoNotes  { get; } = false;
        protected override bool ConvertStickers { get; } = false;

        // LOGIC

        private static readonly SerialTaskQueue _nukeQueue = new();

        protected override async Task<string> MakeMemeImage(MemeFileRequest request, int text)
        {
            return await new F_Process(request.SourcePath).DeepFry(GetQscale()).OutputAs(request.TargetPath);
        }

        protected override Task<string> MakeMemeStick(MemeFileRequest request, int text)
        {
            return MakeMemeImage(request, text);
        }

        protected override async Task<string> MakeMemeVideo(MemeFileRequest request, int text)
        {
            var size = SizeHelpers.GetImageSize_FFmpeg(request.SourcePath).GrowSize().ValidMp4Size();
            return await new F_Process(request.SourcePath).DeepFryVideo(size.Ok(), GetCRF()).Output_WEBM_safe(Suffix);
        }
    }
}