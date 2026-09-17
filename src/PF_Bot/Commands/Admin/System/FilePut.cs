using System.IO.Compression;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Commands.Admin.System;

public class FilePut : CommandHandlerAsync_Admin
{
    protected override async Task Run()
    {
        var file = TryGetFile(Message) ?? TryGetFile(Message.ReplyToMessage);
        if (Args == null || file == null)
        {
            SendManual(MANUAL);
            return;
        }

        var path = new FilePath(Args);

        var force = Options.Contains('!');
        var zip = file is Document doc
            && (doc.FileName?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ?? false)
            && path.Extension != ".zip";

        if (zip.Janai())
        {
            if (path.FileExists.Janai() || force)
            {
                path.EnsureParentDirectoryExist();
                await Bot.DownloadFile(file.FileId, path, Origin);
                Bot.SendSticker(Origin, STICKERS_DONE.PickAny());
                Log($"{Title} >> PUT FILE {path}", color: LogColor.Yellow);
            }
            else
                SendManual($"⚠ File already exist!\n\nTo overwrite:\n\n<code>/fput! {path}</code>");
        }
        else if (zip)
        {
            if (path.DirectoryExists.Janai() || force)
            {
                var temp = GetTempFileName("zip");
                await Bot.DownloadFile(file.FileId, temp, Origin);
                ZipFile.ExtractToDirectory(temp, path, overwriteFiles: true);
                Bot.SendSticker(Origin, STICKERS_DONE.PickAny());
                Log($"{Title} >> PUT DIR {path}", color: LogColor.Yellow);
            }
            else
                SendManual($"⚠ Directory already exist!\n\nTo update/overwrite:\n\n<code>/fput! {path}</code>");
        }
    }

    private static FileBase? TryGetFile
        (Message? m) => m == null
        ? null
        :  (FileBase?)m.Document
        ?? (FileBase?)m.Photo?[^1]
        ?? (FileBase?)m.Sticker
        ?? (FileBase?)m.Voice
        ?? (FileBase?)m.VideoNote
        ?? (FileBase?)m.Audio ?? m.Animation;

    private readonly string[] STICKERS_DONE =
    [
        "CAACAgIAAxkBAAEI5SNqq-Jy9nXnHCECu5NsMKWVEksvhgAC7SUAAqPMwUhMApDEH-0KKD0E",
        "CAACAgQAAxkBAAEI5Slqq-KTTnLEO3LxGEz6-kslXIJzdgACuAUAAtO9YAnoObLTylFwYD0E",
        "CAACAgIAAxkBAAEI5SVqq-KJaxhhknKSwKEQ9rzcZXGU5QAC8BcAAo4GSUg2nKEGsrBX1D0E",
        "CAACAgIAAxkBAAEI5S1qq-Ka1iRqeosYDZjj_kgBZmxk5gAC5xgAAn_pSUrwcbu7IVXevz0E",
    ];

    private const string MANUAL =
        """
        <code>/fput PATH 🗞</code>

        Saves given file to the specified <code>PATH</code> on the server.

        Zip files are unpacked if <code>PATH</code> doesn't end with <code>.zip</code>.
        """;
}