namespace Witlesss.Commands
{
    public class ChatInfo : WitlessCommand
    {
        public override void Run()
        {
            string info = string.Format(CHAT_INFO, Title,
                FileSize(Baka.Path),
                Baka.Interval,
                Baka.MemeChance,
                Baka.MemeQuality,
                Baka.MemeStickers ? "ON" : "OFF",
                Baka.MemeType == MemeType.Dg ? "демотиваторами" : "мемами",
                Baka.AdminsOnly ? "Админы 😎" : "Все 😚");
            if (ChatIsPrivate) info = info.Remove(info.LastIndexOf('\n'));
            Bot.SendMessage(Chat, info);
        }
    }
}