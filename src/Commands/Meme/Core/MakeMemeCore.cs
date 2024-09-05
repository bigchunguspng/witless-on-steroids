using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Memes.Shared;

namespace Witlesss.Commands.Meme.Core // ReSharper disable InconsistentNaming
{
    public abstract class MakeMemeCore_Static : WitlessAsyncCommand
    {
        protected static readonly Regex _repeat = new("[2-9]");
        protected static readonly Regex   _caps = new(@"\S*(up)\S*");
        protected static readonly Regex _nowrap = new(@"\S*(ww)\S*");
    }

    public abstract class MakeMemeCore<T> : MakeMemeCore_Static, ImageProcessor
    {
        protected MemeRequest Request = default!;

        protected abstract Regex _cmd { get; } // todo

        protected abstract string VideoName { get; }

        protected abstract string Log_STR { get; }
        protected abstract string Command { get; }
        protected abstract string Suffix  { get; }

        protected abstract string? DefaultOptions { get; }

        protected virtual bool CropVideoNotes  => true;
        protected virtual bool ConvertStickers => true;

        protected virtual bool ResultsAreRandom => false;

        public void Pass(WitlessContext context)
        {
            Context = context;
        }

        protected async Task RunInternal(string type, string? options)
        {
            if (await ProcessMessage(Message) || await ProcessMessage(Message.ReplyToMessage)) return;

            Bot.SendMessage(Chat, string.Format(MEME_MANUAL, type, options));
        }

        private async Task<bool> ProcessMessage(Message? message)
        {
            if (message is null) return false;

            if      (message.Photo     is not null) await ProcessPhoto(message.Photo[^1].FileId);
            else if (message.HasImageSticker    ()) await ProcessStick(message.Sticker !.FileId);
            else if (message.Animation is not null) await ProcessVideo(message.Animation.FileId);
            else if (message.HasVideoSticker    ()) await ProcessVideo(message.Sticker !.FileId, ".webm");
            else if (message.Video     is not null) await ProcessVideo(message.Video    .FileId);
            else if (message.VideoNote is not null) await ProcessVideo(message.VideoNote.FileId, round: true);
            else if (message.HasImageDocument   ()) await ProcessPhoto(message.Document!.FileId);
            else if (message.HasAnimeDocument   ()) await ProcessVideo(message.Document!.FileId, ".gif");
            else return false;

            return true;
        }


        // PROCESS MEDIA

        public async Task ProcessPhoto(string fileID)
        {
            var path = await Bot.Download(fileID, Chat, ".jpg");
            Request = GetRequestData();

            ParseOptions();
            var repeats = GetRepeatCount();
            var request = new MemeFileRequest(Chat, MemeSourceType.Image, path, Suffix + ".jpg", Baka.Quality);
            for (var i = 0; i < repeats; i++)
            {
                await using var stream = File.OpenRead(await MakeMemeImage(request, GetText()));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}]");
        }

        public async Task ProcessStick(string fileID)
        {
            var path = await Bot.Download(fileID, Chat, ".webp");
            Request = GetRequestData();

            ParseOptions();
            var repeats = GetRepeatCount();
            var sticker = SendAsSticker;
            var extension = sticker ? ".webp" : ".jpg";
            var request = new MemeFileRequest(Chat, MemeSourceType.Sticker, path, Suffix + extension, Baka.Quality)
            {
                ExportAsSticker = sticker,
                JpegSticker = JpegSticker
            };
            for (var i = 0; i < repeats; i++)
            {
                await using var stream = File.OpenRead(await MakeMemeStick(request, GetText()));

                if (sticker) Bot.SendSticker(Chat, new InputOnlineFile(stream));
                else         Bot.SendPhoto  (Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}] STICKER");
        }

        public async Task ProcessVideo(string fileID, string extension = ".mp4", bool round = false)
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Baka)) return;

            var sw = GetStartedStopwatch();

            var path = await Bot.Download(fileID, Chat, extension);
            Request = GetRequestData();

            if (round && CropVideoNotes)
                path = await path.UseFFMpeg(Chat).CropVideoNoteXD();

            ParseOptions();
            var repeats = GetRepeatCount().Clamp(3);
            var note = round && !CropVideoNotes;
            var request = new MemeFileRequest(Chat, MemeSourceType.Video, path, Suffix + ".mp4", Baka.Quality);
            for (var i = 0; i < repeats; i++)
            {
                await using var stream = File.OpenRead(await MakeMemeVideo(request, GetText()));

                if (note) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
                else      Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoName));
            }

            Log($"{Title} >> {Log_STR}{REP(repeats)} [{Request.Options ?? "~"}] VID >> {sw.ElapsedShort()}");
        }

        private string? REP(int repeats) => repeats > 1 ? $"-{repeats}" : null;

        protected abstract void ParseOptions();
        protected abstract T GetMemeText(string? text);

        private T GetText() => GetMemeText(GetTextBase());

        private string? GetTextBase() =>
            Context.Command is not null || Baka.Pics > 100 && Context.Message.ForwardFromChat is null ? Args : null;

        // MEME GENERATION

        protected abstract IMemeGenerator<T> MemeMaker { get; }
        protected abstract SerialTaskQueue   Queue     { get; }

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

        private bool SendAsSticker => CheckOptionsFor(options => options.Contains('='));
        private bool   JpegSticker => CheckOptionsFor(options => options.Contains('x'));

        private bool CheckOptionsFor(Predicate<string> condition)
        {
            return !Request.Empty && _cmd.ExtractGroup(1, Request.Dummy, s => condition(s), false);
        }
    }
}