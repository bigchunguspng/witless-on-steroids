using PF_Bot.Routing.Commands;

namespace PF_Bot.Routing;

public class Skip : SyncCommand
{
    protected override void Run()
    {
        Print($"{Context.Title} >> {Context.Text}", ConsoleColor.Gray);
    }
}