using Witlesss.Backrooms.Helpers;

namespace Witlesss.Commands.Settings
{
    public class SetProbability : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            if (Args is not null && Context.HasIntArgument(out var value))
            {
                Baka.Pics = value.ClampByte();
                ChatsDealer.SaveChatList();
                Bot.SendMessage(Chat, string.Format(SET_P_RESPONSE, Baka.Pics).XDDD());
                Log($"{Title} >> MEME CHANCE >> {Baka.Pics}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "pics"));
        }
    }
}