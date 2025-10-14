using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Features_Aux.Settings.Commands;

public class SetSpeech : CommandHandlerAsync_SettingsBlocking
{
    protected override void RunAuthorized()
    {
        if (Args.TryParseAsInt(out var value))
        {
            Data.Speech = value.ClampByte();
            ChatManager.SaveChats();
            Bot.SendMessage(Origin, SET_FREQUENCY_RESPONSE.Format(Data.Speech).XDDD());
            Log($"{Title} >> SPEECH >> {Data.Speech}");
        }
        else
        {
            var message = SET_X_GUIDE.Format("Вероятность ответа", Data.Speech, "speech");
            SendManual(message);
        }
    }
}