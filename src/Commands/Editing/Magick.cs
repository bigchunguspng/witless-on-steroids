// ReSharper disable InconsistentNaming

using Telegram.Bot.Types;

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
        if      (extension == ".") extension = Ext.Substring(1);
        else if (extension == "p") extension = "png";
        else if (extension == "j") extension = "jpg";
        else if (extension == "w") extension = "webp";

        var extensionInvalid = extension.FileNameIsInvalid();
        if (extensionInvalid || FFMpeg.OptionsMentionsPrivateFile(options))
        {
            await FFMpeg.SendTrollface(Origin, extensionInvalid);
            return;
        }

        var path = await DownloadFile();
        options = options.Replace("THIS", path);

        try
        {
            var result = await ProcessImage(path, options, extension);
            SendResult(result, extension, sendDocument: OptionUsed('g'));
        }
        catch
        {
            var errorMessage = Output is null ? "*пусто*" : string.Join('\n', Output!);
            Bot.SendErrorDetails(Origin, $"magick {MagickCommand}", errorMessage);
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

    private void SendResult(string result, string extension, bool sendDocument = false)
    {
        var name = "made with piece_fap_bot";

        using var stream = System.IO.File.OpenRead(result);
        if      (sendDocument)            Bot.SendDocument (Origin, InputFile_FromStream());
        else if (_pic.IsMatch(extension)) Bot.SendPhoto    (Origin, InputFile.FromStream(stream));
        else if (extension == "webp")     Bot.SendSticker  (Origin, InputFile.FromStream(stream));
        else if (_gif.IsMatch(extension)) Bot.SendAnimation(Origin, InputFile_FromStream());
        else                              Bot.SendDocument (Origin, InputFile_FromStream());

        InputFile InputFile_FromStream() => InputFile.FromStream(stream, name + "." + extension);
    }

    private static readonly Regex _pic = new("^(png|jpe?g)$"), _gif = new("^(gif|webm|mp4)$");
}