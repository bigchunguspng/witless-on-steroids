namespace PF_Bot.Routing.Messages.Commands;

/// For commands that are restricted to bot admins only.
public abstract class CommandHandlerBlocking_Admin : CommandHandlerBlocking
{
    protected override CommandRequirements Requirements
        => CommandRequirements.BotAdmin;
}

/// <inheritdoc cref="CommandHandlerBlocking_Admin"/>
public abstract class CommandHandlerAsync_Admin : CommandHandlerAsync
{
    protected override CommandRequirements Requirements
        => CommandRequirements.BotAdmin;
}