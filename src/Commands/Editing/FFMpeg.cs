using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

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
        if (extension == ".") extension = "mp4"; // todo get file type corresponding extension

        if (extension.FileNameIsInvalid() || options.Contains(File_Config, StringComparison.OrdinalIgnoreCase))
        {
            Bot.SendSticker(Chat, new InputOnlineFile(TROLLFACE));
            return;
        }

        var path = await Bot.Download(FileID, Chat, Ext);

        var result = await path.UseFFMpeg().Edit(options).Out("-Edit", $".{extension}");
        SendResult(result, extension, g: OptionUsed('g'));
        Log($"{Title} >> FFMPEG [{options}] [{extension}]");
    }

    private bool OptionUsed(char option)
    {
        return Command!.Length > 4 && Command.AsSpan()[4..].Contains(option);
    }

    protected override bool MessageContainsFile(Message m)
    {
        if      (m.Photo     != null) (FileID, Ext) = (m.Photo[^1].FileId, ".jpg");
        else if (m.Audio     != null) (FileID, Ext) = (m.Audio    .FileId, m.Audio   .FileName.GetExtension(".mp3"));
        else if (m.Video     != null) (FileID, Ext) = (m.Video    .FileId, ".mp4");
        else if (m.Animation != null) (FileID, Ext) = (m.Animation.FileId, ".mp4");
        else if (m.HasImageSticker()) (FileID, Ext) = (m.Sticker! .FileId, ".webp");
        else if (m.HasVideoSticker()) (FileID, Ext) = (m.Sticker! .FileId, ".webm");
        else if (m.Voice     != null) (FileID, Ext) = (m.Voice    .FileId, ".ogg");
        else if (m.VideoNote != null) (FileID, Ext) = (m.VideoNote.FileId, ".mp4");
        else if (m.Document  != null) (FileID, Ext) = (m.Document .FileId, m.Document.FileName.GetExtension(".png"));
        else return false;

        return true;
    }

    private void SendResult(string result, string extension, bool g = false)
    {
        var name = "made with piece_fap_bot";

        using var stream = File.OpenRead(result);
        if      (SendAsDocument())        Bot.SendDocument (Chat, New_InputOnlineFile());
        else if (_pic.IsMatch(extension)) Bot.SendPhoto    (Chat, new InputOnlineFile(stream));
        else if (extension == "webp")     Bot.SendSticker  (Chat, new InputOnlineFile(stream));
        else if (extension == "mp3")      Bot.SendAudio    (Chat, New_InputOnlineFile());
        else if (SendAsGIF())             Bot.SendAnimation(Chat, New_InputOnlineFile());
        else if (extension == "mp4")      Bot.SendVideo    (Chat, New_InputOnlineFile());
        else                              Bot.SendDocument (Chat, New_InputOnlineFile());

        bool SendAsGIF()      => _gif.IsMatch(extension) && g;
        bool SendAsDocument() => _pic.IsMatch(extension) && g;

        InputOnlineFile New_InputOnlineFile() => new(stream, name + "." + extension);
    }

    private static readonly Regex _pic = new(@"^(png|jpe?g)$");
    private static readonly Regex _gif = new(@"^(gif|webm|mp4)$");
}