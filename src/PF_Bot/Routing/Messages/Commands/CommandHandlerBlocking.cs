namespace PF_Bot.Routing.Messages.Commands;

/// Blocking command. Should be used for short simple actions!
/// The same instance can be used unlimited amount of times.
public abstract class CommandHandlerBlocking : CommandHandler
{
    protected abstract void Run();

    protected sealed override
        Task Handle_Internal()
    {
        Run();

        return Task.CompletedTask;
    }
}