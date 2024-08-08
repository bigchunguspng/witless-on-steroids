﻿using System.Linq;
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

    // MEDIA GET

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
        (this Message message) => message.Document is { MimeType: "image/png" or "image/jpeg", Thumb: not null };

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

    public static (int width, int height) TryGetSize(this FileBase file)
    {
        if (file is PhotoSize             f1  ) return (f1.Width, f1.Height);
        if (file is Sticker               f2  ) return (f2.Width, f2.Height);
        if (file is Video                 f3  ) return (f3.Width, f3.Height);
        if (file is Animation             f4  ) return (f4.Width, f4.Height);
        if (file is Document { Thumb: { } f5 }) return (f5.Width, f5.Height);
        return (0, 0);
    }
}