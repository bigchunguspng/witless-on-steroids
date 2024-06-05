using System;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.MediaTools;

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

        protected abstract string? DefaultOptions { get; }

        protected virtual bool CropVideoNotes { get; } = true;

        protected void Run(string type, string? options = null)
        {
            ImageSaver.PassQuality(Baka);

            var x = Message.ReplyToMessage;
            if (ProcessMessage(Message) || ProcessMessage(x)) return;

            var message = string.Format(MEME_MANUAL, type);
            Bot.SendMessage(Chat, options is null ? message : $"{message}\n\n{options}");
        }

        private bool ProcessMessage(Message? message)
        {
            if (message is null) return false;

            if      (message.Photo     is not null)              ProcessPhoto(message.Photo[^1].FileId);
            else if (message.Animation is not null)              ProcessVideo(message.Animation.FileId);
            else if (message.Sticker   is { IsVideo: true })     ProcessVideo(message.Sticker  .FileId);
            else if (message.Video     is not null)              ProcessVideo(message.Video    .FileId);
            else if (message.VideoNote is not null)              ProcessVideo(message.VideoNote.FileId);
            else if (message.Sticker   is { IsAnimated: false }) ProcessStick(message.Sticker  .FileId);
            else return false;

            return true;
        }

        public    abstract void ProcessPhoto(string fileID);
        public    abstract void ProcessStick(string fileID);
        protected abstract void ProcessVideo(string fileID);

        protected void DoPhoto(string fileID, Func<string, T, string> produce)
        {
            Bot.Download(fileID, Chat, out var path);
            Request = GetRequestData();

            ParseOptions();
            var repeats = GetRepeatCount();
            for (var i = 0; i < repeats; i++)
            {
                var text = GetMemeText(GetText());
                using var stream = File.OpenRead(produce(path, text));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_PHOTO(repeats)}");
        }

        protected void DoStick(string fileID, Func<string, T, string, string> produce, bool convert = true)
        {
            Bot.Download(fileID, Chat, out var path);
            Request = GetRequestData();

            Memes.Sticker = true;

            ParseOptions();
            var repeats = GetRepeatCount();
            var sticker = SendAsSticker;
            var extension = GetStickerExtension();
            for (var i = 0; i < repeats; i++)
            {
                var text = GetMemeText(GetText());
                var result = produce(path, text, extension);
                if (sticker && convert)
                    result = new F_Process(result).Output("-stick", ".webp");
                using var stream = File.OpenRead(result);
                if (sticker) Bot.SendSticker(Chat, new InputOnlineFile(stream));
                else         Bot.SendPhoto  (Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_STICK(repeats)}");
        }

        protected void DoVideo(string fileID, Func<string, T, string> produce)
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Baka)) return;

            var sw = Helpers.GetStartedStopwatch();
            Bot.Download(fileID, Chat, out var path, out var type);
            Request = GetRequestData();

            if (CropVideoNotes && type == MediaType.Round) path = Memes.CropVideoNote(path);

            ParseOptions();
            var text = GetMemeText(GetText());
            using var stream = File.OpenRead(produce(path, text));
            var note = type == MediaType.Round && !CropVideoNotes;
            if (note) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            else      Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoName));

            Log($@"{Title} >> {Log_VIDEO} >> TIME: {FormatTime(sw.Elapsed)}");
        }

        protected abstract void ParseOptions();
        protected abstract T GetMemeText(string? text);


        private string? GetText()
        {
            if (Text is null) return null;

            return _cmd.IsMatch(Text) || ChatIsPrivate ? RemoveCommand(Text) : null;
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

        private string GetStickerExtension() => ConditionSatisfied(ops => ops.Contains('x')) ? ".jpg" : ".png";
    }

    public abstract class MakeMemeCore_Static : WitlessCommand
    {
        protected static readonly char[] split_chars = [' ', '\n'];
        
        protected static readonly Regex _repeat = new(@"(?:(?<![ms]s)(?<![ms]s\d)(?<![ms]s\d\d))[2-9](?!\d?%)", RegexOptions.IgnoreCase);

        protected const string OPTIONS = "ℹ️ Список опций: ";
    }

    public class MemeRequest(string dummy, bool empty, string command)
    {
        /// <summary> A combination of command and default options. </summary>
        public string Dummy = dummy;

        /// <summary> <b>True</b> if both message text and default options are null. </summary>
        public bool Empty = empty;

        /// <summary> Lowercase command text w/o bot username. </summary>
        public string Command = command;
    }
}