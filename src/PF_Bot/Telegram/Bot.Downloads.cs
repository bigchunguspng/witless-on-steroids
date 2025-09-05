using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Telegram;

public partial class Bot
{
    /// Downloads a file to the <b>Pics</b> directory if it's not there already.
    /// <br/>To be used with media files attached to commands.
    public async Task<FilePath> Download(FileBase file, MessageOrigin origin, string extension)
    {
        var directory = Dir_Pics
            .Combine(origin.Chat.ToString())
            .EnsureDirectoryExist();

        var hash = GetCapitalizationHash(file.FileUniqueId).ToString("X");
        var name = $"{file.FileUniqueId}+{hash}{extension}";
        var path = directory.Combine(name);
        if (path.FileExists)
        {
            while (FileIsLocked(path)) await Task.Delay(250);
        }
        else
        {
            await DownloadFile(file.FileId, path, origin);
        }

        return path;
    }

    /// Downloads a file or send a message if the exception is thrown.
    /// <br/>Make sure provided path is unique and directory is created!
    public async Task DownloadFile(string fileId, string path, MessageOrigin origin)
    {
        try
        {
            var file = await Client.GetFile(fileId);
            var stream = new FileStream(path, FileMode.Create);
            Client.DownloadFile(file.FilePath!, stream).Wait();
            await stream.DisposeAsync();
        }
        catch (Exception e)
        {
            var message = e.Message.Contains("file is too big")
                ? FILE_TOO_BIG.PickAny()
                : e.Message.XDDD();
            SendMessage(origin, message);
            throw;
        }
    }

    private static bool FileIsLocked(string path)
    {
        try
        {
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read);
            fs.Close();
        }
        catch (IOException)
        {
            return true;
        }

        return false;
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