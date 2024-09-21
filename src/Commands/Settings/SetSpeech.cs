namespace Witlesss.Commands.Settings;

public class SetSpeech : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Baka.Speech = value.ClampByte();
            ChatService.SaveChatsDB();
            Bot.SendMessage(Chat, string.Format(SET_FREQUENCY_RESPONSE, Baka.Speech).XDDD());
            Log($"{Title} >> SPEECH >> {Baka.Speech}");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Вероятность ответа", Baka.Speech, "speech", RandomInt(0, 100));
            Bot.SendMessage(Chat, message);
        }
    }
}