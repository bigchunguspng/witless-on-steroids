using Telegram.Bot.Types;

namespace Witlesss;

public partial class Bot
{
    private readonly TelegramFileDownloader _downloader = new();

    /// <inheritdoc cref="TelegramFileDownloader.Download"/>
    public Task<string> Download(FileBase file, long chat, string extension)
    {
        return _downloader.Download(file, chat, extension);
    }

    /// <inheritdoc cref="TelegramFileDownloader.DownloadFile"/>
    public Task DownloadFile(string fileID, string path, long chat = default)
    {
        return _downloader.DownloadFile(fileID, path, chat);
    }
}