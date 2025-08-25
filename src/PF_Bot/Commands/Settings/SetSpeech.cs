namespace PF_Bot.Commands.Settings;

public class SetSpeech : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Data.Speech = value.ClampByte();
            ChatService.SaveChatsDB();
            Bot.SendMessage(Origin, string.Format(SET_FREQUENCY_RESPONSE, Data.Speech).XDDD());
            Log($"{Title} >> SPEECH >> {Data.Speech}");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Вероятность ответа", Data.Speech, "speech");
            Bot.SendMessage(Origin, message);
        }
    }
}