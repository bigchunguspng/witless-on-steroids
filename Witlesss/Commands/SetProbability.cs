namespace Witlesss.Commands
{
    public class SetProbability : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;

            if (HasIntArgument(Text, out int value))
            {
                Baka.DgProbability = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, XD(string.Format(SET_P_RESPONSE, Baka.DgProbability)));
                Log($"{Title} >> DG PROBABILITY >> {Baka.DgProbability}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "p"));
        }
    }
}