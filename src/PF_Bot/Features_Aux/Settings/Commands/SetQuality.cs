using PF_Bot.Backrooms.Helpers;
using PF_Bot.Commands;
using PF_Bot.Features_Aux.Settings.Core;

namespace PF_Bot.Features_Aux.Settings.Commands;

public class SetQuality : CommandHandlerAsync_SettingsBlocking
{
    protected override void RunAuthorized()
    {
        if (Args.TryParseAsInt(out var value))
        {
            Data.Quality = value.ClampByte();
            ChatManager.SaveChats();
            Bot.SendMessage(Origin, string.Format(SET_Q_RESPONSE, Data.Quality).XDDD());
            Log($"{Title} >> QUALITY >> {Data.Quality}%");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Качество графики", Data.Quality, "quality");
            SendManual(message);
        }
    }
}