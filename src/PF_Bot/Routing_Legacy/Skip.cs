using PF_Bot.Routing.Commands;

namespace PF_Bot.Routing_Legacy;

public class Skip : CommandHandlerBlocking
{
    protected override void Run()
    {
        Print($"{Context.Title} >> {Context.Text}", ConsoleColor.Gray);
    }
}