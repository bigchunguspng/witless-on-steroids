using PF_Bot.Core.Chats;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Handlers.Manage.Settings
{
    public class ToggleAdmins : WitlessSyncCommand
    {
        protected override void Run()
        {
            if (Context.ChatIsPrivate)
            {
                Bot.SendMessage(Origin, GROUPS_ONLY_COMAND);
            }
            else if (Message.SenderIsAdmin().Result)
            {
                Data.AdminsOnly = Data.AdminsOnly.Janai();
                ChatManager.SaveChatsDB();
                var text = string.Format(ADMINS_RESPONSE, Data.AdminsOnly ? "только админы 😎" : "все участники 😚");
                Bot.SendMessage(Origin, text);
                Log($"{Title} >> ADMINS ONLY >> {(Data.AdminsOnly ? "YES" : "NO")}");
            }
        }
    }
}