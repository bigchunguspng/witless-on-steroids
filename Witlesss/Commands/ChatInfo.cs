namespace Witlesss.Commands
{
    public class ChatInfo : WitlessCommand
    {
        public override void Run()
        {
            string info = string.Format(CHAT_INFO, Title,
                FileSize(Baka.Path),
                Baka.Interval,
                Baka.Meme.Chance,
                Baka.Meme.Quality,
                Baka.Meme.Stickers ? "ON" : "OFF",
                Baka.Meme.Dye == ColorMode.Color ? "цветной" : "белый",
                Baka.Meme.Type == MemeType.Dg ? "демотиваторами" : "мемами",
                Baka.AdminsOnly ? "Админы 😎" : "Все 😚");
            if (ChatIsPrivate) info = info.Remove(info.LastIndexOf('\n'));
            Bot.SendMessage(Chat, info);
        }
    }
}