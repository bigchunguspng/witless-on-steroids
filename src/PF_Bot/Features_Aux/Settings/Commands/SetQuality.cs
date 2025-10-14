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
            Bot.SendMessage(Origin, SET_Q_RESPONSE.Format(Data.Quality).XDDD());
            Log($"{Title} >> QUALITY >> {Data.Quality}%");
        }
        else
        {
            var message = SET_X_GUIDE.Format("Качество графики", Data.Quality, "quality");
            SendManual(message);
        }
    }
}