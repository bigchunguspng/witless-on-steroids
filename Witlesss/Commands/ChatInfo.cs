namespace Witlesss.Commands
{
    public class ChatInfo : WitlessCommand
    {
        public override void Run()
        {
            string info = string.Format(CHAT_INFO, Title,
                FileSize(Baka.Path),
                Baka.Interval,
                Baka.DgProbability,
                Baka.JpgQuality,
                Baka.DemotivateStickers ? "ON" : "OFF",
                Baka.MemesType == MemeType.Dg ? "демотиваторами" : "мемами",
                Baka.AdminsOnly ? "Админы 😎" : "Все 😚");
            if (ChatIsPrivate) info = info.Remove(info.LastIndexOf('\n'));
            Bot.SendMessage(Chat, info);
        }
    }
}