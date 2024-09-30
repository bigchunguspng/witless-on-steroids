using Telegram.Bot;
using Telegram.Bot.Types;

namespace Witlesss.Telegram;

public partial class Bot
{
    /// <summary>
    /// Downloads a file to the <b>Pics</b> directory if it's not there already.
    /// <br/>To be used with media files attached to commands.
    /// </summary>
    public async Task<string> Download(FileBase file, long chat, string extension)
    {
        var directory = Path.Combine(Dir_Pics, chat.ToString());
        Directory.CreateDirectory(directory);

        var hash = GetCapitalizationHash(file.FileUniqueId).ToString("X");
        var name = $"{file.FileUniqueId}#{hash}{extension}";
        var path = Path.Combine(directory, name);
        if (File.Exists(path))
        {
            while (FileIsLocked(path)) await Task.Delay(250);
        }
        else
        {
            await DownloadFile(file.FileId, path, chat);
        }

        return path;
    }

    /// <summary>
    /// Downloads a file or send a message if the exception is thrown.
    /// <br/>Make sure provided path is unique and directory is created!
    /// </summary>
    public async Task DownloadFile(string fileId, string path, long chat = default)
    {
        try
        {
            var file = await Client.GetFileAsync(fileId);
            var stream = new FileStream(path, FileMode.Create);
            Client.DownloadFileAsync(file.FilePath!, stream).Wait();
            await stream.DisposeAsync();
        }
        catch (Exception e)
        {
            var message = e.Message.Contains("file is too big")
                ? FILE_TOO_BIG.PickAny()
                : e.Message.XDDD();
            SendMessage(chat, message);
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

    /// <summary> Certified Windows™ moment. </summary>
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