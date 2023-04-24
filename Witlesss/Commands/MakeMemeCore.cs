﻿using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands
{
    public abstract class MakeMemeCore<T> : MakeMemeCore_Static
    {
        private string    _path;
        private MediaType _type;
        private readonly StopWatch _watch = new();

        private readonly Regex _cmd;

        protected MakeMemeCore(Regex cmd) => _cmd = cmd;

        protected bool ProcessMessage(Message mess)
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

        protected void DoPhoto(string fileID, Func<int, string> log, Func<string, T, string> produce, bool regex)
        {
            Download(fileID);

            var repeats = GetRepeats(regex);
            for (int i = 0; i < repeats; i++)
            {
                using var stream = File.OpenRead(produce(_path, Texts()));
                Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            }
            Log($"{Title} >> {log(repeats)}");
        }

        protected void DoStick(string fileID, string log, Func<string, T, string, string> produce)
        {
            Download(fileID);

            using var stream = File.OpenRead(produce(_path, Texts(), GetStickerExtension()));
            Bot.SendPhoto(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> {log}");
        }

        protected void DoVideo(string fileID, string log, Func<string, T, string> produce)
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Baka)) return;

            _watch.WriteTime();
            Download(fileID);

            if (_type == MediaType.Round) _path = Memes.CropVideoNote(_path);

            using var stream = File.OpenRead(produce(_path, Texts()));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
            Log($@"{Title} >> {log} >> TIME: {_watch.CheckStopWatch()}");
        }

        protected abstract T GetMemeText(string text);
        private T Texts() => GetMemeText(RemoveCommand(Text));

        private string RemoveCommand(string text) => text == null ? null : _cmd.Replace(text, "");

        private string GetStickerExtension() => Text != null && _cmd.Match(Text).Value.Contains('x') ? ".jpg" : ".png";
        
        private void Download(string fileID) => Bot.Download(fileID, Chat, out _path, out _type);
    }

    public abstract class MakeMemeCore_Static : WitlessCommand
    {
        protected static Memes M => Bot.MemeService;

        protected static void Run(Func<Message, bool> process, string type)
        {
            JpegCoder.PassQuality(Baka);

            var x = Message.ReplyToMessage;
            if (process(Message) || process(x)) return;

            Bot.SendMessage(Chat, string.Format(MEME_MANUAL, type));
        }

        public static int GetRepeats(bool regex)
        {
            var repeats = 1;
            if (regex)
            {
                var match = Regex.Match(Text, @"\d");
                if (match.Success && int.TryParse(match.Value, out int x)) repeats = x;
            }
            return repeats;
        }
    }
}