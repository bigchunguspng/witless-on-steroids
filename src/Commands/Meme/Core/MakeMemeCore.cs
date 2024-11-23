using Telegram.Bot.Types;
using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Memes.Shared;

namespace Witlesss.Commands.Meme.Core // ReSharper disable InconsistentNaming
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
        protected static readonly Regex _repeat = new("[2-9]");
        protected static readonly Regex   _caps = new(@"\S*(up)\S*");
        protected static readonly Regex _nowrap = new(@"\S*(ww)\S*");
        protected static readonly Regex  _press = new(@"\S*(\*)(\d{1,2})?\S*");
    }

    public abstract class MakeMemeCore<T> : MakeMemeCore_Static, ImageProcessor
    {
        protected MemeRequest Request = default!;

        protected abstract IMemeGenerator<T> MemeMaker { get; }

        protected abstract Regex _cmd { get; }

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

            Bot.SendMessage(Chat, string.Format(MEME_MANUAL, options));
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
            var path = await DownloadFileAndParseOptions(file, ".jpg");

            var request = GetMemeFileRequest(MemeSourceType.Image, path, Suffix + ".jpg");
            var repeats = GetRepeatCount();
            for (var i = 0; i < repeats; i++)
            {
                await using var stream = File.OpenRead(await MakeMemeImage(request, GetText()));
                Bot.SendPhoto(Chat, InputFile.FromStream(stream));
            }
            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}]");
        }

        public async Task ProcessStick(FileBase file)
        {
            var path = await DownloadFileAndParseOptions(file, ".webp");

            var sticker = SendAsSticker;
            var extension = sticker ? ".webp" : ".jpg";

            var request = GetMemeFileRequest(MemeSourceType.Sticker, path, Suffix + extension);
            request.ExportAsSticker = sticker;
            request.JpegSticker = JpegSticker;
            var repeats = GetRepeatCount();
            for (var i = 0; i < repeats; i++)
            {
                await using var stream = File.OpenRead(await MakeMemeStick(request, GetText()));

                if (sticker) Bot.SendSticker(Chat, InputFile.FromStream(stream));
                else         Bot.SendPhoto  (Chat, InputFile.FromStream(stream));
            }
            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}] STICKER");
        }

        public async Task ProcessVideo(FileBase file, string extension = ".mp4", bool round = false)
        {
            var sw = GetStartedStopwatch();

            var path = await DownloadFileAndParseOptions(file, extension);
            if (round && CropVideoNotes) path = await path.UseFFMpeg(Chat).CropVideoNoteXD();

            var note = round && !CropVideoNotes;

            var request = GetMemeFileRequest(MemeSourceType.Video, path, Suffix + ".mp4");
            var repeats = GetRepeatCount().Clamp(3);
            for (var i = 0; i < repeats; i++)
            {
                await using var stream = File.OpenRead(await MakeMemeVideo(request, GetText()));

                if (note) Bot.SendVideoNote(Chat, InputFile.FromStream(stream));
                else      Bot.SendAnimation(Chat, InputFile.FromStream(stream, VideoName));
            }
            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}] VID >> {sw.ElapsedShort()}");
        }

        private Task<string> DownloadFileAndParseOptions(FileBase file, string extension)
        {
            Request = GetRequestData();
            ParseOptions();

            return Bot.Download(file, Chat, extension);
        }

        private MemeFileRequest GetMemeFileRequest
            (MemeSourceType type, string path, string outputEnding)
            => new(Chat, type, path, outputEnding, Data.Quality, Pressure);

        private T GetText() => GetMemeText(GetTextBase());

        private string? GetTextBase() =>
            Context.Command is not null || Data.Pics > 100 && Context.Message.ForwardFromChat is null ? Args : null;

        protected abstract void ParseOptions();
        protected abstract T GetMemeText(string? text);

        private string? REP(int repeats) => repeats > 1 ? $"-{repeats}" : null;


        // MEME GENERATION

        protected abstract SerialTaskQueue Queue { get; }

        private Task<string> MakeMemeImage(MemeFileRequest request, T text)
        {
            return Queue.Enqueue(() =>
            {
                var sw = GetStartedStopwatch();
                var result = MemeMaker.GenerateMeme(request, text);
                sw.Log(Command);
                return result;
            });
        }

        private async Task<string> MakeMemeStick(MemeFileRequest request, T text)
        {
            if (request.JpegSticker)
                request.SourcePath = await Chat.Convert(request.SourcePath, ".jpg");
            return await MakeMemeImage(request, text);
        }

        private Task<string> MakeMemeVideo(MemeFileRequest request, T text)
        {
            return Queue.Enqueue(() =>
            {
                var sw = GetStartedStopwatch();
                var result = MemeMaker.GenerateVideoMeme(request, text);
                sw.Log(Command + " video");
                return result;
            });
        }


        // OTHER

        private MemeRequest GetRequestData()
        {
            var defaults = DefaultOptions;
            var dummy = string.Empty;
            var command = Context.Command ?? "";
            var options = default(string);
            var empty = Text is null && defaults is null;

            if (!empty)
            {
                options = _cmd.ExtractGroup(1, command, s => s.MakeNull_IfEmpty());
                var combine = options != null && defaults != null && (options.Contains('+') || defaults.Contains('+'));

                options = combine ? defaults + options : options ?? defaults;
                dummy = $"{Command}{options}";
            }

            return new MemeRequest(dummy, empty, command, options);
        }

        private int GetRepeatCount()
        {
            var repeats = 1;
            var hasToBeRepeated = (Args is null || ResultsAreRandom) && CheckOptionsFor(o => _repeat.IsMatch(o));
            if (hasToBeRepeated) repeats = _repeat.ExtractGroup(0, Request.Dummy, int.Parse, repeats);
            return repeats;
        }

        private float     Pressure => OptionsParsing.GetFraction(Request, _press, 75, 2);

        private bool SendAsSticker => CheckOptionsFor(options => options.Contains('='));
        private bool   JpegSticker => CheckOptionsFor(options => options.Contains('x'));

        private bool CheckOptionsFor(Predicate<string> condition)
        {
            return !Request.Empty && _cmd.ExtractGroup(1, Request.Dummy, s => condition(s), false);
        }
    }
}