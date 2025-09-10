using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Edit.Core;
using PF_Bot.Features.Edit.Direct.Core;
using PF_Bot.Features.Edit.Shared;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

// ReSharper disable InconsistentNaming

namespace PF_Bot.Features.Edit.Direct;

public class UseFFMpeg : AudioVideoPhotoCommand
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

        if (Context.ApplyAliases(ref options, Dir_Alias_Peg) == false) return;

        // PROCESS OTHER OPTIONS
        var vf = OptionUsed('v');
        var af = OptionUsed('a');
        var fc = OptionUsed('c');

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
        if (extensionInvalid || DirectEditingHelpers.OptionsMentionsPrivateFile(options) || PixelThiefDetected(options))
        {
            await DirectEditingHelpers.SendTrollface(Origin, extensionInvalid);
            return;
        }

        // EXECUTE

        var input = await DownloadFile();
        var output = input.GetOutputFilePath("Edit", $".{extension}");

        options = options.Replace("THIS", input);

        await FFMpeg.Command(input, output, options).FFMpeg_Run();

        SendResult(output, extension, sendDocument: OptionUsed('g'));
        Log($"{Title} >> FFMPEG [{options}] [{extension}]");
    }

    private bool PixelThiefDetected(string options) =>
        !Message.SenderIsBotAdmin() && (options.Contains("gdigrab") || options.Contains("x11grab"));

    private bool OptionUsed(char option)
    {
        return Command!.Length > 4 && Command.IndexOf(option, 4) > 0;
    }

    protected override bool MessageContainsFile(Message m)
    {
        if      (m.Photo     != null) (File, Ext) = (m.Photo[^1], ".jpg");
        else if (m.Audio     != null) (File, Ext) = (m.Audio    , m.Audio   .FileName.GetExtension_Or(".mp3"));
        else if (m.Video     != null) (File, Ext) = (m.Video    , ".mp4");
        else if (m.Animation != null) (File, Ext) = (m.Animation, ".mp4");
        else if (m.HasImageSticker()) (File, Ext) = (m.Sticker! , ".webp");
        else if (m.HasVideoSticker()) (File, Ext) = (m.Sticker! , ".webm");
        else if (m.Voice     != null) (File, Ext) = (m.Voice    , ".ogg");
        else if (m.VideoNote != null) (File, Ext) = (m.VideoNote, ".mp4");
        else if (m.Document  != null) (File, Ext) = (m.Document , m.Document.FileName.GetExtension_Or(".png"));
        else return false;

        return true;
    }

    private void SendResult(string result, string extension, bool sendDocument = false)
    {
        using var stream = System.IO.File.OpenRead(result);
        if      (sendDocument)            Bot.SendDocument (Origin, InputFile_FromStream());
        else if (_pic.IsMatch(extension)) Bot.SendPhoto    (Origin, InputFile.FromStream(stream));
        else if (extension == "webp")     Bot.SendSticker  (Origin, InputFile.FromStream(stream));
        else if (extension == "mp3")      Bot.SendAudio    (Origin, InputFile_FromStream());
        else if (extension == "mp4")      Bot.SendAnimation(Origin, InputFile_FromStream());
        else                              Bot.SendDocument (Origin, InputFile_FromStream());

        InputFile InputFile_FromStream() => InputFile.FromStream(stream, $"made with piece_fap_bot.{extension}");
    }

    private static readonly Regex _pic = new("^(png|jpe?g)$");
}