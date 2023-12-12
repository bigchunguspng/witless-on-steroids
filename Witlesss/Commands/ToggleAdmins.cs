namespace Witlesss.Commands
{
    public class ToggleAdmins : WitlessCommand
    {
        public override void Run()
        {
            if (ChatIsPrivate)
            {
                Bot.SendMessage(Chat, GROUPS_ONLY_COMAND);
            }
            else if (SenderIsAdmin())
            {
                Baka.AdminsOnly = !Baka.AdminsOnly;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, string.Format(ADMINS_RESPONSE, Baka.AdminsOnly ? "только админы 😎" : "все участники 😚"));
            }
        }

        protected static bool SenderIsAdmin()
        {
            if (Message.SenderChat != null)
            {
                if (Message.SenderChat.Id == Chat) return true;
                
                Bot.SendMessage(Chat, Pick(UNKNOWN_CHAT_RESPONSE));
            }
            else if (!Bot.UserIsAdmin(Message.From, Chat))
            {
                Bot.SendMessage(Chat, Pick(NOT_ADMIN_RESPONSE));
            }
            else return true;

            return false;
        }
    }

    /// <summary> Use this class for commands that <b>can be</b> restricted to admins only. </summary>
    public abstract class SettingsCommand : ToggleAdmins
    {
        private static bool SenderIsSus() => Baka.AdminsOnly && !SenderIsAdmin();

        public override void Run()
        {
            if (SenderIsSus()) return;

            ExecuteAuthorized();
        }

        protected abstract void ExecuteAuthorized();
    }
}