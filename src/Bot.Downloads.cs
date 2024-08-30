namespace Witlesss;

public partial class Bot
{
    private readonly TelegramFileDownloader _downloader = new();

    /// <inheritdoc cref="TelegramFileDownloader.Download"/>
    public Task<string> Download(string fileID, long chat, string extension)
    {
        return _downloader.Download(fileID, chat, extension);
    }

    /// <inheritdoc cref="TelegramFileDownloader.DownloadFile"/>
    public Task DownloadFile(string fileID, string path, long chat = default)
    {
        return _downloader.DownloadFile(fileID, path, chat);
    }
}