using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace Witlesss.Services.Sounds;

public class InlineRequestHandler
{
    private readonly Regex _sound_mode = new(@"^[as!*.@аз](?=[\s]|$)");
    private readonly Regex _caption  = new(@"^(.*?)\|\s?(.*)$");

    // @bot [g!*.@гж] query[|caption]

    public async Task HandleRequest(InlineQuery inline)
    {
        Telemetry.LogInline(inline.From.Id, inline.Query);

        string? query = inline.Query, caption = null;

        var captionMatch = _caption.Match(query);
        if (captionMatch.Success)
        {
            query   = captionMatch.ExtractGroup(1, s => s);
            caption = captionMatch.ExtractGroup(2, s => s);
        }

        var sound_mode = !string.IsNullOrWhiteSpace(query) && _sound_mode.IsMatch(query);
        if (sound_mode)
        {
            query = query!.Length < 3
                ? null
                : query.Substring(2);
        }

        var results = GetResults(sound_mode, query, caption);

        await Bot.Instance.Client.AnswerInlineQuery(inline.Id, results.Take(50));

        var title = inline.From.GetFullNameTruncated();
        var mode = sound_mode ? "a" : "g";
        var   query_log = string.IsNullOrWhiteSpace(query)   ? "[empty]"    :       query;
        var caption_log = string.IsNullOrWhiteSpace(caption) ? string.Empty : $" | {caption}";
        Log($"[@inline-{mode}] {title} >> {query_log}{caption_log}");
    }

    private List<InlineQueryResult> GetResults(bool sound_mode, string? query, string? caption)
    {
        var results = new List<InlineQueryResult>();

        var files = sound_mode
            ? SoundDB.Instance.Search(query) 
            : GIF_DB .Instance.Search(query);

        if (sound_mode)
            foreach (var file in files)
                results.Add(new InlineQueryResultCachedVoice(file.Id, file.FileId, file.GetTitle())
                {
                    Caption = caption, ParseMode = ParseMode.Html
                });
        else
            foreach (var file in files)
                results.Add(new InlineQueryResultCachedGif(file.Id, file.FileId)
                {
                    Caption = caption, ParseMode = ParseMode.Html
                });

        return results;
    }
}