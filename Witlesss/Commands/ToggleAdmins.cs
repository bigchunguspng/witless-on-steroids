using static Witlesss.Extension;
using static Witlesss.Strings;

namespace Witlesss.Commands
{
    public class ToggleAdmins : WitlessCommand
    {
        public override void Run()
        {
            if (Chat > 0)
            {
                Bot.SendMessage(Chat, "Это команда только для бесед 🤓");
            }
            else if (SenderIsAdmin())
            {
                Baka.AdminsOnly = !Baka.AdminsOnly;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, string.Format(ADMINS_RESPONSE, Baka.AdminsOnly ? "только админы 😎" : "все участники 😚"));
            }
        }

        private bool SenderHasPermission() => !Baka.AdminsOnly || SenderIsAdmin();

        private bool SenderIsAdmin()
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

        protected bool SenderIsSus() => !SenderHasPermission();
    }
}