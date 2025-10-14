using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Features_Aux.Settings.Commands
{
    public class ToggleAdmins : CommandHandlerAsync
    {
        protected override CommandRequirements Requirements
            => CommandRequirements.KnownChat;

        protected override async Task Run()
        {
            if (Chat.ChatIsPrivate())
            {
                Deny(DenyReason.ONLY_GROUPS);
            }
            else if (await Message.SenderIsChatAdmin())
            {
                Data.AdminsOnly = Data.AdminsOnly.Janai();
                ChatManager.SaveChats();
                var text = ADMINS_RESPONSE.Format(Data.AdminsOnly ? "только админы 😎" : "все участники 😚");
                Bot.SendMessage(Origin, text);
                Log($"{Title} >> ADMINS ONLY >> {(Data.AdminsOnly ? "YES" : "NO")}");
            }
            else
                Deny(DenyReason.ONLY_CHAT_ADMINS);
        }
    }
}