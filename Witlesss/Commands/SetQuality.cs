namespace Witlesss.Commands
{
    public class SetQuality : SettingsCommand
    {
        protected override void ExecuteAuthorized()
        {
            if (Text.HasIntArgument(out int value))
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