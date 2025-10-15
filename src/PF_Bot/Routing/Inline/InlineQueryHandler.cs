using PF_Bot.Core;
using PF_Bot.Features_Main.Media.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Inline;

public class InlineQueryHandler
{
    private static readonly MediaSearch Jarvis = new();

    public async Task Handle(InlineQuery inline)
    {
        var status = HandlingStatus.OK;
        try
        {
            status = await Jarvis.Search(inline);
        }
        catch (Exception exception)
        {
            status = HandlingStatus.FAIL;

            Unluckies.Handle(exception, inline, title: "INLINE H.");
        }
        finally
        {
            BigBrother.LogInline(inline.From.Id, status, inline.Query);
        }
    }
}