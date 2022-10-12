using static Witlesss.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

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
                Bot.SendMessage(Chat, SET_PROBABILITY_RESPONSE(Baka.DgProbability));
                Log($"{Title} >> DG PROBABILITY >> {Baka.DgProbability}%");
            }
            else
                Bot.SendMessage(Chat, SET_PROBABILITY_MANUAL);
        }
    }
}