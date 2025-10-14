using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Features_Aux.Settings.Commands;

public class SetPics : CommandHandlerAsync_SettingsBlocking
{
    protected override void RunAuthorized()
    {
        if (Args.TryParseAsInt(out var value))
        {
            Data.Pics = value.ClampByte();
            ChatManager.SaveChats();
            Bot.SendMessage(Origin, SET_P_RESPONSE.Format(Data.Pics).XDDD());
            Log($"{Title} >> MEME CHANCE >> {Data.Pics}%");
        }
        else
        {
            var message = SET_X_GUIDE.Format("Вероятность мемчиков", Data.Pics, "pics");
            SendManual(message);
        }
    }
}