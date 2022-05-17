using Witlesss.Also;

namespace Witlesss.Commands
{
    public class SetProbability : WitlessCommand
    {
        public override void Run()
        {
            if (Extension.HasIntArgument(Text, out int value))
            {
                Baka.DgProbability = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, Extension.SET_PROBABILITY_RESPONSE(Baka.DgProbability));
                Logger.Log($"{Title} >> DG PROBABILITY >> {Baka.DgProbability}%");
            }
            else
                Bot.SendMessage(Chat, Strings.SET_PROBABILITY_MANUAL);
        }
    }
}