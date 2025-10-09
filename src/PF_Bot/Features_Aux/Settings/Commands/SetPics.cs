using PF_Bot.Backrooms.Helpers;
using PF_Bot.Commands;
using PF_Bot.Features_Aux.Settings.Core;

namespace PF_Bot.Features_Aux.Settings.Commands;

public class SetPics : CommandHandlerAsync_SettingsBlocking
{
    protected override void RunAuthorized()
    {
        if (Args.TryParseAsInt(out var value))
        {
            Data.Pics = value.ClampByte();
            ChatManager.SaveChats();
            Bot.SendMessage(Origin, string.Format(SET_P_RESPONSE, Data.Pics).XDDD());
            Log($"{Title} >> MEME CHANCE >> {Data.Pics}%");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Вероятность мемчиков", Data.Pics, "pics");
            SendManual(message);
        }
    }
}