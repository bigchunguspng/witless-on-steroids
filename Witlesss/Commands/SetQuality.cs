using static Witlesss.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class SetQuality : WitlessCommand
    {
        public override void Run()
        {
            if (HasIntArgument(Text, out int value))
            {
                Baka.JpgQuality = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, $"Качество демотиваторов будет {Baka.JpgQuality}%");
                Log($"{Title} >> JPG QUALITY >> {Baka.JpgQuality}%");
            }
            else
                Bot.SendMessage(Chat, SET_QUALITY_MANUAL);
        }
    }
}