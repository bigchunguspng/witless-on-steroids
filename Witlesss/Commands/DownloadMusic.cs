using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class DownloadMusic : Command
    {
        private readonly Regex _args = new(@"^\/song\S*\s(http\S*)\s*(?:([\S\s][^-]+) - )?([\S\s]+)?");
        private readonly Regex   _id = new(@"(?:(?:\?v=)|(?:v\/)|(?:\.be\/)|(?:embed\/)|(?:u\/1\/))([\w\d-]+)");
        private readonly Regex  _ops = new(@"\/song(\S+)");

        

        // input: /song[options] URL [artist - ][title]
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            var text = Text;
            var reply = Message.ReplyToMessage;
            if (reply is { Text: { } t } && !text.Contains("http") && t.StartsWith("http") && !t.Contains(' '))
            {
                var s = Text.Split(' ', 2);
                text = s.Length == 2 ? $"{s[0]} {t} {s[1]}" : $"{s[0]} {t}";
            }

            var cover = GetPhotoFileID(Message) ?? GetPhotoFileID(reply);

            var args = _args.Match(text);

            if (args.Success)
            {
                var url    = args.Groups[1].Value;
                var artist = args.Groups[2].Success ? args.Groups[2].Value : null;
                var title  = args.Groups[3].Success ? args.Groups[3].Value : null;

                var yt = url.Contains("youtu");
                var id = yt ? _id.Match(url).Groups[1].Value : url;
                if (id.Length < 1) throw new Exception("video id was too small");

                var ops = _ops.Match(RemoveBotMention());
                var options = ops.Success ? ops.Groups[1].Value.ToLower() : "";

                var message = Bot.PingChat(Chat, Pick(PLS_WAIT_RESPONSE));

                var task = new DownloadMusicTask(id, options, cover, message, yt, SnapshotMessageData())
                {
                    Artist = artist,
                    Title = title
                };

                Bot.RunSafelyAsync(task.RunAsync(), Chat, message);
            }
            else
            {
                Bot.SendMessage(Chat, SONG_MANUAL);
            }
        }

        private string GetPhotoFileID(Message message) => message?.Photo is { } p ? p[^1].FileId : null;
    }
}