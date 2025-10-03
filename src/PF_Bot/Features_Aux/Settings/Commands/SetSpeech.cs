using PF_Bot.Backrooms.Helpers;
using PF_Bot.Commands;
using PF_Bot.Features_Aux.Settings.Core;

namespace PF_Bot.Features_Aux.Settings.Commands;

public class SetSpeech : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Data.Speech = value.ClampByte();
            ChatManager.SaveChats();
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