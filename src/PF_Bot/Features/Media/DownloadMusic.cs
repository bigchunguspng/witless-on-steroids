using PF_Bot.Routing.Commands;
using PF_Bot.Tools_Legacy.YtDlp;
using Telegram.Bot.Types;

#pragma warning disable CS4014

namespace PF_Bot.Features.Media;

public class DownloadMusic : AsyncCommand
{
    private static readonly Regex  _url = new(@"(http\S*)");
    private static readonly Regex _args = new(@"(http\S*)\s*(?:([\S\s][^-]+) - )?([\S\s]+)?");
    private static readonly Regex   _id = new(@"((\?v=)|(v\/)|(\.be\/)|(embed\/)|(u\/1\/))([A-Za-z0-9_-]{11,})");
    private static readonly Regex   _pl = new(@"list=([A-Za-z0-9_-]+)");
    private static readonly Regex  _ops = new(@"\/song(\S+)");


    // input: /song[options] URL [artist - ][title]
    protected override async Task Run()
    {
        var arguments = GetArgsWithURL();

        var cover = GetPhotoFileID(Message) ?? GetPhotoFileID(Message.ReplyToMessage);

        var args = _args.Match(arguments ?? "");
        if (args.Success)
        {
            var url    = args.Groups[1].Value;
            var artist = args.GroupOrNull(2);
            var title  = args.GroupOrNull(3);

            var youTube = url.Contains("youtu");
            var idOrUrl    = youTube ? _id.Match(url).Groups[^1].Value : url;
            var playlistID = youTube ? _pl.Match(url).Groups[^1].Value : null;
            if (playlistID is null && idOrUrl.Length < 1) throw new Exception("no video or playlist id found");

            var options = _ops.ExtractGroup(1, Command!, s => s.ToLower(), "");

            var playListIndex = youTube && playlistID is null
                ? null
                : Regex.Match(options, @"\d+").Value.MakeNull_IfEmpty();

            var message = Bot.PingChat(Origin, PLS_WAIT.PickAny());

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
            Bot.SendMessage(Origin, SONG_MANUAL);
    }

    private string? GetArgsWithURL()
    {
        var urlMatchThis = _url.MatchOrNull(Args);
        if (urlMatchThis is { Success: true })
            return Args;

        var reply = Message.ReplyToMessage?.GetTextOrCaption();
        var urlMatchReply = _url.MatchOrNull(reply);
        if (urlMatchReply is { Success: true })
            return Args is null ? urlMatchReply.Value : $"{urlMatchReply.Value} {Args}";

        return Args;
    }

    private string? GetPhotoFileID(Message? message) => message?.Photo is { } p ? p[^1].FileId : null;
}