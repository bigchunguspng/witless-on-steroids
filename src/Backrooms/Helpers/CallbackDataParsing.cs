using Telegram.Bot.Types;
using Witlesss.Commands.Packing;

namespace Witlesss.Backrooms.Helpers;

public static class CallbackDataParsing
{
    public static string[] GetData    (this CallbackQuery query) => query.Data!.Split(" - ", 2);
    public static long     GetChat    (this CallbackQuery query) => query.Message!.Chat.Id;
    public static int      GetMessage (this CallbackQuery query) => query.Message!.MessageId;

    public static ListPagination GetPagination(this CallbackQuery query, string[] data)
    {
        var numbers = data[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        return new ListPagination(query.GetChat(), query.GetMessage(), numbers[0], numbers[1]);
    }
}