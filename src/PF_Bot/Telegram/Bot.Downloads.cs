using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Telegram;

public partial class Bot
{
    /// Downloads a file to the <b>Pics</b> directory if it's not there already.
    /// <br/>To be used with media files attached to commands.
    public async Task<FilePath> Download
        (FileBase file, MessageOrigin origin, string extension)
    {
        var directory = Dir_Pics
            .Combine(origin.Chat.ToString())
            .EnsureDirectoryExist();

        var hash = GetCapitalizationHash(file.FileUniqueId).ToString("X");
        var name = $"{file.FileUniqueId}+{hash}{extension}";
        var path = directory.Combine(name);

        await DownloadFile(file.FileId, path, origin, overwriteFiles: false);

        return path;
    }

    /// Downloads a file or send a message if the exception is thrown.
    /// <br/> Make sure provided path is unique and directory is created!
    public async Task DownloadFile
        (string fileId, FilePath path, MessageOrigin origin, bool overwriteFiles = true)
    {
        var   semaphore = GetSemaphor(path);
        await semaphore.WaitAsync();

        try
        {
            if (path.FileExists)
            {
                await path.WaitForFile(checkEvery_ms: 125);
                if (overwriteFiles == false) return;
            }

            var file = await Client.GetFile(fileId);
            await using var stream = new FileStream(path, FileMode.Create);
            await Client.DownloadFile(file.FilePath!, stream);
        }
        catch (Exception e)
        {
            if (e.Message.Contains("file is too big"))
            {
                SendMessage(origin, FILE_TOO_BIG.PickAny());
                LogError("Telegram | FILE TOO BIG");
                throw new FileTooBigException();
            }
            else
            {
                SendMessage(origin, e.Message.XDDD());
                throw;
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    private readonly LimitedCache<string, SemaphoreSlim>
        _semaphores = new(32);

    [MethodImpl(MethodImplOptions.Synchronized)]
    private SemaphoreSlim GetSemaphor(string path)
    {
        if (_semaphores.Contains_No(path, out var slim))
        {
            slim = new SemaphoreSlim(1, 1);
            _semaphores.Add(path, slim);
        }

        return slim;
    }

    /// Certified Windows™ moment.
    private static int GetCapitalizationHash(string id)
    {
        var result = 0;
        var length = Math.Min(id.Length, 32);
        for (var i = 0; i < length; i++)
        {
            if (char.IsAsciiLetterUpper(id[i]))
            {
                result |= 1 << i;
            }
        }

        return result;
    }
}

public class FileTooBigException : Exception;