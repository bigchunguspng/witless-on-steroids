using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Witlesss.Commands;

public class Help : SyncCommand
{
    // /help == /man
    // /help        -> main menu
    // /help A B    -> page exist ? navigated to page : ^
    // /help 0 3    -> ^
    // man - 0 3    -> ^ (edit)
    protected override void Run()
    {
        var txt =
            """
            <u><b>MAN...</b></u> 📖📝

            ✋ Техподдержка на связи 💯
            """;
        SendOrEditMessage(Chat, txt, -1, GetPaginationKeyboard());
    }

    protected static InlineKeyboardMarkup GetPaginationKeyboard()
    {
        var a1 = InlineKeyboardButton.WithCallbackData("Общее",   "1");
        var a2 = InlineKeyboardButton.WithCallbackData("Текст",   "2");
        var a3 = InlineKeyboardButton.WithCallbackData("Мемы",    "3");
        var a4 = InlineKeyboardButton.WithCallbackData("Монтаж",  "4");
        var a5 = InlineKeyboardButton.WithCallbackData("Reddit",  "5");
        var a6 = InlineKeyboardButton.WithCallbackData("YouTube", "6");

        var r1 = new List<InlineKeyboardButton> { a1, a4 };
        var r2 = new List<InlineKeyboardButton> { a2, a5 };
        var r3 = new List<InlineKeyboardButton> { a3, a6 };

        var buttons = new List<List<InlineKeyboardButton>> { r1, r2, r3 };
        return new InlineKeyboardMarkup(buttons);

        //string CallbackData(int p) => $"{key} - {p} {perPage}";
    }

    protected static void SendOrEditMessage(long chat, string text, int messageId, InlineKeyboardMarkup? buttons)
    {
        var b = messageId < 0;
        if (b) Bot.SendMessage(chat, text, buttons);
        else Bot.EditMessage(chat, messageId, text, buttons);
    }

    public void HandleCallback(CallbackQuery query, string[] data)
    {
        throw new System.NotImplementedException();
    }
}