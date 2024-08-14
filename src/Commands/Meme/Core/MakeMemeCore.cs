using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;
using Witlesss.Memes.Shared;

namespace Witlesss.Commands.Meme.Core // ReSharper disable InconsistentNaming
{
    public abstract class MakeMemeCore<T> : MakeMemeCore_Static
    {
        protected MemeRequest Request = default!;

        protected abstract Regex _cmd { get; } // todo

        protected abstract string Log_PHOTO ( int x);
        protected abstract string Log_STICK ( int x);
        protected abstract string Log_VIDEO { get; }
        protected abstract string VideoName { get; }

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
            else if (message.HasVideoSticker    ()) await ProcessVideo(message.Sticker !.FileId);
            else if (message.Video     is not null) await ProcessVideo(message.Video    .FileId);
            else if (message.VideoNote is not null) await ProcessVideo(message.VideoNote.FileId);
            else if (message.HasImageDocument   ()) await ProcessPhoto(message.Document!.FileId);
            else if (message.HasAnimeDocument   ()) await ProcessVideo(message.Document!.FileId);
            else return false;

            return true;
        }


        // PROCESS MEDIA

        public async Task ProcessPhoto(string fileID)
        {
            var (path, _) = await Bot.Download(fileID, Chat);
            Request = GetRequestData();

            ParseOptions();
            var repeats = GetRepeatCount();
            var request = new MemeFileRequest(path, Suffix + ".jpg", Baka.Quality)
            {
                Type = MemeSourceType.Image
            };
            for (var i = 0; i < repeats; i++)
            {
                var text = GetMemeText(GetTextBase());
                await using var stream = File.OpenRead(await MakeMemeImage(request, text));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_PHOTO(repeats)}");
        }

        public async Task ProcessStick(string fileID)
        {
            var (path, _) = await Bot.Download(fileID, Chat);
            Request = GetRequestData();

            ParseOptions();
            var repeats = GetRepeatCount();
            var sticker = SendAsSticker;
            var request = new MemeFileRequest(path, Suffix + (sticker ? ".webp" : ".jpg"), Baka.Quality)
            {
                Type = MemeSourceType.Sticker,
                ExportAsSticker = sticker,
                ConvertSticker = ConvertStickerToJpeg()
            };
            for (var i = 0; i < repeats; i++)
            {
                var text = GetMemeText(GetTextBase());
                var result = await MakeMemeStick(request, text);
                if (sticker && ConvertStickers)
                    result = await new F_Process(result).Output("-stick", ".webp");
                await using var stream = File.OpenRead(result);
                if (sticker) Bot.SendSticker(Chat, new InputOnlineFile(stream));
                else         Bot.SendPhoto  (Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_STICK(repeats)}");
        }

        public async Task ProcessVideo(string fileID)
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Baka)) return;

            var sw = Helpers.GetStartedStopwatch();
            var (path, type) = await Bot.Download(fileID, Chat);
            Request = GetRequestData();

            if (CropVideoNotes && type == MediaType.Round) path = await FFMpegXD.CropVideoNote(path);

            ParseOptions();
            var text = GetMemeText(GetTextBase());
            var request = new MemeFileRequest(path, Suffix + ".mp4", Baka.Quality)
            {
                Type = MemeSourceType.Video
            };
            await using var stream = File.OpenRead(await MakeMemeVideo(request, text));
            var note = type == MediaType.Round && !CropVideoNotes;
            if (note) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            else      Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoName));

            Log($@"{Title} >> {Log_VIDEO} >> TIME: {FormatTime(sw.Elapsed)}");
        }

        protected abstract void ParseOptions();
        protected abstract T GetMemeText(string? text);

        private string? GetTextBase() => Context.Command is not null || Baka.Pics > 100 ? Args : null;

        // MEME GENERATION

        protected abstract IMemeGenerator<T> MemeMaker { get; }
        protected abstract SerialTaskQueue   Queue     { get; }

        protected virtual Task<string> MakeMemeImage(MemeFileRequest request, T text)
        {
            return Queue.Enqueue(() =>
            {
                var sw = Helpers.GetStartedStopwatch();
                var result = MemeMaker.GenerateMeme(request, text);
                sw.Log(Command);
                return result;
            });
        }

        protected virtual async Task<string> MakeMemeStick(MemeFileRequest request, T text)
        {
            if (request.ConvertSticker)
                request.SourcePath = await FFMpegXD.Convert(request.SourcePath, ".jpg");
            return await MakeMemeImage(request, text);
        }

        protected virtual Task<string> MakeMemeVideo(MemeFileRequest request, T text)
        {
            return Queue.Enqueue(() => MemeMaker.GenerateVideoMeme(request, text));
        }


        // OTHER

        private MemeRequest GetRequestData()
        {
            var defaults = DefaultOptions;
            var dummy = string.Empty;
            var command = Context.Command ?? "";
            var empty = Text is null && defaults is null;

            if (!empty)
            {
                if (defaults is not null) defaults = Command + defaults;

                var hasOptions = command.Length > Command.Length && command.StartsWith(Command);
                var combine = hasOptions && defaults is not null && (command.Contains('+') || defaults.Contains('+'));

                dummy = hasOptions
                    ? combine
                        ? defaults + command[Command.Length..]
                        : command
                    : defaults ?? command;
            }

            return new MemeRequest(dummy, empty, command);
        }

        private int GetRepeatCount()
        {
            var repeats = 1;
            var hasToBeRepeated = (Args is null || ResultsAreRandom) && CheckOptionsFor(o => _repeat.IsMatch(o));
            if (hasToBeRepeated)
            {
                var match = _repeat.Match(Request.Dummy);
                if (match.Success && int.TryParse(match.Value, out var x)) repeats = x;
            }
            return repeats;
        }

        private bool SendAsSticker => CheckOptionsFor(options => options.Contains('='));

        private bool ConvertStickerToJpeg() => CheckOptionsFor(options => options.Contains('x'));

        private bool CheckOptionsFor(Predicate<string> condition)
        {
            if (Request.Empty) return false;

            var match = _cmd.Match(Request.Dummy);
            if (match.Success) return condition(match.Groups[1].Value);
            return false;
        }
    }

    public abstract class MakeMemeCore_Static : WitlessAsyncCommand
    {
        protected static readonly Regex _repeat = new(@"[2-9]");
        protected static readonly Regex   _caps = new(@"\S*(up)\S*");
        protected static readonly Regex _nowrap = new(@"\S*(ww)\S*");
    }
}