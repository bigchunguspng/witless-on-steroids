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
        private string    _path;
        private MediaType _type;
        private readonly Stopwatch _watch = new();

        private (long Chat, int Message, DateTime Date, string Dummy, bool Empty, string Command) _lastRequest;

        protected abstract Regex _cmd { get; }

        protected abstract string Log_PHOTO ( int x);
        protected abstract string Log_STICK ( int x);
        protected abstract string Log_VIDEO { get; }
        protected abstract string VideoName { get; }

        protected abstract string Options { get; }
        protected abstract string Command { get; }

        protected virtual bool CropVideoNotes { get; } = true;

        protected void Run(string type, string options = null)
        {
            JpegCoder.PassQuality(Baka);

            var x = Message.ReplyToMessage;
            if (ProcessMessage(Message) || ProcessMessage(x)) return;

            var message = string.Format(MEME_MANUAL, type);
            Bot.SendMessage(Chat, options is null ? message : $"{message}\n\n{options}");
        }

        private bool ProcessMessage(Message mess)
        {
            if (mess is null) return false;
            
            if      (mess.Photo     is not null)              ProcessPhoto(mess.Photo[^1].FileId);
            else if (mess.Animation is not null)              ProcessVideo(mess.Animation.FileId);
            else if (mess.Sticker   is { IsVideo: true })     ProcessVideo(mess.Sticker  .FileId);
            else if (mess.Video     is not null)              ProcessVideo(mess.Video    .FileId);
            else if (mess.VideoNote is not null)              ProcessVideo(mess.VideoNote.FileId);
            else if (mess.Sticker   is { IsAnimated: false }) ProcessStick(mess.Sticker  .FileId);
            else return false;
            
            return true;
        }

        public    abstract void ProcessPhoto(string fileID);
        public    abstract void ProcessStick(string fileID);
        protected abstract void ProcessVideo(string fileID);

        protected void DoPhoto(string fileID, Func<string, T, string> produce)
        {
            Download(fileID);

            var repeats = GetRepeats(HasToBeRepeated());
            for (int i = 0; i < repeats; i++)
            {
                using var stream = File.OpenRead(produce(_path, Texts()));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_PHOTO(repeats)}");
        }

        protected void DoStick(string fileID, Func<string, T, string, string> produce, bool convert = true)
        {
            Download(fileID);

            Memes.Sticker = true;

            var repeats = GetRepeats(HasToBeRepeated());
            var sticker = IsAprilFools() && Any() || SendAsSticker();
            var extension = GetStickerExtension();
            for (int i = 0; i < repeats; i++)
            {
                var result = produce(_path, Texts(), extension);
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

            _watch.WriteTime();
            Download(fileID);

            if (CropVideoNotes && _type == MediaType.Round) _path = Memes.CropVideoNote(_path);

            using var stream = File.OpenRead(produce(_path, Texts()));
            if (CropVideoNotes || _type != MediaType.Round)
                Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoName));
            else
                Bot.SendVideoNote(Chat, new InputOnlineFile(stream));

            Log($@"{Title} >> {Log_VIDEO} >> TIME: {_watch.CheckElapsed()}");
        }

        protected abstract T GetMemeText(string text);
        private T Texts() => GetMemeText(RemoveCommand(GetTextOrNull()));

        private string GetTextOrNull()
        {
            return TextStartsWithCommand() || MessageIsNotReposted() && Baka.Meme.Chance == 100 ? Text : null;
        }

        private bool TextStartsWithCommand() => Text != null && _cmd.IsMatch(Text);
        private bool  MessageIsNotReposted() => Message.ForwardFromChat is null;

        protected string GetDummy(out bool empty) => GetDummy(out empty, out _);
        protected string GetDummy(out bool empty, out string command)
        {
            var cached = _lastRequest is var x && x.Chat == Chat && x.Message == Message.MessageId && x.Date == MessageDateTime;
            if (cached)
            {
                empty = _lastRequest.Empty;
                command = _lastRequest.Command;
                return _lastRequest.Dummy;
            }

            command = GetCommand();

            _lastRequest.Chat = Chat;
            _lastRequest.Date = MessageDateTime;
            _lastRequest.Message = Message.MessageId;

                empty = Text is null && Options is null;
            if (empty)
                _lastRequest.Dummy = "";
            else
            {
                var hasOp = command.Length > Command.Length && command.StartsWith(Command);
                var plus  = hasOp && Options is not null && (command.Contains('+') || Options.Contains('+'));

                _lastRequest.Dummy = hasOp ? plus ? Options + command[Command.Length..] : command : Options ?? command;
            }
            _lastRequest.Empty = empty;
            _lastRequest.Command = command;
            return _lastRequest.Dummy;
        }

        private string GetCommand()
        {
            return Text is null ? "" : Text.Split(split_chars, 2)[0].Replace(Config.BOT_USERNAME, "").ToLower();
        }

        private bool HasToBeRepeated() => CheckForCondition(options => _repeat.IsMatch(options) && TextIsGenerated());
        private bool SendAsSticker  () => CheckForCondition(options => options.Contains('='));

        private bool CheckForCondition(Predicate<string> condition)
        {
            var dummy = GetDummy(out var empty);
            if (!empty)
            {
                var match = _cmd.Match(dummy);
                if (match.Success) return condition(match.Groups[1].Value);
            }
            return false;
        }

        private bool TextIsGenerated()
        {
            return Text is null || (Text.StartsWith('/') && !Text.Any(x => split_chars.Contains(x)));
        }

        private int GetRepeats(bool hasToBeRepeated)
        {
            var repeats = 1;
            if (hasToBeRepeated)
            {
                var dummy = GetDummy(out _);
                var match = _repeat.Match(dummy);
                if (match.Success && int.TryParse(match.Value, out int x)) repeats = x;
            }
            return repeats;
        }

        private string RemoveCommand(string text) => text == null ? null : _cmd.Replace(text, "");

        private string GetStickerExtension() => CheckForCondition(ops => ops.Contains('x')) ? ".jpg" : ".png";

        private void Download(string fileID) => Bot.Download(fileID, Chat, out _path, out _type);
    }

    public abstract class MakeMemeCore_Static : WitlessCommand
    {
        protected static readonly char[] split_chars = new[] { ' ', '\n' };
        
        protected static readonly Regex _repeat = new(@"(?:(?<![ms]s)(?<![ms]s\d)(?<![ms]s\d\d))[2-9](?!\d?%)", RegexOptions.IgnoreCase);

        protected const string OPTIONS = "ℹ️ Список опций: ";
    }
}