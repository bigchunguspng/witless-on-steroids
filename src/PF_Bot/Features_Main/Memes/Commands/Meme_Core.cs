using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Bot.Features_Main.Memes.Core.Shared;
using PF_Bot.Routing.Commands;
using PF_Bot.Routing.Messages;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Main.Memes.Commands // ReSharper disable InconsistentNaming
{
    public abstract class Meme_Core_Static : CommandHandlerAsync
    {
        protected const string
            _r_caps   = "up",
            _r_nowrap = "ww";

        protected static readonly Regex
            _r_repeat = new("[2-9]",           RegexOptions.Compiled),
            _r_press  = new(@"(\*)(\d{1,2})?", RegexOptions.Compiled);
    }

    public abstract class Meme_Core<TCaption> : Meme_Core_Static, AutoMemesHandler
    {
        protected MemeOptionsContext MemeOptions = null!;

        protected abstract IMemeGenerator<TCaption> MemeMaker { get; }

        protected abstract string VideoName { get; }

        protected abstract string Log_STR { get; }
        protected abstract string Log_CMD { get; }
        protected abstract string Suffix  { get; }

        protected abstract string? DefaultOptions { get; }

        protected virtual bool CropVideoNotes   => true;
        protected virtual bool ResultsAreRandom => false;

        protected async Task RunInternal(string options)
        {
            if (Input.HasValue && await ProcessInput(Input.Value))
                return;

            if (await ProcessMessage(Message) || await ProcessMessage(Message.ReplyToMessage))
                return;

            SendManual(MEME_MANUAL.Format(options));
        }

        private async Task<bool> ProcessInput(FilePath input)
        {
            var      ext = input.Extension;
            if      (ext is ".jpg" or ".png"           ) await ProcessPhoto(input);
            else if (ext is                     ".webp") await ProcessStick(input);
            else if (ext is ".mp4" or ".gif" or ".webm") await ProcessVideo(input);
            else return false;

            return true;
        }

        private async Task<bool> ProcessMessage(Message? message)
        {
            if (message == null) return false;

            if      (message.Photo      != null) await ProcessPhoto(message.Photo[^1]);
            else if (message.HasImageSticker ()) await ProcessStick(message.Sticker !);
            else if (message.Animation  != null) await ProcessVideo(message.Animation);
            else if (message.HasVideoSticker ()) await ProcessVideo(message.Sticker !, ".webm");
            else if (message.Video      != null) await ProcessVideo(message.Video    );
            else if (message.VideoNote  != null) await ProcessVideo(message.VideoNote, round: true);
            else if (message.HasImageDocument()) await ProcessPhoto(message.Document!);
            else if (message.HasAnimeDocument()) await ProcessVideo(message.Document!, ".gif");
            else if (message.HasVideoDocument()) await ProcessVideo(message.Document!, ".webm");
            else return false;

            return true;
        }


        // PROCESS MEDIA (TELEGRAM)

        public async Task ProcessPhoto(FileBase file)
        {
            var input = await Bot.Download(file, Origin, ".jpg");
            await ProcessPhoto(input);
        }

        public async Task ProcessStick(FileBase file)
        {
            var input = await Bot.Download(file, Origin, ".webp");
            await ProcessStick(input);
        }

        public async Task ProcessVideo(FileBase file, string extension = ".mp4", bool round = false)
        {
            var input = await Bot.Download(file, Origin, extension);
            await ProcessVideo(input, round);
        }

        // PROCESS MEDIA (LOCAL)

        private async Task ProcessPhoto(FilePath input)
        {
            SetupOptions();

            var repeats = GetRepeatCount();
            for (var i = 0; i < repeats; i++)
            {
                var request = GetMemeFileRequest(MemeSourceType.Image, input, ".jpg");
                await MakeMemeImage(request);
                SendFile(request.TargetPath, MediaType.Photo);
            }
            Log($"{GetLogString(repeats)} PHOTO");
        }

        private async Task ProcessStick(FilePath input)
        {
            SetupOptions();

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

                var type = sticker ? MediaType.Stick : MediaType.Photo;
                SendFile(request.TargetPath, type);
            }
            Log($"{GetLogString(repeats)} STICKER");
        }

        private async Task ProcessVideo(FilePath input, bool round = false)
        {
            SetupOptions();

            var sw = Stopwatch.StartNew();

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

                var type = note ? MediaType.Round : MediaType.Anime;
                var name = note ? null : VideoName;
                SendFile(request.TargetPath, type, name);
            }
            Log($"{GetLogString(repeats)} VID >> {sw.ElapsedReadable()}");
        }

        //

        private float _pressure;

        private void SetupOptions()
        {
            MemeOptions = GetOptionsContext();
            ParseOptions();

            _pressure = MemeOptions.GetFraction(_r_press, 75, 2);
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
            return $"{Title} >> {Log_STR}{repSuffix} [{MemeOptions.Options ?? "~"}]";
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
            var useArgs =
                Mode != CommandMode.AUTO
             || Context.Automemes_UseMessageText;

            return GetMemeText(useArgs ? Args : null);
        }

        protected abstract TCaption GetMemeText(string? text);


        // OPTIONS

        private MemeOptionsContext GetOptionsContext()
        {
            var default_options = DefaultOptions;

            var empty = Context.Options == null && default_options == null;
            if (empty) return new MemeOptionsContext(empty, string.Empty, null, null);

            var command_options = Context.Options;

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
                random && MemeOptions.Empty.Janai()
                    ? _r_repeat.ExtractGroup(0, MemeOptions.Buffer, int.Parse, 1)
                    : 1;
        }

        private bool OptionsContains(char option)
        {
            return MemeOptions.Buffer.Contains(option);
        }

        // AUTO-MODE

        public void Automemes_PassContext(CommandContext context)
        {
            Context = context;
        }
    }
}