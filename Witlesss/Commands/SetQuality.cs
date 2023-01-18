namespace Witlesss.Commands
{
    public class SetQuality : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;

            if (HasIntArgument(Text, out int value))
            {
                Baka.MemeQuality = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, XD(string.Format(SET_Q_RESPONSE, Baka.MemeQuality)));
                Log($"{Title} >> JPG QUALITY >> {Baka.MemeQuality}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "quality"));
        }
    }
}