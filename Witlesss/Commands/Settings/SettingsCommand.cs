namespace Witlesss.Commands.Settings;

/// <summary>
/// Use this class as a base for the commands
/// that can be restricted to admins only.
/// </summary>
public abstract class SettingsCommand : WitlessSyncCommand
{
    private bool /* when the */ SenderIsSus() // !😳
    {
        return Baka.AdminsOnly && Message.SenderIsAdmin().Result == false;
    }

    protected override void Run()
    {
        if (SenderIsSus()) return;

        RunAuthorized();
    }

    protected abstract void RunAuthorized();
}