using Witlesss.Also;

namespace Witlesss.Commands
{
    public class SetFrequency : WitlessCommand
    {
        public override void Run()
        {
            if (Extension.HasIntArgument(Text, out int value))
            {
                Baka.Interval = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, Extension.SET_FREQUENCY_RESPONSE(Baka.Interval));
                Logger.Log($"{Title} >> FUNNY INTERVAL >> {Baka.Interval}");
            }
            else
                Bot.SendMessage(Chat, Strings.SET_FREQUENCY_MANUAL);
        }
    }
}