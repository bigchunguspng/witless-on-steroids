using Telegram.Bot.Types;

namespace Witlesss.Commands.Editing;

public class CorruptImage : PhotoCommand
{
    //      /hex  N
    //      /hexg N
    // todo /hexg N1 N2 … NN

    private string _name      = null!;
    private byte[] _jpegBytes = null!;

    protected override async Task Execute()
    {
        var path = await DownloadFile();
        var jpeg = await path.UseFFMpeg(Origin).Out("-jpeg", ".jpg");

        _name = jpeg.RemoveExtension();
        _jpegBytes = await System.IO.File.ReadAllBytesAsync(jpeg);

        var g = Context.Command!.Contains('g');
        if (g) await HexVid();
        else   await HexPic();
    }

    private async Task HexVid()
    {
        var corruptionCount = Context.HasIntArgument(out var x) 
            ? Math.Max(x, 0) 
            : (_jpegBytes.Length / 1500F).CeilingInt();

        var id = GetHashCode();

        for (var i = 0; i < 30; i++)
        {
            var bytes = _jpegBytes.ToArray();
            var file = $"{_name}-{id}-{i:d3}.jpg";

            Corrupt(bytes, corruptionCount);
            await System.IO.File.WriteAllBytesAsync(file, bytes);
        }

        var inputPattern  = $"{_name}-{id}-*.jpg";
        var result        = $"{_name}-{id}.mp4";

        var args = $"-delay 5 \"{inputPattern}\" -loop 0 \"{result}\"";
        await SystemHelpers.StartProcess("magick", args).WaitForExitAsync();

        await using var stream = System.IO.File.OpenRead(result);
        Bot.SendAnimation(Origin, InputFile.FromStream(stream, "piece_fap_bot-hex.mp4"));
        Log($"{Title} >> HEX [#{corruptionCount}] VID");
    }

    private async Task HexPic()
    {
        var corruptionCount = Context.HasIntArgument(out var x) 
            ? Math.Max(x, 0) 
            : (_jpegBytes.Length / 1500F).CeilingInt();

        Corrupt(_jpegBytes, corruptionCount);

        var corruptedFile = $"{_name}-Hex.jpg";
        await System.IO.File.WriteAllBytesAsync(corruptedFile, _jpegBytes);

        var extension = Type is MediaType.Stick ? "webp" : "png";
        var result = $"{_name}-Hex-Fix.{extension}";
        var args = $"\"{corruptedFile}\" \"{result}\"";
        await SystemHelpers.StartReadableProcess("magick", args).WaitForExitAsync();

        SendResult(result);
        Log($"{Title} >> HEX [#{corruptionCount}]");
    }

    private void Corrupt(byte[] bytes, int corruptionCount)
    {
        var start = 0;
        var end = bytes.Length - 2;

        for (var i = 0; i < end; i++)
        {
            // find START OF SCAN marker FFDA
            if (bytes[i] == 0xFF && bytes[i + 1] == 0xDA)
            {
                start = i + 2 + bytes[i + 3];
                break;
            }
        }

        var glitches = new byte[corruptionCount];
        Random.Shared.NextBytes(glitches);

        for (var i = 0; i < corruptionCount; i++)
        {
            // don't put END OF IMAGE marker FFD9 in the IMAGE DATA segment
            if (glitches[i] is 0xFF or 0xD9) glitches[i] = 0x11;

            var offset = Random.Shared.Next(start, end);
            bytes[offset] = glitches[i];
        }
    }
}