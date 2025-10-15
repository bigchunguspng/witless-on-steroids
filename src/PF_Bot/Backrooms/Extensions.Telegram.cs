using System.Diagnostics.CodeAnalysis;
using PF_Bot.Core;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Backrooms;

public static partial class Extensions
{
    private const string UNKNOWN = "[UNKNOWN]";

    public static async Task<User> GetMe_AtAllCost(this TelegramBotClient client)
    {
        while (true)
        {
            try
            {
                return await client.GetMe();
            }
            catch (Exception e)
            {
                LogError($"NO INTERNET? | {e.GetErrorMessage()}");
                Task.Delay(5000).Wait();
            }
        }
    }

    public static int? GetThread
        (this Message message)
        =>     message.IsAutomaticForward  ? message.Id              // channel post
            :  message.IsTopicMessage      ? message.MessageThreadId // forum thread message
            :  message.ReplyToMessage?.Id ?? message.MessageThreadId;

    public static string Format_ChatMessage
        (this Message message) => $"{message.Chat.Id}-{message.Id}";

    public static bool IsForwarded
        (this Message message) => message.ForwardFromChat != null;

    public static string? GetTextOrCaption
        (this Message message) => message.Caption ?? message.Text;

    public static string GetChatTitle(this Message message)
    {
        var chat = message.Chat;
        var title = chat.Title ?? chat.GetUserFullName();
        return title.Truncate(32);
    }

    public static string GetFullNameTruncated(this User user)
    {
        return user.GetUserFullName().Truncate(32);
    }

    private static readonly Regex
        _rgx_chatMessageURL = new(@"https:\/\/t.me\/(?:c\/(\d+)|(\S+))\/(\d+)", RegexOptions.Compiled);

    public static (ChatId chat, int message) GetChatIdAndMessage(this string url)
    {
        var match = _rgx_chatMessageURL.Match(url);
        var chat = match.Groups[1].Success
            ? new ChatId(long.Parse($"-100{match.Groups[1].Value}"))
            : new ChatId(              $"@{match.Groups[2].Value}");
        var message = int.Parse(match.Groups[3].Value);

        return (chat, message);
    }

    public static string GetUserFullName(this Chat chat)
    {
        var name = chat.FirstName!;
        var last = chat.LastName;
        return last is null ? name : $"{name} {last}";
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
        (this Message message) => (message.Chat.Id, message.GetThread());

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

    public static bool ProvidesFile(this Message message, string type, [MaybeNullWhen(false)] out Document document)
    {
        document = message.GetDocument(type) ?? message.ReplyToMessage.GetDocument(type);
        return document != null;
    }

    public static Document? GetDocument(this Message? message, string type)
    {
        return message?.Document?.MimeType == type ? message.Document : null;
    }

    //

    public static bool SenderIsBotAdmin
        (this Message message) => message.From != null && Config.AdminIDs.Contains(message.From.Id);

    public static async Task<bool> SenderIsChatAdmin(this Message message)
    {
        var chat = message.Chat.Id;
        if (message.SenderChat is not null)
        {
            if (message.SenderChat.Id == chat) return true;

            App.Bot.SendMessage(message.GetOrigin(), UNKNOWN_CHAT.PickAny());
        }
        else if (await message.From.IsAdminInChat(chat) == false)
        {
            App.Bot.SendMessage(message.GetOrigin(), NOT_A_CHAT_ADMIN.PickAny());
        }
        else return true;

        return false;
    }

    private static async Task<bool> IsAdminInChat(this User? user, long chat)
    {
        if (user is null) return false;

        var admins = await App.Bot.Client.GetChatAdministrators(chat);
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

    public static string GetAnimationNameOr(this Message message, string text)
    {
        return message.GetAnimationName() ?? message.ReplyToMessage.GetAnimationName() ?? text;
    }

    private static string? GetAnimationName(this Message? message)
    {
        return message?.Animation?.FileName ?? message?.Document?.FileName;
    }

    public static bool ChatIsPrivate(this long chatId) => chatId > 0;

    public static (int width, int height) TryGetSize(this FileBase file)
    {
        if (file is PhotoSize                 f1  ) return (f1.Width, f1.Height);
        if (file is Sticker                   f2  ) return (f2.Width, f2.Height);
        if (file is Video                     f3  ) return (f3.Width, f3.Height);
        if (file is Animation                 f4  ) return (f4.Width, f4.Height);
        if (file is Document { Thumbnail: { } f5 }) return (f5.Width, f5.Height);
        return (0, 0);
    }
}