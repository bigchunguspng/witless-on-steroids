using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;

#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class DownloadMusic : AsyncCommand
    {
        private readonly Regex  _url = new(@"(http\S*|[A-Za-z0-9_-]{11,})");
        private readonly Regex _args = new(@"(http\S*|[A-Za-z0-9_-]{11,})\s*(?:([\S\s][^-]+) - )?([\S\s]+)?");
        private readonly Regex   _id = new(@"(?:(?:\?v=)|(?:v\/)|(?:\.be\/)|(?:embed\/)|(?:u\/1\/))([A-Za-z0-9_-]{11,})");
        private readonly Regex   _pl = new(@"list=([A-Za-z0-9_-]+)");
        private readonly Regex  _ops = new(@"\/song(\S+)");


        // input: /song[options] URL [artist - ][title]
        protected override async Task Run()
        {
            var arguments = ArgsWithURL();

            var cover = GetPhotoFileID(Message) ?? GetPhotoFileID(Message.ReplyToMessage);

            var args = _args.Match(arguments ?? "");
            if (args.Success)
            {
                var url    = args.Groups[1].Value;
                var artist = args.Groups[2].Success ? args.Groups[2].Value : null;
                var title  = args.Groups[3].Success ? args.Groups[3].Value : null;

                var yt = url.Contains("youtu");
                var id = yt ? _id.Match(url).Groups[1].Value : url;
                var pl = yt ? _pl.Match(url).Groups[1].Value : null;
                if (id.Length < 1 && pl is null) throw new Exception("no video or playlist id found");

                var ops = _ops.Match(Command!);
                var options = ops.Success ? ops.Groups[1].Value.ToLower() : "";

                var message = Bot.PingChat(Chat, Responses.PLS_WAIT.PickAny());

                var task = new DownloadMusicTask(id, options, cover, message, yt, pl, Context)
                {
                    Artist = artist,
                    Title = title
                };

                await Bot.RunSafelyAsync(task.RunAsync(), Chat, message);
            }
            else
            {
                Bot.SendMessage(Chat, SONG_MANUAL, preview: false);
            }
        }

        private string? ArgsWithURL()
        {
            var urlProvided = Args is not null && _url.IsMatch(Args);
            if (urlProvided) return Args;

            var text = Message.ReplyToMessage?.GetTextOrCaption();
            if (text is null) return Args;

            var match = _url.Match(text);
            return match.Success ? Args is null ? match.Value : $"{match.Value} {Args}" : Args;
        }

        private string? GetPhotoFileID(Message? message) => message?.Photo is { } p ? p[^1].FileId : null;
    }
}