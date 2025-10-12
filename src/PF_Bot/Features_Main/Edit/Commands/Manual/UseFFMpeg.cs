using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Main.Edit.Helpers;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Main.Edit.Commands.Manual;

public class UseFFMpeg : FileEditor_AudioVideoPhoto
{
    protected override string SyntaxManual => $"/man_44\n{ALIAS_INFO}/apeg_info";

    // /peg  [options]      [extension]
    // /pegv [videofilters] [extension]
    // /pega [audiofilters] [extension]

    // /peg     [code:0:5]! [extension] <-- ALIAS USAGE
    // /peg […]$[code:0:5]! [extension] <-- ALIAS USAGE

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

        if (Context.ApplyAliases(ref options, Dir_Alias_Peg).Failed())
        {
            Status = CommandResultStatus.BAD;
            return;
        }

        // PROCESS OTHER OPTIONS
        var vf = Options.Contains('v');
        var af = Options.Contains('a');
        var fc = Options.Contains('c');

        if (vf || af || fc)
        {
            var filter = fc ? "filter_complex" : $"{(vf ? 'v' : 'a')}f";
            options = $"-{filter} \"{options}\"";
            if (vf || af) options = $"{options} -c:{(vf ? 'a' : 'v')} copy";
        }

        // GET EXTENSION
        var extension = args[^1];
        if      (extension == ".") extension = Ext == ".webm" ? "mp4" : Ext.Substring(1);
        else if (extension == "3") extension = "mp3";
        else if (extension == "4") extension = "mp4";
        else if (extension == "o") extension = "ogg";
        else if (extension == "p") extension = "png";
        else if (extension == "j") extension = "jpg";
        else if (extension == "w") extension = "webp";

        var extensionInvalid = extension.FileNameIsInvalid();
        if (extensionInvalid || ManualEditing.OptionsMentionsPrivateFile(options) || PixelThiefDetected(options))
        {
            Status = CommandResultStatus.BAD;
            await ManualEditing.SendTrollface(Origin, extensionInvalid);
            return;
        }

        // EXECUTE

        var input = await GetFile();
        var output = input.GetOutputFilePath("Edit", $".{extension}");

        options = options.Replace("THIS", input);

        await FFMpeg.Command(input, output, options).FFMpeg_Run();

        SendResult(output, extension, sendDocument: Options.Contains('g'));
        Log($"{Title} >> FFMPEG [{options}] [{extension}]");
    }

    private bool PixelThiefDetected(string options) =>
        Message.SenderIsBotAdmin().Janai()
     && (options.Contains("gdigrab")
      || options.Contains("x11grab"));

    protected override bool MessageContainsFile(Message m) => GetAnyFileID(m);

    private void SendResult(string result, string extension, bool sendDocument = false)
    {
        var type =     sendDocument ? MediaType.Other :
            extension     == "webp" ? MediaType.Stick :
            extension     ==  "mp3" ? MediaType.Audio :
            extension     ==  "mp4" ? MediaType.Anime :
            _pic.IsMatch(extension) ? MediaType.Photo : MediaType.Other;

        var name = type is MediaType.Photo or MediaType.Stick
            ? null
            : $"made with piece_fap_bot.{extension}";

        SendFile(result, type, name);
    }

    private static readonly Regex
        _pic = new("^(png|jpe?g)$", RegexOptions.Compiled);
}