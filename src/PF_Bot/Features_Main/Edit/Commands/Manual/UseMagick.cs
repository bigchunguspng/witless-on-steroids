// ReSharper disable InconsistentNaming

using PF_Bot.Commands.Admin.System;
using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Main.Edit.Helpers;
using PF_Tools.ProcessRunning;

namespace PF_Bot.Features_Main.Edit.Commands.Manual;

public class UseMagick : FileEditor_VideoPhoto
{
    protected override string SyntaxManual => $"/man_43\n{ALIAS_INFO}/aim_info";
    protected override string IgnoreFileOption => "i";

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
            SetBadStatus();
            return;
        }

        var i = Options.Contains('i'); // no input

        // GET EXTENSION
        var extension = args[^1];
        if (i && extension == "-") extension = null;
        else if (extension == ".") extension = Ext.Substring(1);
        else if (extension == "p") extension = "png";
        else if (extension == "j") extension = "jpg";
        else if (extension == "w") extension = "webp";
        else if (extension == "4") extension = "mp4";
        else if (extension == "g") extension = "gif";

        var extensionInvalid = extension != null && extension.FileNameIsInvalid();
        if (extensionInvalid || ManualEditing.OptionsMentionsPrivateFile(options))
        {
            SetBadStatus();
            await ManualEditing.SendTrollface(Origin, extensionInvalid);
            return;
        }

        // EXECUTE

        options = options.Replace("CHAT", Chat.ToString());

        if (extension != null)
        {
            string output;
            if (i) // no input file
            {
                output = GetTempFileName(extension);
                await RunMagick($"{options} \"{output}\"");
            }
            else // some input file
            {
                var      input = await GetFile();
                output = input.GetOutputFilePath("Mgk", $".{extension}");

                options = options.Replace("THIS", input);

                await RunMagick($"\"{input}\" {options} \"{output}\"");
            }

            SendResult(output, extension, sendDocument: Options.Contains('g'));
        }
        else // i, extension == null
        {
            var result = await RunMagick(options);
            var output = result.Output.ToString();
            ProcessOutputSender.SendProcessOutput(Origin, output, null, result.ExitCode);
        }
        Log($"{Title} >> MAGICK [{options}] [{extension}]");
    }

    private static async Task<ProcessResult> RunMagick(string args)
    {
        var processResult = await ProcessRunner.Run(MAGICK, args);
        if (processResult.Failure) throw new ProcessException(MAGICK, processResult);
        return processResult;
    }

    private void SendResult(string result, string extension, bool sendDocument = false)
    {
        var type =     sendDocument ? MediaType.Other :
            extension     == "webp" ? MediaType.Stick :
            _pic.IsMatch(extension) ? MediaType.Photo :
            _gif.IsMatch(extension) ? MediaType.Anime : MediaType.Other;

        var name = type is MediaType.Photo or MediaType.Stick
            ? null
            : $"[{Desert.GetSand()}] made with piece_fap_bot.{extension}";

        DeleteAny_MessageToEdit();
        SendFile(result, type, name);
    }

    private static readonly Regex
        _pic = new("^(png|jpe?g)$",    RegexOptions.Compiled),
        _gif = new("^(gif|webm|mp4)$", RegexOptions.Compiled);
}