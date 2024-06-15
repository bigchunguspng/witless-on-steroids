using System;
using System.Threading.Tasks;

namespace Witlesss.Commands;

public class PoopText : WitlessAsyncCommand
{
    protected override async Task Run()
    {
        await Task.Delay(GetRealisticResponseDelay(150, Text));
        Bot.SendMessage(Chat, Baka.Generate());
        Log($"{Title} >> FUNNY");
    }

    private static int GetRealisticResponseDelay(int initialTime, string? text)
    {
        return text is null ? initialTime : Math.Min(text.Length, 120) * 25;
    }
}