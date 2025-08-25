namespace PF_Bot.Commands.Settings;

public class SetPics : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Data.Pics = value.ClampByte();
            ChatService.SaveChatsDB();
            Bot.SendMessage(Origin, string.Format(SET_P_RESPONSE, Data.Pics).XDDD());
            Log($"{Title} >> MEME CHANCE >> {Data.Pics}%");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Вероятность мемчиков", Data.Pics, "pics");
            Bot.SendMessage(Origin, message);
        }
    }
}