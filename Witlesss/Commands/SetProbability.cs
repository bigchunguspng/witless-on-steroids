using static Witlesss.Also.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class SetProbability : WitlessCommand
    {
        public override void Run()
        {
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