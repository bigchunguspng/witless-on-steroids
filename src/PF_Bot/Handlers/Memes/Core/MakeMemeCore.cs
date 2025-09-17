using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Editing;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Handlers.Memes.Core // ReSharper disable InconsistentNaming
{
    public interface ImageProcessor
    {
        void Pass(WitlessContext context);

        Task ProcessPhoto(FileBase file);
        Task ProcessStick(FileBase file);
        Task ProcessVideo(FileBase file, string extension = ".mp4", bool round = false);
    }

    public abstract class MakeMemeCore_Static : WitlessAsyncCommand
    {
        protected static readonly Regex
            _r_repeat = new("[2-9]",                 RegexOptions.Compiled),
            _r_caps   = new(@"\S*(up)\S*",           RegexOptions.Compiled),
            _r_nowrap = new(@"\S*(ww)\S*",           RegexOptions.Compiled),
            _r_press  = new(@"\S*(\*)(\d{1,2})?\S*", RegexOptions.Compiled);
    }

    public abstract class MakeMemeCore<T> : MakeMemeCore_Static, ImageProcessor
    {
        protected MemeRequest Request = null!;

        protected abstract IMemeGenerator<T> MemeMaker { get; }

        protected abstract Regex _rgx_cmd { get; }

        protected abstract string VideoName { get; }

        protected abstract string Log_STR { get; }
        protected abstract string Command { get; }
        protected abstract string Suffix  { get; }

        protected abstract string? DefaultOptions { get; }

        protected virtual bool CropVideoNotes   => true;
        protected virtual bool ResultsAreRandom => false;

        public void Pass(WitlessContext context)
        {
            Context = context;
        }

        protected async Task RunInternal(string? options)
        {
            if (await ProcessMessage(Message) || await ProcessMessage(Message.ReplyToMessage)) return;

            Bot.SendMessage(Origin, string.Format(MEME_MANUAL, options));
        }

        private async Task<bool> ProcessMessage(Message? message)
        {
            if (message is null) return false;

            if      (message.Photo     is not null) await ProcessPhoto(message.Photo[^1]);
            else if (message.HasImageSticker    ()) await ProcessStick(message.Sticker !);
            else if (message.Animation is not null) await ProcessVideo(message.Animation);
            else if (message.HasVideoSticker    ()) await ProcessVideo(message.Sticker !, ".webm");
            else if (message.Video     is not null) await ProcessVideo(message.Video    );
            else if (message.VideoNote is not null) await ProcessVideo(message.VideoNote, round: true);
            else if (message.HasImageDocument   ()) await ProcessPhoto(message.Document!);
            else if (message.HasAnimeDocument   ()) await ProcessVideo(message.Document!, ".gif");
            else if (message.HasVideoDocument   ()) await ProcessVideo(message.Document!, ".webm");
            else return false;

            return true;
        }


        // PROCESS MEDIA

        public async Task ProcessPhoto(FileBase file)
        {
            var input = await DownloadFileAndParseOptions(file, ".jpg");

            var repeats = GetRepeatCount();
            for (var i = 0; i < repeats; i++)
            {
                var output = GetOutputFilePath(input, ".jpg");
                var request = GetMemeFileRequest(MemeSourceType.Image, input, output);
                await MakeMemeImage(request, GetText());
                await using var stream = File.OpenRead(output);
                Bot.SendPhoto(Origin, InputFile.FromStream(stream));
            }
            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}]");
        }

        public async Task ProcessStick(FileBase file)
        {
            var input = await DownloadFileAndParseOptions(file, ".webp");

            var jpegSticker = JpegSticker;
            if (jpegSticker)
            {
                var output = input.GetOutputFilePath("stick-JPEG", ".jpg");
                await FFMpeg.Command(input, output, "").FFMpeg_Run();
                input = output;
            }

            var sticker = SendAsSticker;
            var extension = sticker ? ".webp" : ".jpg";

            var repeats = GetRepeatCount();
            for (var i = 0; i < repeats; i++)
            {
                var output = GetOutputFilePath(input, extension);
                var request = GetMemeFileRequest(MemeSourceType.Sticker, input, output);
                request.ExportAsSticker = sticker;
                request.JpegSticker = jpegSticker;
                await MakeMemeImage(request, GetText());
                await using var stream = File.OpenRead(output);

                if (sticker) Bot.SendSticker(Origin, InputFile.FromStream(stream));
                else         Bot.SendPhoto  (Origin, InputFile.FromStream(stream));
            }
            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}] STICKER");
        }

        public async Task ProcessVideo(FileBase file, string extension = ".mp4", bool round = false)
        {
            var sw = Stopwatch.StartNew();

            var input = await DownloadFileAndParseOptions(file, extension);
            if (round && CropVideoNotes)
            {
                var output = input.GetOutputFilePath("crop", ".mp4");
                await FFMpeg.Command(input, output, o => o.Crop(FFMpegOptions.VIDEONOTE_CROP)).FFMpeg_Run();
                input = output;
            }

            var note = round && CropVideoNotes.IsOff();

            var repeats = GetRepeatCount().Clamp(3);
            for (var i = 0; i < repeats; i++)
            {
                var output = GetOutputFilePath(input, ".mp4");
                var request = GetMemeFileRequest(MemeSourceType.Video, input, output);
                await MakeMemeVideo(request, GetText());
                await using var stream = File.OpenRead(output);

                if (note) Bot.SendVideoNote(Origin, InputFile.FromStream(stream));
                else      Bot.SendAnimation(Origin, InputFile.FromStream(stream, VideoName));
            }
            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}] VID >> {sw.ElapsedReadable()}");
        }

        private Task<FilePath> DownloadFileAndParseOptions(FileBase file, string extension)
        {
            Request = GetRequestData();
            ParseOptions();

            return Bot.Download(file, Origin, extension);
        }

        private MemeFileRequest GetMemeFileRequest
            (MemeSourceType type, FilePath input, FilePath output)
        {
            return new MemeFileRequest(type, input, output, Data.Quality, Pressure);
        }

        private FilePath GetOutputFilePath(FilePath input, string extension)
        {
            return input.ChangeEnding($"{Suffix}-{Desert.GetSilt()}{extension}");
        }

        private T GetText() => GetMemeText(GetTextBase());

        private string? GetTextBase() =>
            Context.Command is not null || Data.Pics > 100 && Context.Message.ForwardFromChat is null ? Args : null;

        protected abstract void ParseOptions();
        protected abstract T GetMemeText(string? text);

        private string? REP(int repeats) => repeats > 1 ? $"-{repeats}" : null;


        // MEME GENERATION

        private async Task MakeMemeImage
            (MemeFileRequest request, T text)
        {
            var sw = Stopwatch.StartNew();
            await MemeMaker.GenerateMeme(request, text);
            sw.Log(Command);
        }

        private async Task MakeMemeVideo
            (MemeFileRequest request, T text)
        {
            var sw = Stopwatch.StartNew();
            await MemeMaker.GenerateVideoMeme(request, text);
            sw.Log(Command + " video");
        }


        // OPTIONS

        private MemeRequest GetRequestData()
        {
            var defaults = DefaultOptions;
            var dummy = string.Empty;
            var command = Context.Command ?? "";
            var options = default(string);
            var empty = Text is null && defaults is null;

            if (empty.Janai())
            {
                options = _rgx_cmd.ExtractGroup(1, command, s => s.MakeNull_IfEmpty());
                var combine = options != null && defaults != null && (options.Contains('+') || defaults.Contains('+'));

                options = combine ? defaults + options : options ?? defaults;
                dummy = $"{Command}{options}";
            }

            return new MemeRequest(dummy, empty, command, options);
        }

        private int GetRepeatCount()
        {
            var repeats = 1;
            var hasToBeRepeated = (Args is null || ResultsAreRandom) && CheckOptionsFor(o => _r_repeat.IsMatch(o));
            if (hasToBeRepeated) repeats = _r_repeat.ExtractGroup(0, Request.Dummy, int.Parse, repeats);
            return repeats;
        }

        private float     Pressure => OptionsParsing.GetFraction(Request, _r_press, 75, 2);

        private bool SendAsSticker => CheckOptionsFor(options => options.Contains('='));
        private bool   JpegSticker => CheckOptionsFor(options => options.Contains('x'));

        private bool CheckOptionsFor(Predicate<string> condition)
        {
            return Request.Empty.Janai() && _rgx_cmd.ExtractGroup(1, Request.Dummy, s => condition(s), false);
        }
    }
}