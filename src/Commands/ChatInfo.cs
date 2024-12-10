using System.Diagnostics.CodeAnalysis;
using System.Text;
using Witlesss.Commands.Meme.Core;

namespace Witlesss.Commands
{
    public class ChatInfo : WitlessSyncCommand
    {
        protected override void Run()
        {
            var sb = new StringBuilder("<b>").Append(Title).Append("</b>\n");

            var size = PackPath.FileSizeInBytes();
            var icon = size switch
            {
                <      2_000 => "🗒",
                <    200_000 => "📖",
                <    800_000 => "📗",
                <  4_000_000 => "📙",
                < 16_000_000 => "📔",
                _            => "📚"
            };

            sb.Append("\nВес словаря: ").Append(size.ReadableFileSize()).Append(' ').Append(icon);
            if (ChatService.BakaIsLoaded(Chat))
                sb
                    .Append("\nСлов в запасе: ")
                    .Append(BrowseReddit.FormatSubs(Baka.WordCount, "💨")).Append(' ');
            sb.Append("\nВероятность ответа: ").Append(Data.Speech).Append('%');
            sb.Append("\nКачество графики: ").Append(Data.Quality).Append('%');
            if (!Context.ChatIsPrivate)
                sb.Append("\nМогут 🔩⚙️: ").Append(Data.AdminsOnly ? "только админы 😎" : "все 🤠");

            sb.Append("\n\n<u>Авто-мемы:</u>");
            sb.Append("\nТип: ").Append(Types[Data.Type]);
            sb.Append("\nВероятность: ").Append(Data.Pics).Append('%');
            sb.Append("\nСтикеры: ").Append(Data.Stickers ? "тоже 👌" : "пропускаем");

            if (Data.Options is not null)
            {
                var anyOptions = false;
                var optionsBuilder = new StringBuilder("\n\n<u>Авто-опции</u>:");

                if (IsNotNull(Data.Options.Meme)) AppendOptions("meme", Data.Options.Meme);
                if (IsNotNull(Data.Options.Top )) AppendOptions("top",  Data.Options.Top );
                if (IsNotNull(Data.Options.Dp  )) AppendOptions("dp",   Data.Options.Dp  );
                if (IsNotNull(Data.Options.Dg  )) AppendOptions("dg",   Data.Options.Dg  );
                if (IsNotNull(Data.Options.Nuke)) AppendOptions("nuke", Data.Options.Nuke);

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

            Bot.SendMessage(Origin, sb.ToString());
            Log($"{Title} >> CHAT INFO");
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
