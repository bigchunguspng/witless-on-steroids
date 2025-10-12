// ReSharper disable InconsistentNaming

using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Main.Edit.Helpers;
using PF_Bot.Routing.Commands;
using PF_Tools.ProcessRunning;

namespace PF_Bot.Features_Main.Edit.Commands.Manual;

public class UseMagick : FileEditor_Photo
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

        if (Context.ApplyAliases(ref options, Dir_Alias_Im).Failed())
        {
            Status = CommandResultStatus.BAD;
            return;
        }

        // GET EXTENSION
        var extension = args[^1];
        if      (extension == ".") extension = Ext.Substring(1);
        else if (extension == "p") extension = "png";
        else if (extension == "j") extension = "jpg";
        else if (extension == "w") extension = "webp";

        var extensionInvalid = extension.FileNameIsInvalid();
        if (extensionInvalid || ManualEditing.OptionsMentionsPrivateFile(options))
        {
            Status = CommandResultStatus.BAD;
            await ManualEditing.SendTrollface(Origin, extensionInvalid);
            return;
        }

        // EXECUTE

        var path = await GetFile();
        options = options.Replace("THIS", path);

        var output = await ProcessImage(path, options, extension);
        SendResult(output, extension, sendDocument: Options.Contains('g'));
        Log($"{Title} >> MAGICK [{options}] [{extension}]");
    }

    private async Task<string> ProcessImage(FilePath input, string options, string extension)
    {
        var output = input.GetOutputFilePath("Mgk", $".{extension}");
        var processResult = await ProcessRunner.Run(MAGICK, $"\"{input}\" {options} \"{output}\"");
        if (processResult.Failure) throw new ProcessException(MAGICK, processResult);

        return output;
    }

    private void SendResult(string result, string extension, bool sendDocument = false)
    {
        var type =     sendDocument ? MediaType.Other :
            extension     == "webp" ? MediaType.Stick :
            _pic.IsMatch(extension) ? MediaType.Photo :
            _gif.IsMatch(extension) ? MediaType.Anime : MediaType.Other;

        var name = type is MediaType.Photo or MediaType.Stick
            ? null
            : $"made with piece_fap_bot.{extension}";

        SendFile(result, type, name);
    }

    private static readonly Regex
        _pic = new("^(png|jpe?g)$",    RegexOptions.Compiled),
        _gif = new("^(gif|webm|mp4)$", RegexOptions.Compiled);
}