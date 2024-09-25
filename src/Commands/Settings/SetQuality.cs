namespace Witlesss.Commands.Settings;

public class SetQuality : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Baka.Quality = value.ClampByte();
            ChatService.SaveChatsDB();
            Bot.SendMessage(Chat, string.Format(SET_Q_RESPONSE, Baka.Quality).XDDD());
            Log($"{Title} >> JPG QUALITY >> {Baka.Quality}%");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Качество графики", Baka.Quality, "quality");
            Bot.SendMessage(Chat, message);
        }
    }
}