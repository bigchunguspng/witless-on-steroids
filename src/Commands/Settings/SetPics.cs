namespace Witlesss.Commands.Settings;

public class SetPics : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Baka.Pics = value.ClampByte();
            ChatService.SaveChatsDB();
            Bot.SendMessage(Chat, string.Format(SET_P_RESPONSE, Baka.Pics).XDDD());
            Log($"{Title} >> MEME CHANCE >> {Baka.Pics}%");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Вероятность мемчиков", Baka.Pics, "pics", RandomInt(0, 100));
            Bot.SendMessage(Chat, message);
        }
    }
}