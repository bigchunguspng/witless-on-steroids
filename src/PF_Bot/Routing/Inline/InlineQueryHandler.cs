using PF_Bot.Core;
using PF_Bot.Features_Main.Media.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Inline;

public class InlineQueryHandler
{
    private readonly MediaSearch _Jarvis = new();

    public async Task Handle(InlineQuery inline)
    {
        Telemetry.LogInline(inline.From.Id, inline.Query);

        await _Jarvis.Search(inline);
    }
}