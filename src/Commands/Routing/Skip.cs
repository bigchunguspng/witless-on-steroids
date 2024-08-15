using Telegram.Bot.Types;

namespace Witlesss.Commands.Routing;

public class Skip : CommandAndCallbackRouter
{
    protected override void Run()
    {
        Log($"{Context.Title} >> {Context.Text}", ConsoleColor.Gray);
    }

    public override void OnCallback(CallbackQuery query)
    {
        Log(query.Data ?? "-", ConsoleColor.Yellow);
    }
}