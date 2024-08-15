namespace Witlesss.Commands.Generation;

public class PoopText : WitlessAsyncCommand
{
    protected override async Task Run()
    {
        await Task.Delay(GetRealisticResponseDelay(Text));

        Bot.SendMessage(Chat, Baka.Generate(), preview: true);
        Log($"{Title} >> FUNNY");
    }

    private static int GetRealisticResponseDelay(string? text)
    {
        return text is null
            ? 150
            : Math.Min(text.Length, 120) * 25; // 1 second / 40 characters, 3 seconds max
    }
}