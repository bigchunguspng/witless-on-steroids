using PF_Bot.Core;
using PF_Bot.Routing_Legacy.Commands;

namespace PF_Bot.Features_Main.Text.Commands;

public class PoopText : WitlessAsyncCommand
{
    protected override async Task Run()
    {
        await Task.Delay(GetRealisticResponseDelay(Text));

        var text = App.FunnyMessages.TryDequeue(Origin.Chat) ?? Baka.Generate();

        Bot.SendMessage(Origin, text, preview: true);
        Log($"{Title} >> FUNNY");
    }

    private static int GetRealisticResponseDelay(string? text)
    {
        return text is null
            ? 150
            : Math.Min(text.Length, 120) * 25; // 1 second / 40 characters, 3 seconds max
    }
}