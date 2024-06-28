using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;
using Witlesss.Memes;

namespace Witlesss.Commands.Meme // ReSharper disable InconsistentNaming
{
    public abstract class MakeMemeCore<T> : MakeMemeCore_Static
    {
        protected MemeRequest Request = default!;

        protected abstract Regex _cmd { get; }

        protected abstract string Log_PHOTO ( int x);
        protected abstract string Log_STICK ( int x);
        protected abstract string Log_VIDEO { get; }
        protected abstract string VideoName { get; }

        protected abstract string Command { get; }
        protected abstract string Suffix  { get; }

        protected abstract string? DefaultOptions { get; }

        protected virtual bool CropVideoNotes  { get; } = true;
        protected virtual bool ConvertStickers { get; } = true;


        public void Pass(WitlessContext context)
        {
            Context = context;
        }

        protected async Task RunInternal(string type, string? options = null)
        {
            if (await ProcessMessage(Message) || await ProcessMessage(Message.ReplyToMessage)) return;

            var message = string.Format(MEME_MANUAL, type);
            Bot.SendMessage(Chat, options is null ? message : $"{message}\n\n{options}");
        }

        private async Task<bool> ProcessMessage(Message? message)
        {
            if (message is null) return false;

            if      (message.Photo     is not null)              await ProcessPhoto(message.Photo[^1].FileId);
            else if (message.Animation is not null)              await ProcessVideo(message.Animation.FileId);
            else if (message.Sticker   is { IsVideo: true })     await ProcessVideo(message.Sticker  .FileId);
            else if (message.Video     is not null)              await ProcessVideo(message.Video    .FileId);
            else if (message.VideoNote is not null)              await ProcessVideo(message.VideoNote.FileId);
            else if (message.Sticker   is { IsAnimated: false }) await ProcessStick(message.Sticker  .FileId);
            else return false;

            return true;
        }

        public async Task ProcessPhoto(string fileID)
        {
            var (path, _) = await Bot.Download(fileID, Chat);
            Request = GetRequestData();

            ParseOptions();
            var repeats = GetRepeatCount();
            var txt = GetProvidedText();
            var request = new MemeFileRequest(path, Suffix + ".jpg", Baka.Meme.Quality)
            {
                Type = MemeSourceType.Image
            };
            for (var i = 0; i < repeats; i++)
            {
                var text = GetMemeText(txt);
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
            var txt = GetProvidedText();
            var request = new MemeFileRequest(path, Suffix + (sticker ? ".webp" : ".jpg"), Baka.Meme.Quality)
            {
                Type = MemeSourceType.Sticker,
                ExportAsSticker = sticker,
                ConvertSticker = ConvertStickerToJpeg()
            };
            for (var i = 0; i < repeats; i++)
            {
                var text = GetMemeText(txt);
                var result = await MakeMemeStick(request, text);
                if (sticker && ConvertStickers)
                    result = await new F_Process(result).Output("-stick", ".webp");
                await using var stream = File.OpenRead(result);
                if (sticker) Bot.SendSticker(Chat, new InputOnlineFile(stream));
                else         Bot.SendPhoto  (Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_STICK(repeats)}");
        }

        private async Task ProcessVideo(string fileID)
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Baka)) return;

            var sw = Helpers.GetStartedStopwatch();
            var (path, type) = await Bot.Download(fileID, Chat);
            Request = GetRequestData();

            if (CropVideoNotes && type == MediaType.Round) path = await FFMpegXD.CropVideoNote(path);

            ParseOptions();
            var text = GetMemeText(GetProvidedText());
            var request = new MemeFileRequest(path, Suffix + ".mp4", Baka.Meme.Quality)
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


        // MEME GENERATION

        protected abstract IMemeGenerator<T> MemeMaker { get; }
        protected abstract SerialTaskQueue   Queue     { get; }

        protected virtual Task<string> MakeMemeImage(MemeFileRequest request, T text)
        {
            return Queue.Enqueue(() => MemeMaker.GenerateMeme(request, text));
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


        private string? GetProvidedText()
        {
            if (Text is null) return null;

            return _cmd.IsMatch(Text) || Context.ChatIsPrivate ? RemoveCommand(Text) : null;
        }

        private MemeRequest GetRequestData()
        {
            var defaults = DefaultOptions;
            var dummy = string.Empty;
            var command = Text is null ? "" : Text.Split(split_chars, 2)[0].Replace(Config.BOT_USERNAME, "").ToLower();
            var empty = Text is null && defaults is null;

            if (!empty)
            {
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

        // todo still repeat if random options are used (watermarks, randoms colors, ...)
        private bool HasToBeRepeated => ConditionSatisfied(options => _repeat.IsMatch(options) && NoTextProvided);
        private bool SendAsSticker   => ConditionSatisfied(options => options.Contains('='));
        private bool NoTextProvided  => Text is null || (Text.StartsWith('/') && !Text.Any(x => split_chars.Contains(x)));

        private bool ConditionSatisfied(Predicate<string> condition)
        {
            if (Request.Empty) return false;

            var match = _cmd.Match(Request.Dummy);
            if (match.Success) return condition(match.Groups[1].Value);
            return false;
        }

        private int GetRepeatCount()
        {
            var repeats = 1;
            if (HasToBeRepeated)
            {
                var match = _repeat.Match(Request.Dummy);
                if (match.Success && int.TryParse(match.Value, out var x)) repeats = x;
            }
            return repeats;
        }

        private string RemoveCommand(string text) => _cmd.Replace(text, "");

        private bool ConvertStickerToJpeg() => ConditionSatisfied(ops => ops.Contains('x'));
    }

    public abstract class MakeMemeCore_Static : WitlessAsyncCommand
    {
        protected static readonly char[] split_chars = [' ', '\n'];
        
        protected static readonly Regex _repeat = new(@"[2-9]", RegexOptions.IgnoreCase);

        protected const string OPTIONS = "ℹ️ Список опций: ";
    }

    public class MemeRequest(string dummy, bool empty, string command)
    {
        /// <summary> A combination of command and default options. </summary>
        public string Dummy = dummy;

        /// <summary> <b>True</b> if both message text and default options are null. </summary>
        public readonly bool Empty = empty;

        /// <summary> Lowercase command text w/o bot username. </summary>
        public readonly string Command = command;
    }

    public class MemeFileRequest(string path, string oututEnding, int quality)
    {
        public string SourcePath { get; set; } = path;
        public string TargetPath { get; } = path.ReplaceExtension(oututEnding);

        public int Quality { get; set; } = quality; // 0 - 100

        public MemeSourceType  Type { get; init; }
        public bool ExportAsSticker { get; init; }
        public bool  ConvertSticker { get; init; }

        public bool IsSticker => Type == MemeSourceType.Sticker;

        /// <summary>
        /// Constant Rate Factor (for MP4 compresion).<br/>
        /// 0 - lossless, 23 - default, 51 - worst possible.
        /// </summary>
        public int GetCRF()
        {
            return Quality > 80
                ? 0
                : 51 - (int)(0.42 * Quality); // 17 - 51
        }

        /// <summary>
        /// Quality of JPEG image or MP3 audio.<br/>
        /// 1 - highest, 2-3 - default (JPEG), 31 - lowest.
        /// </summary>
        public int GetQscale()
        {
            return 31 - (int)(0.29 * Quality); // 2 - 31
        }

        public Image<Rgba32> GetVideoSnapshot()
        {
            return Image.Load<Rgba32>(FFMpegXD.Snapshot(SourcePath));
        }
    }

    public enum MemeSourceType
    {
        Image, Sticker, Video
    }
}