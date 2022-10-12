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
                Baka.JpgQuality,
                Baka.DemotivateStickers ? "ON" : "OFF",
                Baka.AdminsOnly ? "Админы 😎" : "Все 😚");
            Bot.SendMessage(Chat, info);
        }
    }
}