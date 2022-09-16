using static Witlesss.Extension;
using static Witlesss.Strings;

namespace Witlesss.Commands
{
    public class ChatInfo : WitlessCommand
    {
        public override void Run()
        {
            string info =
                string.Format(
                CHAT_INFO, Title,
                FileSize(Baka.Path),
                Baka.Interval,
                Baka.DgProbability,
                Baka.DemotivateStickers ? "ON" : "OFF");
            Bot.SendMessage(Chat, info);
        }
    }
}