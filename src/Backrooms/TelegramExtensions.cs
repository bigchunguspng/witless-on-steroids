using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Witlesss.Backrooms;

public static class TelegramExtensions
{
    private const string UNKNOWN = "[UNKNOWN]";

    public static string? GetTextOrCaption(this Message message)
    {
        return message.Caption ?? message.Text;
    }

    public static string GetChatTitle(this Message message)
    {
        var title = message.Chat.Title ?? message.From?.GetUserFullName() ?? UNKNOWN;
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

    // MEDIA

    public static PhotoSize? GetPhoto       (this Message message) => message.Photo?[^1];
    public static Sticker?   GetImageSticker(this Message message)
    {
        return message.Sticker is { IsVideo: false, IsAnimated: false } sticker ? sticker : null;
    }
    public static Animation? GetAnimation   (this Message message)
    {
        return message.Animation is { FileSize: <= 320_000, Duration: <= 21 } anime ? anime : null;
    }

    //

    public static async Task<bool> SenderIsAdmin(this Message message)
    {
        var chat = message.Chat.Id;
        if (message.SenderChat is not null)
        {
            if (message.SenderChat.Id == chat) return true;

            Bot.Instance.SendMessage(chat, Responses.UNKNOWN_CHAT.PickAny());
        }
        else if (await message.From.IsAdminInChat(chat) == false)
        {
            Bot.Instance.SendMessage(chat, Responses.NOT_ADMIN.PickAny());
        }
        else return true;

        return false;
    }

    public static async Task<bool> IsAdminInChat(this User? user, long chat)
    {
        if (user is null) return false;

        var admins = await Bot.Instance.Client.GetChatAdministratorsAsync(chat);
        return admins.Any(x => x.User.Id == user.Id);
    }

    public static string GetSenderName(this Message message)
    {
        return message.SenderChat?.Title ?? message.From?.GetUserFullName() ?? UNKNOWN;
    }

    public static string GetSongNameOr(this Message message, string text)
    {
        return message.GetSongName() ?? message.ReplyToMessage.GetSongName() ?? text;
    }

    private static string? GetSongName(this Message? message)
    {
        return message?.Audio?.FileName ?? message?.Document?.FileName;
    }

    public static bool ChatIsNotPrivate(this long chatId) => chatId < 0;
}