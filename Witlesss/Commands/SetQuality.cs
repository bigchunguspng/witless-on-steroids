namespace Witlesss.Commands
{
    public class SetQuality : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;

            if (HasIntArgument(Text, out int value))
            {
                Baka.JpgQuality = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, XD(string.Format(SET_Q_RESPONSE, Baka.JpgQuality)));
                Log($"{Title} >> JPG QUALITY >> {Baka.JpgQuality}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "q"));
        }
    }
}