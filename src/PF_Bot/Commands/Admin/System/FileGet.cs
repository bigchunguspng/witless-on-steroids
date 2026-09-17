using System.IO.Compression;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Commands.Admin.System;

public class FileGet : CommandHandlerAsync_Admin
{
    protected override async Task Run()
    {
        if (Args == null)
        {
            SendManual(MANUAL);
            return;
        }

        var path = new FilePath(Args);
        if (path.FileExists || TryFindFile(ref path))
        {
            await using var stream = File.OpenRead(path);
            Bot.SendDocument(Origin, InputFile.FromStream(stream, path.FileName));
            Log($"{Title} >> GET FILE {path}", color: LogColor.Yellow);
        }
        else if (path.DirectoryExists)
        {
            var temp = GetTempFileName("zip");
            ZipFile.CreateFromDirectory(path, temp);

            await using var stream = File.OpenRead(temp);
            Bot.SendDocument(Origin, InputFile.FromStream(stream, path.FileName + ".zip"));
            Log($"{Title} >> GET DIR {path}", color: LogColor.Yellow);
        }
        else
        {
            SetBadStatus();
            Bot.SendMessage(Origin, $"{FAIL_EMOJI.PickAny()} File not found:\n\n<code>{path}</code>");
        }
    }

    private static bool TryFindFile(ref FilePath path)
    {
        var dir = path.DirectoryName;
        if (dir.IsNull_OrEmpty()) dir = ".";
        var files = Directory.GetFiles(dir, $"*{path.FileName}*");
        var success = files.Length > 0;
        if (success) path = new FilePath(files[0]);
        return success;
    }

    private const string MANUAL =
        """
        <code>/fget PATH</code>

        Sends a file at given <code>PATH</code> from the server to the chat.

        Directories are sent as zip archives.
        """;
}