using PF_Bot.Routing.Commands;

namespace PF_Bot.Commands;

public abstract class CommandHandlerAsync_Settings 
    :                 CommandHandlerAsync
{
    protected override CommandRequirements Requirements
        => CommandRequirements.KnownChat;

    protected async Task<bool>
        /* when the */ SenderIsSus() // !😳
        => Settings.AdminsOnly
        &&       Message. SenderIsBotAdmin() == false
        && await Message.SenderIsChatAdmin() == false;
}

/// For commands that can be restricted to chat admins only.
/// Is <see cref="CommandHandlerAsync"/> under the hood anyways!
public abstract class CommandHandlerAsync_SettingsBlocking 
    :                 CommandHandlerAsync_Settings
{
    protected sealed override async Task Run()
    {
        var sus = await SenderIsSus();
        if (sus) Deny(DenyReason.ONLY_CHAT_ADMINS);
        else     RunAuthorized();
    }

    protected abstract void RunAuthorized();
}

/// <inheritdoc cref="CommandHandlerAsync_SettingsBlocking"/>
public abstract class CommandHandlerAsync_SettingsAsync 
    :                 CommandHandlerAsync_Settings
{
    protected sealed override async Task Run()
    {
        var sus = await SenderIsSus();
        if (sus) Deny(DenyReason.ONLY_CHAT_ADMINS);
        else     await RunAuthorized();
    }

    protected abstract Task RunAuthorized();
}