using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Routing;

public class Skip : CommandAndCallbackRouter
{
    protected override void Run()
    {
        Print($"{Context.Title} >> {Context.Text}", ConsoleColor.Gray);
    }

    public override void OnCallback(CallbackQuery query)
    {
        Print(query.Data ?? "-", ConsoleColor.Yellow);
    }
}