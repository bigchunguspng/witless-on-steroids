namespace Witlesss.Commands.Settings;

/// <summary>
/// Use this class as a base for the commands
/// that can be restricted to admins only.
/// </summary>
public abstract class SettingsCommand : WitlessSyncCommand
{
    private bool /* when the */ SenderIsSus() // !😳
    {
        return Data.AdminsOnly && Message.SenderIsAdmin().Result == false;
    }

    protected override void Run()
    {
        if (SenderIsSus()) return;

        RunAuthorized();
    }

    protected abstract void RunAuthorized();
}

/// <summary> <inheritdoc cref="SettingsCommand"/> </summary>
public abstract class AsyncSettingsCommand : WitlessAsyncCommand
{
    private async Task<bool> SenderIsSus()
    {
        return Data.AdminsOnly && await Message.SenderIsAdmin() == false;
    }

    protected override async Task Run()
    {
        if (await SenderIsSus()) return;

        await RunAuthorized();
    }

    protected abstract Task RunAuthorized();
}