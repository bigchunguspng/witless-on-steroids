using Telegram.Bot.Types;

// ReSharper disable InconsistentNaming

namespace Witlesss.Commands.Editing;

public class FFMpeg : AudioVideoPhotoCommand
{
    protected override string SyntaxManual => "/man_44";

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
        if (options.Contains('!'))
        {
            // APPLY ALIASES
            while (true)
            {
                var match = AliasRegex.Match(options);
                if (match.Success == false) break;
                
                if (!Context.ApplyAlias(match, ref options, Dir_Alias_Peg)) return;
            }
        }

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
        else if (extension == "p") extension = "png";
        else if (extension == "j") extension = "jpg";
        else if (extension == "w") extension = "webp";

        if (extension.FileNameIsInvalid() || options.Contains(File_Config, StringComparison.OrdinalIgnoreCase))
        {
            Bot.SendSticker(Origin, InputFile.FromFileId(TROLLFACE));
            return;
        }

        var path = await DownloadFile();

        var result = await path.UseFFMpeg(Origin).Edit(options).Out("-Edit", $".{extension}");
        SendResult(result, extension, sendDocument: OptionUsed('g'));
        Log($"{Title} >> FFMPEG [{options}] [{extension}]");
    }

    private bool OptionUsed(char option)
    {
        return Command!.Length > 4 && Command.AsSpan()[4..].Contains(option);
    }

    protected override bool MessageContainsFile(Message m)
    {
        if      (m.Photo     != null) (File, Ext) = (m.Photo[^1], ".jpg");
        else if (m.Audio     != null) (File, Ext) = (m.Audio    , m.Audio   .FileName.GetExtension(".mp3"));
        else if (m.Video     != null) (File, Ext) = (m.Video    , ".mp4");
        else if (m.Animation != null) (File, Ext) = (m.Animation, ".mp4");
        else if (m.HasImageSticker()) (File, Ext) = (m.Sticker! , ".webp");
        else if (m.HasVideoSticker()) (File, Ext) = (m.Sticker! , ".webm");
        else if (m.Voice     != null) (File, Ext) = (m.Voice    , ".ogg");
        else if (m.VideoNote != null) (File, Ext) = (m.VideoNote, ".mp4");
        else if (m.Document  != null) (File, Ext) = (m.Document , m.Document.FileName.GetExtension(".png"));
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