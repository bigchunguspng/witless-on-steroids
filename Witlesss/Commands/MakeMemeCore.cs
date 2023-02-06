using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using StrInt     = System.Func<int, string>;
using MemeMaker  = System.Func<string, Witlesss.Extension.DgText, string>;
using MemeMakerX = System.Func<string, Witlesss.Extension.DgText, string, string>;

namespace Witlesss.Commands
{
    public abstract class MakeMemeCore : WitlessCommand
    {
        private DateTime _time;
        private string   _path;

        protected Memes M => Bot.MemeService;

        protected void Run(Func<Message, bool> process, string type)
        {
            JpegCoder.PassQuality(Baka);

            var x = Message.ReplyToMessage;
            if (process(Message) || process(x)) return;

            Bot.SendMessage(Chat, string.Format(MEME_MANUAL, type));
        }

        protected void DoPhoto(string fileID, Func<DgText> texts, StrInt log, MemeMaker produce, bool regex)
        {
            Download(fileID);

            var repeats = 1;
            if (regex)
            {
                var match = Regex.Match(Text, @"\d");
                if (match.Success && int.TryParse(match.Value, out int x)) repeats = x;
            }
            for (int i = 0; i < repeats; i++)
            {
                using var stream = File.OpenRead(produce(_path, texts()));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {log(repeats)}");
        }

        protected void DoStick(string fileID, Func<DgText> texts, string log, MemeMakerX produce)
        {
            Download(fileID);

            using var stream = File.OpenRead(produce(_path, texts(), GetStickerExtension()));
            Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> {log}");
        }

        protected void DoVideo(string fileID, Func<DgText> texts, string log, MemeMaker produce)
        {
            if (Bot.ChatIsBanned(Baka)) return;

            WriteTime();
            Download(fileID);

            using var stream = File.OpenRead(produce(_path, texts()));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
            Log($@"{Title} >> {log} >> TIME: {CheckStopWatch()}");
        }
        
        private void Download(string fileID) => Bot.Download(fileID, Chat, out _path);

        private string GetStickerExtension() => Text != null && Text.Contains('x') ? ".jpg" : ".png";

        private void   WriteTime() => _time  = DateTime.Now;
        private string CheckStopWatch() => $@"{DateTime.Now - _time:s\.fff}";
    }
}