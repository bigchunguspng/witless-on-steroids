using System.Collections.Generic;

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
                types[Baka.Meme.Type],
                Baka.AdminsOnly ? "Админы 😎" : "Все 😚");
            if (ChatIsPrivate) info = info.Remove(info.LastIndexOf('\n'));
            Bot.SendMessage(Chat, info);
        }

        private readonly Dictionary<MemeType, string> types = new()
        {
            { MemeType.Meme, "стают мемами"     },
            { MemeType.Dg,   "демотивируются"   },
            { MemeType.Top,  "обретают подпись" }
        };
    }
}