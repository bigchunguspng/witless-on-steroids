using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Witlesss.Backrooms;

public static partial class Extensions
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

    public static string GetFullNameTruncated(this User user)
    {
        return user.GetUserFullName().Truncate(32);
    }

    private static readonly Regex _chatMessageURL = new(@"https:\/\/t.me\/(?:c\/(\d+)|(\S+))\/(\d+)");

    public static (ChatId chat, int message) GetChatIdAndMessage(this string url)
    {
        var match = _chatMessageURL.Match(url);
        var chat = match.Groups[1].Success
            ? new ChatId(long.Parse($"-100{match.Groups[1].Value}"))
            : new ChatId(              $"@{match.Groups[2].Value}");
        var message = int.Parse(match.Groups[3].Value);

        return (chat, message);
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

    // MEDIA GET

    public static MessageOrigin GetOrigin
        (this Message message) => (message.Chat.Id, message.MessageThreadId);

    public static PhotoSize? GetPhoto
        (this Message message) => message.Photo?[^1];

    public static   Sticker? GetImageSticker
        (this Message message) => message.HasImageSticker() ? message.Sticker : null;

    public static   Sticker? GetVideoSticker
        (this Message message) => message.HasVideoSticker() ? message.Sticker : null;

    public static Animation? GetAnimation
        (this Message message) => message.Animation is { FileSize: <= 350_000, Duration: <= 30 } anime ? anime : null;

    public static MessageEntity? GetURL
        (this Message message) =>
        message.       Entities?.FirstOrDefault(x => x.Type == MessageEntityType.Url)
     ?? message.CaptionEntities?.FirstOrDefault(x => x.Type == MessageEntityType.Url);

    // MEDIA HAS?

    public static bool HasImageSticker
        (this Message message) => message.Sticker is { IsVideo: false, IsAnimated: false };

    public static bool HasVideoSticker
        (this Message message) => message.Sticker is { IsVideo: true };

    public static bool HasImageDocument
        (this Message message) => message.Document is { MimeType: "image/png" or "image/jpeg" };

    public static bool HasAnimeDocument
        (this Message message) => message.Document?.MimeType?.StartsWith("image") ?? false;

    public static bool HasVideoDocument
        (this Message message) => message.Document?.MimeType?.StartsWith("video") ?? false;

    public static bool HasAudioDocument
        (this Message message) => message.Document?.MimeType?.StartsWith("audio") ?? false;

    // MEDIA …

    public static bool ProvidesFile(this Message message, string type, out Document? document)
    {
        document = message.GetDocument(type) ?? message.ReplyToMessage.GetDocument(type);
        return document is not null;
    }

    public static Document? GetDocument(this Message? message, string type)
    {
        return message?.Document?.MimeType == type ? message.Document : null;
    }

    //

    public static bool SenderIsBotAdmin
        (this Message message) => message.From?.Id == Config.AdminID;

    public static async Task<bool> SenderIsAdmin(this Message message)
    {
        var chat = message.Chat.Id;
        if (message.SenderChat is not null)
        {
            if (message.SenderChat.Id == chat) return true;

            Bot.Instance.SendMessage(message.GetOrigin(), UNKNOWN_CHAT.PickAny());
        }
        else if (await message.From.IsAdminInChat(chat) == false)
        {
            Bot.Instance.SendMessage(message.GetOrigin(), NOT_ADMIN.PickAny());
        }
        else return true;

        return false;
    }

    public static async Task<bool> IsAdminInChat(this User? user, long chat)
    {
        if (user is null) return false;

        var admins = await Bot.Instance.Client.GetChatAdministrators(chat);
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

    public static (int width, int height) TryGetSize(this FileBase file)
    {
        if (file is PhotoSize                 f1  ) return (f1.Width, f1.Height);
        if (file is Sticker                   f2  ) return (f2.Width, f2.Height);
        if (file is Video                     f3  ) return (f3.Width, f3.Height);
        if (file is Animation                 f4  ) return (f4.Width, f4.Height);
        if (file is Document { Thumbnail: { } f5 }) return (f5.Width, f5.Height);
        return (0, 0);
    }

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