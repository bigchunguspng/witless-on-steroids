namespace Witlesss.Commands
{
    public class SetProbability : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            if (Args is not null && Context.HasIntArgument(out var value))
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