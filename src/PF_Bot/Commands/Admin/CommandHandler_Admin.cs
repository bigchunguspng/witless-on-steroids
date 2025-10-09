using PF_Bot.Routing.Commands;

namespace PF_Bot.Commands.Admin;

/// For commands that are restricted to bot admins only.
public abstract class CommandHandlerBlocking_Admin : CommandHandlerBlocking
{
    protected override void Run()
    {
        var admin = Message.SenderIsBotAdmin();
        if (admin) RunAuthourized();
        else       Deny(DenyReason.ONLY_BOT_ADMINS);
    }

    protected abstract void RunAuthourized();
}

/// <inheritdoc cref="CommandHandlerBlocking_Admin"/>
public abstract class CommandHandlerAsync_Admin : CommandHandlerAsync
{
    protected override async Task Run()
    {
        var admin = Message.SenderIsBotAdmin();
        if (admin) await RunAuthourized();
        else       Deny(DenyReason.ONLY_BOT_ADMINS);
    }

    protected abstract Task RunAuthourized();
}