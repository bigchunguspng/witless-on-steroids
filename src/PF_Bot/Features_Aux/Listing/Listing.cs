using System.Text;
using PF_Bot.Core;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Features_Aux.Listing;

public record ListPagination(MessageOrigin Origin, int MessageId = -1, int Page = 0, int PerPage = 25);

public static class Listing
{
    public static InlineKeyboardMarkup GetPaginationKeyboard
        (this ListPagination pagination, int last, string key)
    {
        return new InlineKeyboardMarkup(pagination.GetPaginationButtons(last, key));
    }

    public static List<InlineKeyboardButton> GetPaginationButtons
        (this ListPagination pagination, int last, string key)
    {
        var (_, _, page, perPage) = pagination;

        if (page < 0) page = last;

        var inactive = InlineKeyboardButton.WithCallbackData("💀", "-");
        var buttons = new List<InlineKeyboardButton> { inactive, inactive, inactive, inactive };

        if (page > 1       ) buttons[0] = InlineKeyboardButton.WithCallbackData("⏪", CallbackData(0));
        if (page > 0       ) buttons[1] = InlineKeyboardButton.WithCallbackData("⬅️", CallbackData(page - 1));
        if (page < last    ) buttons[2] = InlineKeyboardButton.WithCallbackData("➡️", CallbackData(page + 1));
        if (page < last - 1) buttons[3] = InlineKeyboardButton.WithCallbackData("⏩", CallbackData(last));

        return buttons;

        string CallbackData(int p) => $"{key} - {p} {perPage}";
    }

    public static int GetLastPageIndex
        (this ListPagination pagination, int items_Count)
    {
        return (int)Math.Ceiling(items_Count / (double)pagination.PerPage) - 1;
    }

    public static void SendList<T>
    (
        IReadOnlyCollection<T> list,
        string callbackKey,
        ListPagination pagination,
        Action<StringBuilder> header,
        Action<StringBuilder, T> itemText,
        Action<StringBuilder>? footer = null,
        string separator = "\n",
        string placeholder = "*пусто*"
    )
    {
        var (origin, messageId, page, perPage) = pagination;

        var paginated = list.Count > perPage;
        var lastPage = pagination.GetLastPageIndex(list.Count);

        if (page < 0) page = lastPage;

        var sb = BuildPageContent
        (
            list, page, perPage, lastPage, paginated,
            header, itemText, footer, separator, placeholder
        );

        var buttons = paginated
            ? pagination.GetPaginationKeyboard(lastPage, callbackKey)
            : null;

        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    public static StringBuilder BuildPageContent<T>
    (
        IReadOnlyCollection<T> list,
        int page, int perPage, int lastPage, bool paginated,
        Action<StringBuilder>  header,
        Action<StringBuilder, T> itemText,
        Action<StringBuilder>? footer = null,
        string separator = "\n",
        string placeholder = "*пусто*"
    )
    {
        var sb = new StringBuilder();
        header .Invoke(sb);
        if (paginated) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n");
        if (list.Count == 0) sb.Append(placeholder);
        else
        {
            var i = 0;
            list.Skip(perPage * page)
                .Take(perPage)
                .ForEach(item =>
                {
                    if (i > 0) sb.Append(separator);
                    itemText(sb, item);
                    i = 1;
                });
        }
        footer?.Invoke(sb);
        if (paginated) sb.Append(USE_ARROWS);
        return sb;
    }
}