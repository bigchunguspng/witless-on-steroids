using Witlesss.Backrooms.Helpers;

namespace Witlesss.Commands.Settings
{
    public class SetQuality : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            if (Args is not null && Context.HasIntArgument(out var value))
            {
                Baka.Quality = value.ClampByte();
                ChatsDealer.SaveChatList();
                Bot.SendMessage(Chat, string.Format(SET_Q_RESPONSE, Baka.Quality).XDDD());
                Log($"{Title} >> JPG QUALITY >> {Baka.Quality}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "quality"));
        }
    }
}