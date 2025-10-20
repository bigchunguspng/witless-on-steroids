using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs;

namespace PF_Bot.Routing.Messages.Auto;

public class PoopText(int chance) : MessageHandler
{
    private HandlingStatus Status = HandlingStatus.OK;

    public async Task Run(MessageContext context)
    {
        Context = context;
        try
        {
            await SendText();
        }
        catch (Exception exception)
        {
            Status = HandlingStatus.FAIL;

            Unluckies.Handle(exception, Context, $"FUNNY | {Title}");
        }
        finally
        {
            BigBrother.LogAuto(Chat, Status, Message, AutoType.TEXT, chance);
        }
    }

    private async Task SendText()
    {
        await Task.Delay(GetRealisticResponseDelay(Text));

        var baka = PackManager.GetBaka(Chat);
        var text = App.FunnyMessages.TryDequeue(Chat) ?? baka.Generate();

        App.Bot.SendMessage(Origin, text, preview: true);
        Log($"{Title} >> FUNNY");
    }

    private static int GetRealisticResponseDelay
        (string? text) => text == null
        ? 150
        : Math.Min(text.Length, 120) * 25; // 1 second / 40 characters, 3 seconds max
}