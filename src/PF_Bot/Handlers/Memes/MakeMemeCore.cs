using PF_Bot.Core.Editing;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Handlers.Memes // ReSharper disable InconsistentNaming
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
        protected const string
            _r_caps   = "up",
            _r_nowrap = "ww";

        protected static readonly Regex
            _r_repeat = new("[2-9]",           RegexOptions.Compiled),
            _r_press  = new(@"(\*)(\d{1,2})?", RegexOptions.Compiled);
    }

    public abstract class MakeMemeCore<TCaption> : MakeMemeCore_Static, ImageProcessor
    {
        protected MemeOptionsContext Options = null!;

        protected abstract IMemeGenerator<TCaption> MemeMaker { get; }

        protected abstract Regex _rgx_cmd { get; }

        protected abstract string VideoName { get; }

        protected abstract string Log_STR { get; }
        protected abstract string Log_CMD { get; }
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
                var request = GetMemeFileRequest(MemeSourceType.Image, input, ".jpg");
                await MakeMemeImage(request);
                await using var stream = File.OpenRead(request.TargetPath);
                Bot.SendPhoto(Origin, InputFile.FromStream(stream));
            }
            Log($"{GetLogString(repeats)} PHOTO");
        }

        public async Task ProcessStick(FileBase file)
        {
            var input = await DownloadFileAndParseOptions(file, ".webp");

            var jpegSticker = OptionsContains('x');
            if (jpegSticker)
            {
                var output = input.GetOutputFilePath("stick-JPEG", ".jpg");
                await FFMpeg.Command(input, output, "").FFMpeg_Run();
                input = output;
            }

            var /*send as*/ sticker = OptionsContains('=');
            var extension = sticker ? ".webp" : ".jpg";

            var repeats = GetRepeatCount();
            for (var i = 0; i < repeats; i++)
            {
                var request = GetMemeFileRequest(MemeSourceType.Sticker, input, extension);
                request.ExportAsSticker = sticker;
                request.JpegSticker = jpegSticker;
                await MakeMemeImage(request);
                await using var stream = File.OpenRead(request.TargetPath);

                if (sticker) Bot.SendSticker(Origin, InputFile.FromStream(stream));
                else         Bot.SendPhoto  (Origin, InputFile.FromStream(stream));
            }
            Log($"{GetLogString(repeats)} STICKER");
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
                var request = GetMemeFileRequest(MemeSourceType.Video, input, ".mp4");
                await MakeMemeVideo(request);
                await using var stream = File.OpenRead(request.TargetPath);

                if (note) Bot.SendVideoNote(Origin, InputFile.FromStream(stream));
                else      Bot.SendAnimation(Origin, InputFile.FromStream(stream, VideoName));
            }
            Log($"{GetLogString(repeats)} VID >> {sw.ElapsedReadable()}");
        }

        //

        private float _pressure;

        private Task<FilePath> DownloadFileAndParseOptions(FileBase file, string extension)
        {
            Options = GetOptionsContext();
            ParseOptions();

            _pressure = Options.GetFraction(_r_press, 75, 2);
            return Bot.Download(file, Origin, extension);
        }

        protected abstract void ParseOptions();

        private MemeRequest GetMemeFileRequest
            (MemeSourceType type, FilePath input, string extension)
        {
            var output = input.ChangeEnding($"{Suffix}-{Desert.GetSilt()}{extension}");
            return new MemeRequest(type, input, output, Data.Quality, _pressure);
        }

        private string GetLogString(int repeats)
        {
            var repSuffix = repeats > 1 ? $"-{repeats}" : null;
            return $"{Title} >> {Log_STR}{repSuffix} [{Options.Options ?? "~"}]";
        }


        // MEME GENERATION

        private async Task MakeMemeImage
            (MemeRequest request)
        {
            var sw = Stopwatch.StartNew();
            await MemeMaker.GenerateMeme(request, GetText());
            sw.Log(Log_CMD);
        }

        private async Task MakeMemeVideo
            (MemeRequest request)
        {
            var sw = Stopwatch.StartNew();
            await MemeMaker.GenerateVideoMeme(request, GetText());
            sw.Log(Log_CMD + " video");
        }

        private TCaption GetText()
        {
            var baseText
                = Context.Command != null
               || Data.Pics > 100 && Context.Message.IsForwarded().Janai()
                    ? Args
                    : null;
            return GetMemeText(baseText);
        }

        protected abstract TCaption GetMemeText(string? text);


        // OPTIONS

        private MemeOptionsContext GetOptionsContext()
        {
            var default_options = DefaultOptions;

            var empty = Text == null && default_options == null;
            if (empty) return new MemeOptionsContext(empty, string.Empty, null, null);

            var command = Context.Command ?? string.Empty;
            var command_options = _rgx_cmd.ExtractGroup(1, command, s => s.MakeNull_IfEmpty());

            var combineOptions =
                command_options != null
             && default_options != null
             && (command_options.Contains('+') || default_options.Contains('+'));

            var options = combineOptions
                ? default_options + command_options
                : command_options ?? default_options;

            return new MemeOptionsContext(empty, $"{options}", options, command_options);
        }

        private int GetRepeatCount()
        {
            var random = Args == null || ResultsAreRandom;
            return
                random && Options.Empty.Janai()
                    ? _r_repeat.ExtractGroup(0, Options.Buffer, int.Parse, 1)
                    : 1;
        }

        private bool OptionsContains(char option)
        {
            return Options.Buffer.Contains(option);
        }
    }
}