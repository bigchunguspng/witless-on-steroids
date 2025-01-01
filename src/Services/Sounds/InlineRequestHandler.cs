using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Witlesss.Services.Sounds;

public class InlineRequestHandler
{
    public async Task HandleRequest(InlineQuery inline)
    {
        var results = new List<InlineQueryResult>();

        var sounds = SoundDB.Instance.Search(inline.Query);
        foreach (var sound in sounds)
        {
            var id = Random.Shared.Next().ToString();
            results.Add(new InlineQueryResultCachedVoice(id, sound.FileId, sound.Text));
        }

        await Bot.Instance.Client.AnswerInlineQuery(inline.Id, results);

        var title = inline.From.GetFullNameTruncated();
        var query = string.IsNullOrEmpty(inline.Query) ? "[empty]" : inline.Query;
        Log($"[@inline] {title} >> {query}");
    }
}