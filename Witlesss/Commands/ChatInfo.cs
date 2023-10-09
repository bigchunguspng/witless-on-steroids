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
            if (Baka.Meme.OptionsM is not null) info += $"\nОпции /meme: <code>{Baka.Meme.OptionsM[5..]}</code>";
            if (Baka.Meme.OptionsT is not null) info +=  $"\nОпции /top: <code>{Baka.Meme.OptionsT[4..]}</code>";
            if (Baka.Meme.OptionsD is not null) info +=   $"\nОпции /dp: <code>{Baka.Meme.OptionsD[3..]}</code>";
            Bot.SendMessage(Chat, info);
        }

        private readonly Dictionary<MemeType, string> types = new()
        {
            { MemeType.Meme, "стают мемами"     },
            { MemeType.Dg,   "демотивируются💀" },
            { MemeType.Top,  "обретают подпись" },
            { MemeType.Dp,   "демотивируются👌" }
        };
    }
}
