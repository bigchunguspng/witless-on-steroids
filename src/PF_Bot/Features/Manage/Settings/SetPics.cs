using PF_Bot.Backrooms.Helpers;
using PF_Bot.State.Chats;

namespace PF_Bot.Features.Manage.Settings;

public class SetPics : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Data.Pics = value.ClampByte();
            ChatManager.SaveChatsDB();
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