using PF_Bot.Backrooms.Helpers;
using PF_Bot.State.Chats;

namespace PF_Bot.Features.Manage.Settings;

public class SetQuality : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Data.Quality = value.ClampByte();
            ChatManager.SaveChatsDB();
            Bot.SendMessage(Origin, string.Format(SET_Q_RESPONSE, Data.Quality).XDDD());
            Log($"{Title} >> QUALITY >> {Data.Quality}%");
        }
        else
        {
            var message = string.Format(SET_X_GUIDE, "Качество графики", Data.Quality, "quality");
            Bot.SendMessage(Origin, message);
        }
    }
}