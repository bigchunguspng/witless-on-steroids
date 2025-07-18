using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Witlesss.Services.Sounds;

public class InlineRequestHandler
{
    private readonly Regex _gif_mode = new(@"^[g!*.@гж](?=[\s]|$)");

    public async Task HandleRequest(InlineQuery inline)
    {
        Telemetry.LogInline(inline.From.Id, inline.Query);

        var results = new List<InlineQueryResult>();

        var gif_mode = _gif_mode.IsMatch(inline.Query);
        var query = gif_mode
            ? inline.Query.Length < 3
                ? null
                : inline.Query.Substring(2)
            : inline.Query;

        var files = gif_mode
            ? GIF_DB .Instance.Search(query) 
            : SoundDB.Instance.Search(query);

        if (gif_mode)
            foreach (var file in files)
                results.Add(new InlineQueryResultCachedGif(file.Id, file.FileId));
        else
            foreach (var file in files)
                results.Add(new InlineQueryResultCachedVoice(file.Id, file.FileId, file.GetTitle()));

        await Bot.Instance.Client.AnswerInlineQuery(inline.Id, results.Take(50));

        var title = inline.From.GetFullNameTruncated();
        var mode = gif_mode ? "g" : "a";
        var query_log = string.IsNullOrWhiteSpace(query) ? "[empty]" : query;
        Log($"[@inline-{mode}] {title} >> {query_log}");
    }
}