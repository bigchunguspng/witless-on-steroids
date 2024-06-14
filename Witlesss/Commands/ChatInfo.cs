using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Witlesss.Commands
{
    public class ChatInfo : WitlessSyncCommand
    {
        protected override void Run()
        {
            var sb = new StringBuilder("<b>").Append(Title).Append("</b>\n");

            var size = SizeInBytes(Baka.Path);
            var icon = size switch
            {
                <      2_000 => "🗒",
                <    200_000 => "📖",
                <    800_000 => "📗",
                <  4_000_000 => "📙",
                < 16_000_000 => "📔",
                _            => "📚"
            };

            sb.Append("\nВес словаря: ").Append(FileSize(size)).Append(' ').Append(icon);
            sb.Append("\nИнтервал генерации: ").Append(Baka.Interval);
            sb.Append("\nКачество графики: ").Append(Baka.Meme.Quality).Append('%');
            if (!Context.ChatIsPrivate)
                sb.Append("\nМогут 🔩⚙️: ").Append(Baka.AdminsOnly ? "только админы 😎" : "все 🤠");

            sb.Append("\n\n<u>Авто-мемы:</u>");
            sb.Append("\nТип: ").Append(Types[Baka.Meme.Type]);
            sb.Append("\nВероятность: ").Append(Baka.Meme.Chance).Append('%');
            sb.Append("\nСтикеры: ").Append(Baka.Meme.Stickers ? "тоже 🍑" : "пропускаем");

            var anyOptions = false;
            var optionsBuilder = new StringBuilder("\n\n<u>Опции</u>:");
            if (IsNotNull(Baka.Meme.OptionsM)) AppendOptions("meme", Baka.Meme.OptionsM[5..]);
            if (IsNotNull(Baka.Meme.OptionsT)) AppendOptions("top",  Baka.Meme.OptionsT[4..]);
            if (IsNotNull(Baka.Meme.OptionsD)) AppendOptions("dp",   Baka.Meme.OptionsD[3..]);
            if (IsNotNull(Baka.Meme.OptionsG)) AppendOptions("dg",   Baka.Meme.OptionsG[3..]);
            if (IsNotNull(Baka.Meme.OptionsN)) AppendOptions("nuke", Baka.Meme.OptionsN[5..]);
            if (anyOptions) sb.Append(optionsBuilder);

            Bot.SendMessage(Chat, sb.ToString());

            bool IsNotNull([NotNullWhen(true)] string? s)
            {
                var ok = s is not null;
                anyOptions |= ok;
                return ok;
            }

            void AppendOptions(string cmd, string options)
            {
                optionsBuilder.Append("\n- /").Append(cmd).Append(": <code>").Append(options).Append("</code>");
            }
        }

        public static readonly Dictionary<MemeType, string> Types = new()
        {
            { MemeType.Meme, "мемы"             },
            { MemeType.Dg,   "демотиваторы💀"   },
            { MemeType.Top,  "подписанки 💬"    },
            { MemeType.Dp,   "демотиваторы👌"   },
            { MemeType.Nuke, "ядерные отходы🍤" }
        };
    }
}
