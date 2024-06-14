namespace Witlesss.Commands
{
    public class ToggleAdmins : WitlessSyncCommand
    {
        protected override void Run()
        {
            if (Context.ChatIsPrivate)
            {
                Bot.SendMessage(Chat, GROUPS_ONLY_COMAND);
            }
            else if (Message.SenderIsAdmin())
            {
                Baka.AdminsOnly = !Baka.AdminsOnly;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, string.Format(ADMINS_RESPONSE, Baka.AdminsOnly ? "только админы 😎" : "все участники 😚"));
            }
        }
    }

    /// <summary> Use this class for commands that <b>can be</b> restricted to admins only. </summary>
    public abstract class SettingsCommand : WitlessSyncCommand
    {
        private bool /* when the */ SenderIsSus() // !😳
        {
            return Baka.AdminsOnly && !Message.SenderIsAdmin();
        }

        protected override void Run()
        {
            if (SenderIsSus()) return;

            RunAuthorized();
        }

        protected abstract void RunAuthorized();
    }
}