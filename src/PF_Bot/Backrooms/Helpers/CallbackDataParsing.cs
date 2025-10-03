using PF_Bot.Features_Aux.Listing;
using Telegram.Bot.Types;

namespace PF_Bot.Backrooms.Helpers;

// todo mode to callback context, router or wherewer it's used
public static class CallbackDataParsing
{
    public static MessageOrigin
        GetOrigin
        (this CallbackQuery query) => (query.GetChat(), query.GetThread());

    public static (string? key, string content)
        ParseData
        (this CallbackQuery query)
    {
        if (query.Data!.Contains(" - ").Janai())
            return (null, query.Data);

        var parts = query.Data.Split(" - ", 2);
        return (parts[0], parts[1]);

    }

    public static long
        GetChat
        (this CallbackQuery query) => query.Message!.Chat.Id;

    public static int?
        GetThread
        (this CallbackQuery query) => query.Message!.MessageThreadId;

    public static int
        GetMessageId
        (this CallbackQuery query) => query.Message!.Id;

    public static ListPagination
        GetPagination
        (this CallbackQuery query, string content)
    {
        var numbers = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        return new ListPagination(query.GetOrigin(), query.GetMessageId(), numbers[0], numbers[1]);
    }
}