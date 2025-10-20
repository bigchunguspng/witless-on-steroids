using PF_Bot.Features_Web.Piracy.Core;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

#pragma warning disable CS4014

namespace PF_Bot.Features_Web.Piracy.Commands;

public class DownloadMusic : CommandHandlerAsync
{
    private static readonly Regex
        _rgx_url  = new(@"(http\S*)", RegexOptions.Compiled),
        _rgx_args = new(@"(http\S*)\s*(?:([\S\s][^-]+) - )?([\S\s]+)?", RegexOptions.Compiled),
        _rgx_id   = new(@"((\?v=)|(v\/)|(\.be\/)|(embed\/)|(u\/1\/))([A-Za-z0-9_-]{11,})", RegexOptions.Compiled),
        _rgx_pl   = new("list=([A-Za-z0-9_-]+)", RegexOptions.Compiled);


    // input: /song[options] URL [artist - ][title]
    protected override async Task Run()
    {
        var arguments = GetArgsWithURL();

        var cover = GetPhotoFileID(Message) ?? GetPhotoFileID(Message.ReplyToMessage);

        var args = _rgx_args.Match(arguments ?? "");
        if (args.Success)
        {
            var url    = args.Groups[1].Value;
            var artist = args.GroupOrNull(2);
            var title  = args.GroupOrNull(3);

            var youTube = url.Contains("youtu");
            var idOrUrl    = youTube ? _rgx_id.Match(url).Groups[^1].Value : url;
            var playlistID = youTube ? _rgx_pl.Match(url).Groups[^1].Value : null;
            if (playlistID is null && idOrUrl.Length < 1) throw new Exception("no video or playlist id found");

            var playListIndex = youTube && playlistID is null
                ? null
                : Options.MatchNumber().Value.MakeNull_IfEmpty();

            MessageToEdit = Bot.PingChat(Origin, PLS_WAIT.PickAny());

            var task = new DownloadMusicTask(idOrUrl, youTube, Context)
            {
                PlaylistID    = playlistID,
                PlayListIndex = playListIndex,
                HighQuality    = Options.Contains('q'),
                NameOnly       = Options.Contains('n'),
                RemoveBrackets = Options.Contains('c'),
                Uploader       = Options.Contains('u'),
                CropSquare     = Options.Contains('s'),
                ExtractThumb   = Options.Contains('p') && cover is null,
                Cover  = cover,
                Artist = artist,
                Title  = title,
            };

            var sw = Stopwatch.StartNew();

            var (mp3, jpg) = await task.RunAsync();

            Bot.DeleteMessageAsync(Chat, MessageToEdit);
            MessageToEdit = 0;

            await using var stream = File.OpenRead(mp3);
            Bot.SendAudio(Origin, InputFile.FromStream(stream, mp3), jpg);
            Log($"{Title} >> YOUTUBE MUSIC >> TIME: {sw.ElapsedReadable()}", LogLevel.Info, LogColor.Yellow);
        }
        else
            SendManual(SONG_MANUAL);
    }

    private string? GetArgsWithURL()
    {
        var urlMatchThis = _rgx_url.MatchOrNull(Args);
        if (urlMatchThis is { Success: true })
            return Args;

        var reply = Message.ReplyToMessage?.GetTextOrCaption();
        var urlMatchReply = _rgx_url.MatchOrNull(reply);
        if (urlMatchReply is { Success: true })
            return Args is null ? urlMatchReply.Value : $"{urlMatchReply.Value} {Args}";

        return Args;
    }

    private string? GetPhotoFileID(Message? message) => message?.Photo is { } p ? p[^1].FileId : null;
}