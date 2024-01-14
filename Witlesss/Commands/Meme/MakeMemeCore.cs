using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Meme // ReSharper disable InconsistentNaming
{
    public abstract class MakeMemeCore<T> : MakeMemeCore_Static
    {
        private string    _path;
        private MediaType _type;
        private readonly Stopwatch _watch = new();

        protected abstract Regex _cmd { get; }

        protected abstract string Log_PHOTO ( int x);
        protected abstract string Log_STICK ( int x);
        protected abstract string Log_VIDEO { get; }
        protected abstract string VideoName { get; }

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

        protected void DoStick(string fileID, Func<string, T, string, string> produce)
        {
            Download(fileID);

            var repeats = GetRepeats(HasToBeRepeated());
            for (int i = 0; i < repeats; i++)
            {
                using var stream = File.OpenRead(produce(_path, Texts(), GetStickerExtension()));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {Log_STICK(repeats)}");
        }

        protected void DoVideo(string fileID, Func<string, T, string> produce)
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Baka)) return;

            _watch.WriteTime();
            Download(fileID);

            if (_type == MediaType.Round) _path = Memes.CropVideoNote(_path);

            using var stream = File.OpenRead(produce(_path, Texts()));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoName));
            Log($@"{Title} >> {Log_VIDEO} >> TIME: {_watch.CheckElapsed()}");
        }

        protected abstract T GetMemeText(string text);
        private T Texts() => GetMemeText(RemoveCommand(GetTextUnlessItsReposted()));

        private string GetTextUnlessItsReposted() => Message.ForwardFromChat is null ? Text : null;

        protected string GetDummy(string options, string command, out bool empty)
        {
                empty = Text is null && options is null;
            if (empty) return "";

            var cmd   = Text is null ? "" : Text.Split(split_chars, 2)[0].Replace(Config.BOT_USERNAME, "").ToLower();
            var hasOp = cmd.Length > command.Length && cmd.StartsWith(command);
            var plus  = hasOp && cmd.Contains('+');

            return hasOp ? plus ? options + cmd[command.Length..] : cmd : options ?? cmd;
        }

        private bool HasToBeRepeated()
        {
            if (Text is null) return false;
            var cmd = _cmd.Match(Text);
            if (cmd.Success) return _repeat.IsMatch(cmd.Groups[1].Value);
            return false;
        }

        private string RemoveCommand(string text) => text == null ? null : _cmd.Replace(text, "");

        private string GetStickerExtension() => Text != null && _cmd.Match(Text).Value.Contains('x') ? ".jpg" : ".png";
        
        private void Download(string fileID) => Bot.Download(fileID, Chat, out _path, out _type);
    }

    public abstract class MakeMemeCore_Static : WitlessCommand
    {
        protected static readonly char[] split_chars = new[] { ' ', '\n' };
        
        protected static readonly Regex _repeat = new(@"(?:(?<![ms]s)(?<![ms]s\d)(?<![ms]s\d\d))[2-9](?!\d?%)", RegexOptions.IgnoreCase);

        public static int GetRepeats(bool regex)
        {
            var repeats = 1;
            if (regex)
            {
                var match = _repeat.Match(Text);
                if (match.Success && int.TryParse(match.Value, out int x)) repeats = x;
            }
            return repeats;
        }
    }
}