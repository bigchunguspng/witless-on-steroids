namespace PF_Bot.Routing.Commands;

/// Non-blocking command. Should be used for time consuming actions!
/// A new instance should be created every time!
public abstract class CommandHandlerAsync : CommandHandler
{
    protected abstract Task Run();

    protected sealed override async
        Task Handle_Internal()
    {
        await Run();
    }
}