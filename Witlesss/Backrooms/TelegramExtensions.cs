using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Witlesss.Backrooms;

public static class TelegramExtensions
{
    public static string? GetTextOrCaption(this Message message)
    {
        return message.Caption ?? message.Text;
    }

    public static string GetChatTitle(this Message message)
    {
        var title = message.Chat.Title ?? message.From?.GetUserFullName() ?? "[UNKNOWN]";
        return title.Truncate(32);
    }

    public static string GetUserFullName(this User user)
    {
        var name = user.FirstName;
        var last = user.LastName;
        return last is null ? name : $"{name} {last}";
    }

    public static bool ContainsSpoilers(this Message message)
    {
        return message.CaptionEntities is { } c && c.Any(x => x.Type == MessageEntityType.Spoiler);
    }

    public static bool SenderIsAdmin(this Message message)
    {
        var chat = message.Chat.Id;
        if (message.SenderChat is not null)
        {
            if (message.SenderChat.Id == chat) return true;

            Bot.Instance.SendMessage(chat, Pick(UNKNOWN_CHAT_RESPONSE));
        }
        else if (message.From is not null && !Bot.Instance.UserIsAdmin(message.From, chat))
        {
            Bot.Instance.SendMessage(chat, Pick(NOT_ADMIN_RESPONSE));
        }
        else return true;

        return false;
    }
}