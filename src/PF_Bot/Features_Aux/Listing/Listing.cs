using System.Text;
using PF_Bot.Core;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Features_Aux.Listing;

public struct ListPagination(MessageOrigin origin, int messageId = -1, int page = 0, int perPage = 25, int extra = -1)
{
    public MessageOrigin Origin    = origin;
    public int           MessageId = messageId;
    public int           Page      = page;
    public int           PerPage   = perPage;
    public int           Extra     = extra;

    public int GetPositivePage
        (int lastPage) => Page < 0 ? Page = lastPage + 1 + Page : Page;

    public int GetLastPageIndex
        (int itemsTotal) => (int)Math.Ceiling(itemsTotal / (double)PerPage) - 1;
}

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
        var page = pagination.GetPositivePage(last);

        var inactive = InlineKeyboardButton.WithCallbackData("💀", "-");
        var buttons = new List<InlineKeyboardButton> { inactive, inactive, inactive, inactive };

        if (page > 1       ) buttons[0] = InlineKeyboardButton.WithCallbackData("⏪", CallbackData(0));
        if (page > 0       ) buttons[1] = InlineKeyboardButton.WithCallbackData("⬅️", CallbackData(page - 1));
        if (page < last    ) buttons[2] = InlineKeyboardButton.WithCallbackData("➡️", CallbackData(page + 1));
        if (page < last - 1) buttons[3] = InlineKeyboardButton.WithCallbackData("⏩", CallbackData(last));

        return buttons;

        string CallbackData(int p) => pagination.Extra == -1
            ? $"{key} - {p} {pagination.PerPage}"
            : $"{key} - {p} {pagination.PerPage} {pagination.Extra}";
    }
}

public class PaginatedList<T>(IReadOnlyCollection<T> list, string? callbackKey, ListPagination pagination)
{
    public  ListPagination Pagination = pagination;
    private MessageOrigin  Origin     => Pagination.Origin;
    private int            MessageId  => Pagination.MessageId;
    public  int            Page       => Pagination.GetPositivePage(LastPage);
    public  int            PerPage    => Pagination.PerPage;

    public bool Paginated => Pagination.PerPage < list.Count;
    public int   LastPage => Pagination.GetLastPageIndex(list.Count);

    public          Action<StringBuilder>?   Header   { get; init; }
    public required Action<StringBuilder, T> ItemText { get; init; }
    public          Action<StringBuilder>?   Footer   { get; init; }
    public          string HeadSeparator { get; init; } = "\n\n";
    public          string ItemSeparator { get; init; } = "\n";
    public          string Placeholder   { get; init; } = "*пусто*";
    public          bool   ShowArrowsTip { get; init; } = true;

    public InlineKeyboardMarkup? Keyboard { get; set; }

    public void Send()
    {
        var sb = new StringBuilder();
        Header?.Invoke(sb);
        if (Paginated && Header != null)
            sb.Append(' ');
        if (Paginated)
            sb.Append($"📃{Page + 1}/{LastPage + 1}");
        if (Paginated || Header != null)
            sb.Append(HeadSeparator);
        if (list.Count == 0)
            sb.Append(Placeholder);
        else
        {
            var i = 0;
            list.Skip(PerPage * Page)
                .Take(PerPage)
                .ForEach(item =>
                {
                    if (i > 0) sb.Append(ItemSeparator);
                    ItemText(sb, item);
                    i = 1;
                });
        }
        Footer?.Invoke(sb);
        if (Paginated && ShowArrowsTip) sb.Append(USE_ARROWS);

        Debug.Assert(Keyboard != null || callbackKey != null);

        if (Keyboard == null && Paginated)
            Keyboard = Pagination.GetPaginationKeyboard(LastPage, callbackKey!);

        App.Bot.SendOrEditMessage(Origin, sb.ToString(), MessageId, Keyboard);
    }
}