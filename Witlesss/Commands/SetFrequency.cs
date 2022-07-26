using static Witlesss.Also.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class SetFrequency : WitlessCommand
    {
        public override void Run()
        {
            if (HasIntArgument(Text, out int value))
            {
                Baka.Interval = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, SET_FREQUENCY_RESPONSE(Baka.Interval));
                Log($"{Title} >> FUNNY INTERVAL >> {Baka.Interval}");
            }
            else
                Bot.SendMessage(Chat, SET_FREQUENCY_MANUAL);
        }
    }
}