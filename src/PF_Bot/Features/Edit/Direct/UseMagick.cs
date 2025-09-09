// ReSharper disable InconsistentNaming

using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Edit.Core;
using PF_Bot.Features.Edit.Shared;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Edit.Direct;

public class UseMagick : PhotoCommand
{
    protected override string SyntaxManual => $"/man_43\n{ALIAS_INFO}/aim_info";

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
        if (extensionInvalid || UseFFMpeg.OptionsMentionsPrivateFile(options))
        {
            await UseFFMpeg.SendTrollface(Origin, extensionInvalid);
            return;
        }

        // EXECUTE

        var path = await DownloadFile();
        options = options.Replace("THIS", path);

        var output = await ProcessImage(path, options, extension);
        SendResult(output, extension, sendDocument: OptionUsed('g'));
        Log($"{Title} >> MAGICK [{options}] [{extension}]");
    }

    private bool OptionUsed(char option)
    {
        return Command!.Length > 3 && Command.AsSpan()[3..].Contains(option);
    }

    private async Task<string> ProcessImage(FilePath path, string options, string extension)
    {
        var output = path.GetOutputFilePath("Mgk", extension);
        var processResult = await ProcessRunner.Run(MAGICK, $"\"{path}\" {options} \"{output}\"");
        if (processResult.Failure) throw new ProcessException(MAGICK, processResult);

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