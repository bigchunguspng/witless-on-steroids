using Telegram.Bot.Types.InputFiles;

// ReSharper disable InconsistentNaming

namespace Witlesss.Commands.Editing;

public class Magick : PhotoCommand
{
    protected override string SyntaxManual => "/man_43";

    private string? MagickCommand;
    private List<string>? Output;

    // /im [options] [extension]

    protected override async Task Execute()
    {
        if (Args is null)
        {
            SendManual();
            return;
        }

        var args = Args!.SplitN();

        // GET OPTIONS
        var options = string.Join(' ', args.SkipLast(1));
        if (options.Contains('!'))
        {
            // APPLY ALIASES
            var matches = AliasRegex.Matches(options);
            foreach (var match in matches.OfType<Match>())
            {
                if (!Context.ApplyAlias(match, ref options, Dir_Alias_Im)) return;
            }
        }

        // GET EXTENSION
        var extension = args[^1];
        if (extension == ".") extension = "png"; // todo get file type corresponding extension

        if (extension.FileNameIsInvalid() || options.Contains(File_Config, StringComparison.OrdinalIgnoreCase))
        {
            Bot.SendSticker(Chat, new InputOnlineFile(TROLLFACE));
            return;
        }

        var (path, _) = await Bot.Download(FileID, Chat);

        try
        {
            SendResult(await ProcessImage(path, options, extension), extension, g: OptionUsed('g'));
        }
        catch
        {
            SendErrorDetails(Chat);
        }

        Log($"{Title} >> MAGICK [{options}] [{extension}]");
    }

    private bool OptionUsed(char option)
    {
        return Command!.Length > 3 && Command.AsSpan()[3..].Contains(option);
    }

    private async Task<string> ProcessImage(string path, string options, string extension)
    {
        var directory = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var output = UniquePath(directory, name + $"-Mgk.{extension}");
        var exe = "magick";
        MagickCommand = $"\"{path}\" {options} \"{output}\"";
        Output = [];
        var process = SystemHelpers.StartReadableProcess(exe, MagickCommand);
        while (true)
        {
            var line = await process.StandardError.ReadLineAsync();
            if (line is null) break;

            Output.Add(line);
            LogError(line);
        }
        return output;
    }

    private void SendResult(string result, string extension, bool g = false)
    {
        var name = "made with piece_fap_bot";

        using var stream = File.OpenRead(result);
        if      (SendAsDocument())        Bot.SendDocument (Chat, New_InputOnlineFile());
        else if (_pic.IsMatch(extension)) Bot.SendPhoto    (Chat, new InputOnlineFile(stream));
        else if (extension == "webp")     Bot.SendSticker  (Chat, new InputOnlineFile(stream));
        else if (SendAsGIF())             Bot.SendAnimation(Chat, New_InputOnlineFile());
        else                              Bot.SendDocument (Chat, New_InputOnlineFile());

        bool SendAsGIF()      => _gif.IsMatch(extension) && g;
        bool SendAsDocument() => _pic.IsMatch(extension) && g;

        InputOnlineFile New_InputOnlineFile() => new(stream, name + "." + extension);
    }

    private void SendErrorDetails(long chat)
    {
        var path = UniquePath(Dir_Temp, "error.txt");
        var args = $"magick {MagickCommand}";
        var error = Output is null ? "*пусто*" : string.Join('\n', Output!);
        var text = string.Format(FF_ERROR_REPORT, args, GetRandomASCII(), error);
        File.WriteAllText(path, text);
        using var stream = File.OpenRead(path);
        Bot.SendDocument(chat, new InputOnlineFile(stream, "произошла ашыпка.txt"));
    }

    private static readonly Regex _pic = new(@"^(png|jpe?g)$");
    private static readonly Regex _gif = new(@"^(gif|webm|mp4)$");
}