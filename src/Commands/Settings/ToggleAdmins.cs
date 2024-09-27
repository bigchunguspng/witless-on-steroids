namespace Witlesss.Commands.Settings
{
    public class ToggleAdmins : WitlessSyncCommand
    {
        protected override void Run()
        {
            if (Context.ChatIsPrivate)
            {
                Bot.SendMessage(Chat, GROUPS_ONLY_COMAND);
            }
            else if (Message.SenderIsAdmin().Result)
            {
                Data.AdminsOnly = !Data.AdminsOnly;
                ChatService.SaveChatsDB();
                var text = string.Format(ADMINS_RESPONSE, Data.AdminsOnly ? "только админы 😎" : "все участники 😚");
                Bot.SendMessage(Chat, text);
            }
        }
    }
}