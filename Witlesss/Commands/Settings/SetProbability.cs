using Witlesss.Backrooms.Helpers;

namespace Witlesss.Commands.Settings
{
    public class SetProbability : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            if (Args is not null && Context.HasIntArgument(out var value))
            {
                Baka.Meme.Chance = value;
                ChatsDealer.SaveChatList();
                Bot.SendMessage(Chat, string.Format(SET_P_RESPONSE, Baka.Meme.Chance).XDDD());
                Log($"{Title} >> MEME CHANCE >> {Baka.Meme.Chance}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "pics"));
        }
    }
}