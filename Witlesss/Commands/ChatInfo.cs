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

            var size = SizeInBytes(Baka.FilePath);
            var icon = size switch
            {
                <      2_000 => "🗒",
                <    200_000 => "📖",
                <    800_000 => "📗",
                <  4_000_000 => "📙",
                < 16_000_000 => "📔",
                _            => "📚"
            };

            // todo print Baka.Baka.DB.Vocabulary.Count too
            sb.Append("\nВес словаря: ").Append(FileSize(size)).Append(' ').Append(icon);
            sb.Append("\nИнтервал генерации: ").Append(Baka.Speech);
            sb.Append("\nКачество графики: ").Append(Baka.Quality).Append('%');
            if (!Context.ChatIsPrivate)
                sb.Append("\nМогут 🔩⚙️: ").Append(Baka.AdminsOnly ? "только админы 😎" : "все 🤠");

            sb.Append("\n\n<u>Авто-мемы:</u>");
            sb.Append("\nТип: ").Append(Types[Baka.Type]);
            sb.Append("\nВероятность: ").Append(Baka.Pics).Append('%');
            sb.Append("\nСтикеры: ").Append(Baka.Stickers ? "тоже 🍑" : "пропускаем");

            if (Baka.Options is not null)
            {
                var anyOptions = false;
                var optionsBuilder = new StringBuilder("\n\n<u>Опции</u>:");

                if (IsNotNull(Baka.Options.Meme)) AppendOptions("meme", Baka.Options.Meme[5..]);
                if (IsNotNull(Baka.Options.Top )) AppendOptions("top",  Baka.Options.Top [4..]);
                if (IsNotNull(Baka.Options.Dp  )) AppendOptions("dp",   Baka.Options.Dp  [3..]);
                if (IsNotNull(Baka.Options.Dg  )) AppendOptions("dg",   Baka.Options.Dg  [3..]);
                if (IsNotNull(Baka.Options.Nuke)) AppendOptions("nuke", Baka.Options.Nuke[5..]);

                if (anyOptions) sb.Append(optionsBuilder);

                // ==

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

            Bot.SendMessage(Chat, sb.ToString());
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
