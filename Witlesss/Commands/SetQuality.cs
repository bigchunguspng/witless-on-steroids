namespace Witlesss.Commands
{
    public class SetQuality : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;

            if (HasIntArgument(Text, out int value))
            {
                Baka.Meme.Quality = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, XDDD(string.Format(SET_Q_RESPONSE, Baka.Meme.Quality)));
                Log($"{Title} >> JPG QUALITY >> {Baka.Meme.Quality}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "quality"));
        }
    }
}