namespace Witlesss;

public partial class Bot
{
    private readonly TelegramFileDownloader _downloader = new();

    /// <inheritdoc cref="TelegramFileDownloader.Download"/>
    public Task<(string path, MediaType type)> Download(string fileID, long chat)
    {
        return _downloader.Download(fileID, chat);
    }

    /// <inheritdoc cref="TelegramFileDownloader.DownloadFile"/>
    public Task DownloadFile(string fileID, string path, long chat = default)
    {
        return _downloader.DownloadFile(fileID, path, chat);
    }
}