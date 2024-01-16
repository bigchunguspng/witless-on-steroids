using System.Collections.Generic;

namespace Witlesss.Commands
{
    public class ChatInfo : WitlessCommand
    {
        public override void Run()
        {
            var info = string.Format
            (
                CHAT_INFO, Title,
                FileSize(Baka.Path),
                Baka.Interval,
                Baka.Meme.Chance,
                Baka.Meme.Quality,
                Baka.Meme.Stickers ? "ON" : "OFF",
                Baka.Meme.Dye == ColorMode.Color ? "цветной" : "белый",
                types[Baka.Meme.Type],
                Baka.AdminsOnly ? "Админы 😎" : "Все 😚"
            );
            if (ChatIsPrivate) info = info.Remove(info.LastIndexOf('\n'));
            if (Baka.Meme.OptionsM is not null) info += string.Format(OPTIONS, "meme", Baka.Meme.OptionsM[5..]);
            if (Baka.Meme.OptionsT is not null) info += string.Format(OPTIONS, "top",  Baka.Meme.OptionsT[4..]);
            if (Baka.Meme.OptionsD is not null) info += string.Format(OPTIONS, "dp",   Baka.Meme.OptionsD[3..]);
            if (Baka.Meme.OptionsG is not null) info += string.Format(OPTIONS, "dg",   Baka.Meme.OptionsG[3..]);
            if (Baka.Meme.OptionsN is not null) info += string.Format(OPTIONS, "nuke", Baka.Meme.OptionsN[5..]);
            Bot.SendMessage(Chat, info); // todo sb
        }

        private const string OPTIONS = "\nОпции /{0}: <code>{1}</code>";

        private readonly Dictionary<MemeType, string> types = new()
        {
            { MemeType.Meme, "стают мемами"     },
            { MemeType.Dg,   "демотивируются💀" },
            { MemeType.Top,  "обретают подпись" },
            { MemeType.Dp,   "демотивируются👌" },
            { MemeType.Nuke, "фритюрятся🍤"     }
        };
    }
}
