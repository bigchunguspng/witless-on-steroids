using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Editing;

public class AdvancedEdit : AudioVideoPhotoCommand
{
    protected override string SyntaxManual => "/man_44";

    // /peg  [options]      [extension]
    // /pegv [videofilters] [extension]
    protected override async Task Execute()
    {
        if (Args is null)
        {
            SendManual();
        }
        else
        {
            var args = Args.Split(' ');

            var vf = OptionUsed('v');
            var af = OptionUsed('a');
            var fc = OptionUsed('c');

            var options = string.Join(' ', args.SkipLast(1));
            if (vf || af || fc)
            {
                var filter = fc ? "filter_complex" : $"{(vf ? 'v' : 'a')}f";
                options = $"-{filter} \"{options}\"";
            }

            var extension = args[^1];

            foreach (var c in extension)
            {
                if (Path.GetInvalidFileNameChars().Contains(c)) Bot.SendSticker(Chat, new InputOnlineFile(TROLLFACE));
            }

            var (path, _) = await Bot.Download(FileID, Chat);

            SendResult(await FFMpegXD.Edit(path, options, extension), extension, g: OptionUsed('g'));
            Log($"{Title} >> EDIT [{options}] [{extension}]");
        }
    }

    private bool OptionUsed(char option)
    {
        return Command!.Length > 4 && Command.AsSpan()[4..].Contains(option);
    }

    protected override bool MessageContainsFile(Message m)
    {
        if      (m.Photo     is not null)              FileID = m.Photo[^1].FileId;
        else if (m.Audio     is not null)              FileID = m.Audio    .FileId;
        else if (m.Video     is not null)              FileID = m.Video    .FileId;
        else if (m.Animation is not null)              FileID = m.Animation.FileId;
        else if (m.Sticker   is { IsAnimated: false }) FileID = m.Sticker  .FileId;
        else if (m.Voice     is not null)              FileID = m.Voice    .FileId;
        else if (m.VideoNote is not null)              FileID = m.VideoNote.FileId;
        else if (m.Document  is not null)              FileID = m.Document .FileId;
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

    private const string TROLLFACE = "CAACAgQAAx0CW-fiGwABBCUKZZ1tWkTgqp6spEH7zvPgyqZ3w0AAAt4BAAKrb-4HuRiqZWTyoLw0BA";
}
