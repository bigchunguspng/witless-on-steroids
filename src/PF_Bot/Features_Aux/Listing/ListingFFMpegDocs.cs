using PF_Bot.Core;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingFFMpegDocs
{
    private static readonly Lazy<FFMpegDocumentation>        Docs_Lazy = new(new FFMpegDocumentation());
    public  static               FFMpegDocumentation Docs => Docs_Lazy.Value;

    private const string
        TEXT_MAIN =
            """
            📚 <u><b>Документация FFMpeg</b> теперь в Telegram</u> 😎

            📖 <a href="https://ffmpeg.org/ffmpeg-filters.html"><b>Сабж</b></a> 
            """,
        TEXT_SYNTAX =
            $"""
            ℹ️ <u><b>Синтаксис команды</b></u>

            <blockquote><b>Варианты использования</b>:
            <code>/pegman</code> - главная.
            <code>/pegman x</code> - синтаксис (вы здесь).
            <code>/pegman [a/v]</code> - открыть список аудио/видео фильтров.
            <code>/pegman [a/v] [N]</code> - открыть список на странице с фильтром номер <code>N</code>.
            <code>/pegman [текст/код]</code> - открыть справку по нужному фильтру.</blockquote>

            <blockquote><b>Параметры</b>:
            <code>N</code> - номер фильтра в соответствующем списке.
            <code>код</code> - категория + номер: <code>[a/v][N]</code> / <code>[N][a/v]</code>.
            <code>текст</code> - название фильтра: полностью / только начало / любая часть.</blockquote>

            <blockquote><b>Примеры</b>:
            <code>/pegman v131</code> - открыть справку <a href="{FFMpegDocumentation.URL}#huesaturation">этого фильтра</a> по коду.
            <code>/pegman 131v</code> - тоже самое.
            <code>/pegman huesat</code> - тоже самое, по началу имени.
            <code>/pegman huesaturation</code> - тоже самое, по имени.
            <code>/pegman v 131</code> - открыть список фильтров на странице с этим фильтром.</blockquote>
            """;

    public const int PER_PAGE = 20;

    private static readonly InlineKeyboardButton
        _butt_AF     = new("🎧 Audio Filters", CallbackData($"af - 0 {PER_PAGE}")),
        _butt_VF     = new("🎬 Video Filters", CallbackData($"vf - 0 {PER_PAGE}")),
        _butt_Syntax = new("ℹ️ Синтаксис",     CallbackData(   " - x")),
        _butt_Main   = new("Назад",     /*💨*/ CallbackData(   " - m"));
    //  _butt_page                        💨                "a/v - i(:p)"

    private static InlineKeyboardButton ButtAF
        (int page)  => new("🎧 Audio Filters", CallbackData($"af - {page} {PER_PAGE}"));
    private static InlineKeyboardButton ButtVF
        (int page)  => new("🎬 Video Filters", CallbackData($"vf - {page} {PER_PAGE}"));

    // MENU

    public static void SendMainMenu(MessageOrigin origin, int messageId = -1)
    {
        var button_rows = new List<List<InlineKeyboardButton>>();
        button_rows.Add([_butt_AF]);
        button_rows.Add([_butt_VF]);
        button_rows.Add([_butt_Syntax]);
        App.Bot.SendOrEditMessage(origin, TEXT_MAIN, messageId, new InlineKeyboardMarkup(button_rows));
    }

    public static void SendSyntax(MessageOrigin origin, int messageId = -1)
    {
        var button_rows = new List<List<InlineKeyboardButton>>();
        button_rows.Add([_butt_Main]);
        App.Bot.SendOrEditMessage(origin, TEXT_SYNTAX, messageId, new InlineKeyboardMarkup(button_rows));
    }

    // LIST

    public static void SendFilters(FilterKind kind, ListPagination pagination, int? targetNumber = null)
    {
        var        audio = kind == FilterKind.Audio;
        var key  = audio ? "a" : "v";
        var list = audio ? Docs.PagesAF : Docs.PagesVF;

        var pl = new PaginatedList<FFMpegDocsPage>(list, null, pagination)
        {
            Header = sb => sb.Append("⚙ <b>").Append(audio ? "Audio" : "Video").Append(" Filters</b>"),
            ItemText = (sb, item) => sb.Append($"<code>/pegman {item.Number,3}{key}</code> - {item.Title}"),
        };

        if (targetNumber.HasValue)
        {
            var i = targetNumber.Value - 1;
            pl.Pagination.Page = Math.Clamp(i / pl.PerPage, 0, pl.LastPage);
        }

        var button_rows = new List<List<InlineKeyboardButton>>();
        if (pl.Paginated) // add 1..4 buttons to open some filters from this page
        {
            var filterButtons = new List<InlineKeyboardButton>();
            var page_i0  = pl.Page * pl.PerPage; // first filter on page
            var page_len = pl.Page == pl.LastPage 
                ? list.Count - page_i0 
                : pl.PerPage;
            var count = Math.Min(4, page_len); // number of buttons to make
            var step = (double)page_len / count; // 20/4 -> 5  2/2 -> 1
            for (var i = 0; i < count; i++)
            {
                var page_i = page_i0 + (i * step).RoundInt();
                var text = $"{list[page_i].Number}{key}";
                var data = CallbackData($"{key} - {page_i}");
                filterButtons.Add(new InlineKeyboardButton(text, data));
            }
            button_rows.Add(filterButtons);
            button_rows.Add(pagination.GetPaginationButtons(pl.LastPage, $"{Registry.CallbackKey_FFMpeg}{key}f"));
        }
        button_rows.Add([_butt_Main]);

        pl.Keyboard = new InlineKeyboardMarkup(button_rows);
        pl.Send();
    }

    // PAGE

    public static void SendPage_OrSyntax
        (MessageOrigin origin, FilterKind kind, int number, int page = 0, int messageId = -1)
    {
        var        audio = kind == FilterKind.Audio;
        var list = audio ? Docs.PagesAF : Docs.PagesVF;
        var i = number - 1;
        if (i >= 0 && i < list.Count)
        {
            SendPage(origin, list, i, audio, page, messageId);
        }
        else
            SendSyntax(origin);
    }

    public static void SendPage_OrSyntax
        (MessageOrigin origin, string search)
    {
        var match
            =  Docs.PagesVF.FirstOrDefault(x => x.Title == search)
            ?? Docs.PagesAF.FirstOrDefault(x => x.Title == search)
            ?? Docs.PagesVF.FirstOrDefault(x => x.Title.StartsWith(search))
            ?? Docs.PagesAF.FirstOrDefault(x => x.Title.StartsWith(search))
            ?? Docs.PagesVF.FirstOrDefault(x => x.Title.Contains(search))
            ?? Docs.PagesAF.FirstOrDefault(x => x.Title.Contains(search));
        if (match != null)
        {
            var i = match.Number - 1;
            var audio = Docs.PagesVF[i].Title != match.Title; // i < VF.len
            var list = audio ? Docs.PagesAF : Docs.PagesVF;
            SendPage(origin, list, i, audio);
        }
        else
            SendSyntax(origin);
    }

    private static void SendPage
        (MessageOrigin origin, List<FFMpegDocsPage> list, int i, bool audio, int filter_page = 0, int messageId = -1)
    {
        var key   = audio ? "a" : "v";
        var emoji = audio ? "🎧" : "🎬";
        var page  = list[i];
        var multipage = page.Content.Length > 1;
        var text
            = $"{emoji} {page.Number} - "
            + $"<a href=\"{FFMpegDocumentation.URL}#{page.Anchor}\"><b>{page.Title}</b></a>"
            + (multipage ? $" 📃{filter_page + 1}/{page.Content.Length}" : "")
            + $"{page.Content[filter_page]}";

        var list_page = i / PER_PAGE;
        var list_butt = audio ? ButtAF(list_page) : ButtVF(list_page);

        var inactive = InlineKeyboardButton.WithCallbackData("💀", "-");
        var button_rows = new List<List<InlineKeyboardButton>>();
        if (multipage) // I'M NOT READING ALL O'THAT 😂😭🤣👌
        {
            button_rows.Add([inactive, inactive]);
            if (filter_page > 0)
            {
                var data = CallbackData($"{key} - {i}:{filter_page - 1}");
                button_rows[0][0] = new InlineKeyboardButton("⬅️ Вернуться", data);
            }
            if (filter_page < page.Content.Length - 1)
            {
                var data = CallbackData($"{key} - {i}:{filter_page + 1}");
                button_rows[0][1] = new InlineKeyboardButton("➡️ Читать дальше", data);
            }
        }
        button_rows.Add([inactive, inactive]);
        button_rows.Add([list_butt]);
        button_rows.Add([_butt_Syntax, _butt_Main]);
        if (i - 1 >= 0)
        {
            var title = $"⬅️ {list[i - 1].Title}";
            var data = CallbackData($"{key} - {i - 1}");
            button_rows[^3][0] = new InlineKeyboardButton(title, data);
        }
        if (i + 1 < list.Count)
        {
            var title = $"➡️ {list[i + 1].Title}";
            var data = CallbackData($"{key} - {i + 1}");
            button_rows[^3][1] = new InlineKeyboardButton(title, data);
        }
        App.Bot.SendOrEditMessage(origin, text, messageId, new InlineKeyboardMarkup(button_rows));
    }

    private static string CallbackData(string text) => $"{Registry.CallbackKey_FFMpeg}{text}";
}

public enum FilterKind
{
    Audio,
    Video
}