using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing;

public class AdvancedEdit : FileEditingCommand
{
    // /ffxd [options]      [extension]
    // /ffv  [videofilters] [extension]
    public override void Run()
    {
        if (NothingToProcess()) return;

        if (Text.Contains(' '))
        {
            var args = Text.Split(' ');

            var cmd = RemoveBotMention(args[0]);
            var vf = cmd.Contains('v');
            var af = cmd.Contains('a');

            var options = string.Join(' ', args.Skip(1).SkipLast(1));
            if (vf || af) options = $"-{(vf ? 'v' : 'a')}f \"{options}\"";
            var extension = args[^1];

            foreach (var c in extension)
            {
                if (Path.GetInvalidFileNameChars().Contains(c)) Bot.SendSticker(Chat, new InputOnlineFile(TROLLFACE));
            }

            Bot.Download(FileID, Chat, out var path);

            SendResult(Memes.Edit(path, options, extension), extension, g: cmd.Contains('g'));
            Log($"{Title} >> EDIT [{options}] [{extension}]");
        }
        else
            SendManual();
    }

    protected override void SendManual()
    {
        Bot.SendSticker(Chat, new InputOnlineFile(Pick(DUDE)));
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

        bool SendAsGIF() => _gif.IsMatch(extension) || extension == "mp4" && g;
        bool SendAsDocument() => _pic.IsMatch(extension) && g;

        InputOnlineFile New_InputOnlineFile() => new(stream, name + "." + extension);
    }

    private static readonly Regex _pic = new(@"^(png|jpe?g)$");
    private static readonly Regex _gif = new(@"^(gif|webm)$");

    private const string TROLLFACE = "CAACAgQAAx0CW-fiGwABBCUKZZ1tWkTgqp6spEH7zvPgyqZ3w0AAAt4BAAKrb-4HuRiqZWTyoLw0BA";
    private readonly string[] DUDE = new[]
    {
        "CAACAgIAAxkBAAECZ1JlnXp-IZ7pQQ-65xXjPrf8xvLQnwACdDgAApAuAUsOAWZE2RxhujQE",
        "CAACAgIAAxkBAAECZ1ZlnXqSnVFOCQ-ZxiYnSDNOvjqSywAC1TUAAurH-EqrUrbGIyDdGDQE",
        "CAACAgIAAxkBAAECZ1plnXqxyEYT4UhvU9VSqpPLKURJzwACQjMAAnY7KEv2hp5tYChZ1TQE",
        "CAACAgIAAxkBAAECZ15lnXrCZtJt1AHSZoZqVC3dYdt8lwACgTYAAhXhKUvA8pcpkfVJDTQE",
        "CAACAgIAAxkBAAECZ2JlnXrva00d_k9_MeOhaK9kCx9zCgACmAwAAo3W-Enqi3-VH6it3TQE",
        "CAACAgIAAxkBAAECZ2hlnXsUKryvKUXxZAY4MOHloG9-gQACfRAAAjnXSUgzwFOJH8ykBjQE",
        "CAACAgIAAxkBAAECZ2plnXsj1OQUBQtMrMkr5Bv8tPH5bwACRw0AApUa-ElKPI21HNHo4jQE",
        "CAACAgIAAxkBAAECZ2xlnXtHd7bVPVqydpWCZwbq2e9dUAAC7QwAAtua-EmTOpxJHSiIcjQE",
        "CAACAgIAAxkBAAECZ3BlnXuokpG6KNRX4hVnbqNiQRtyqQACKScAAo8ZQEsx0p6zb98J0zQE",
        "CAACAgIAAxkBAAECZ3JlnXvncYpVXuvyAXe01aNWYOauJAACDiMAAsX-SUha8rwmJSbb0TQE"
    };
}
