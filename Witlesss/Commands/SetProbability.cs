namespace Witlesss.Commands
{
    public class SetProbability : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;

            if (HasIntArgument(Text, out int value))
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