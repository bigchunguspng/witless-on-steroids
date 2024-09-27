namespace Witlesss.Commands.Settings;

public class SetQuality : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Data.Quality = value.ClampByte();
            ChatService.SaveChatsDB();
            Bot.SendMessage(Chat, string.Format(SET_Q_RESPONSE, Data.Quality).XDDD());
            Log($"{Title} >> QUALITY >> {Data.Quality}%");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Качество графики", Data.Quality, "quality");
            Bot.SendMessage(Chat, message);
        }
    }
}