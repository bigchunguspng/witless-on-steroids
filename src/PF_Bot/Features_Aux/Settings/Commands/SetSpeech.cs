using PF_Bot.Commands;
using PF_Bot.Features_Aux.Settings.Core;

namespace PF_Bot.Features_Aux.Settings.Commands;

public class SetSpeech : CommandHandlerAsync_SettingsBlocking
{
    protected override void RunAuthorized()
    {
        if (Args.TryParseAsInt(out var value))
        {
            Data.Speech = value.ClampByte();
            ChatManager.SaveChats();
            Bot.SendMessage(Origin, string.Format(SET_FREQUENCY_RESPONSE, Data.Speech).XDDD());
            Log($"{Title} >> SPEECH >> {Data.Speech}");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Вероятность ответа", Data.Speech, "speech");
            SendManual(message);
        }
    }
}