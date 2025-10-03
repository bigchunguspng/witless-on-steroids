using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Features_Aux.Listing;

public record ListPagination(MessageOrigin Origin, int MessageId = -1, int Page = 0, int PerPage = 25);

public static class Listing
{
    public static InlineKeyboardMarkup GetPaginationKeyboard(int page, int perPage, int last, string key)
    {
        var inactive = InlineKeyboardButton.WithCallbackData("💀", "-");
        var buttons = new List<InlineKeyboardButton> { inactive, inactive, inactive, inactive };

        if (page > 1       ) buttons[0] = InlineKeyboardButton.WithCallbackData("⏪", CallbackData(0));
        if (page > 0       ) buttons[1] = InlineKeyboardButton.WithCallbackData("⬅️", CallbackData(page - 1));
        if (page < last    ) buttons[2] = InlineKeyboardButton.WithCallbackData("➡️", CallbackData(page + 1));
        if (page < last - 1) buttons[3] = InlineKeyboardButton.WithCallbackData("⏩", CallbackData(last));

        return new InlineKeyboardMarkup(buttons);

        string CallbackData(int p) => $"{key} - {p} {perPage}";
    }
}