namespace Witlesss.Commands
{
    public class SetProbability : SettingsCommand
    {
        protected override void ExecuteAuthorized()
        {
            if (Text.HasIntArgument(out int value))
            {
                Baka.Meme.Chance = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, XDDD(string.Format(SET_P_RESPONSE, Baka.Meme.Chance)));
                Log($"{Title} >> MEME CHANCE >> {Baka.Meme.Chance}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "pics"));
        }
    }
}