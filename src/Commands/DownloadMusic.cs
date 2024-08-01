using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;

#pragma warning disable CS4014

namespace Witlesss.Commands;

public class DownloadMusic : AsyncCommand
{
    private static readonly Regex  _url = new(@"(http\S*|[A-Za-z0-9_-]{11,})");
    private static readonly Regex _args = new(@"(http\S*|[A-Za-z0-9_-]{11,})\s*(?:([\S\s][^-]+) - )?([\S\s]+)?");
    private static readonly Regex   _id = new(@"((\?v=)|(v\/)|(\.be\/)|(embed\/)|(u\/1\/))([A-Za-z0-9_-]{11,})");
    private static readonly Regex   _pl = new(@"list=([A-Za-z0-9_-]+)");
    private static readonly Regex  _ops = new(@"\/song(\S+)");


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

            var youTube = url.Contains("youtu");
            var idOrUrl    = youTube ? _id.Match(url).Groups[^1].Value : url;
            var playlistID = youTube ? _pl.Match(url).Groups[^1].Value : null;
            if (playlistID is null && idOrUrl.Length < 1) throw new Exception("no video or playlist id found");

            var match = _ops.Match(Command!);
            var options = match.Success ? match.Groups[1].Value.ToLower() : "";

            var playListIndex = youTube && playlistID is null
                ? null
                : Regex.Match(options, @"\d+").Value.NullOnEmpty();

            var message = Bot.PingChat(Chat, Responses.PLS_WAIT.PickAny());

            var task = new DownloadMusicTask(idOrUrl, youTube, Context, message)
            {
                PlaylistID    = playlistID,
                PlayListIndex = playListIndex,
                HighQuality    = options.Contains('q'),
                NameOnly       = options.Contains('n'),
                RemoveBrackets = options.Contains('c'),
                Uploader       = options.Contains('u'),
                CropSquare     = options.Contains('s'),
                ExtractThumb   = options.Contains('p') && cover is null,
                Cover  = cover,
                Artist = artist,
                Title  = title,
            };

            await Bot.RunOrThrow(task.RunAsync(), Chat, message);
        }
        else
            Bot.SendMessage(Chat, SONG_MANUAL, preview: false);
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