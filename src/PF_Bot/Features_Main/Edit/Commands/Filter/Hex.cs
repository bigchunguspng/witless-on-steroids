using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Hex : FileEditor_Photo
{
    //      /hex  N
    //      /hexg N
    // todo /hexg N1 N2 … NN

    private string _name      = null!;
    private byte[] _jpegBytes = null!;

    protected override async Task Execute()
    {
        var g = Options.Contains('g');

        var input = await GetFile();
        var jpeg = input.GetOutputFilePath("JPEG", ".jpg");

        await JpegImage(input, output: jpeg, g);

        _name = jpeg.ToString().RemoveExtension();
        _jpegBytes = await System.IO.File.ReadAllBytesAsync(jpeg);

        if (g) await HexVid();
        else   await HexPic();
    }

    private async Task JpegImage(string input, string output, bool g)
    {
        var options = FFMpeg.OutputOptions();
        if (g)
        {
            var video = await FFProbe.GetVideoStream(input);
            options.MP4_EnsureValidSize(video);
        }

        await FFMpeg.Command(input, output, options).FFMpeg_Run();
    }

    private async Task HexVid()
    {
        var corruptionCount = Args.TryParseAsInt(out var x) 
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
        var processResult = await ProcessRunner.Run(MAGICK, args);
        if (processResult.Failure) throw new ProcessException(MAGICK, processResult);

        SendFile(result, MediaType.Anime, "piece_fap_bot-hex.mp4");
        Log($"{Title} >> HEX [#{corruptionCount}] VID");
    }

    private async Task HexPic()
    {
        var corruptionCount = Args.TryParseAsInt(out var x) 
            ? Math.Max(x, 0) 
            : (_jpegBytes.Length / 1500F).CeilingInt();

        Corrupt(_jpegBytes, corruptionCount);

        var corruptedFile = $"{_name}-Hex.jpg";
        await System.IO.File.WriteAllBytesAsync(corruptedFile, _jpegBytes);

        var extension = Type is MediaType.Stick ? "webp" : "png";
        var result = $"{_name}-Hex-Fix.{extension}";
        var args = $"\"{corruptedFile}\" \"{result}\"";
        var processResult = await ProcessRunner.Run(MAGICK, args);
        if (processResult.Failure) throw new ProcessException(MAGICK, processResult);

        SendResult(result);
        Log($"{Title} >> HEX [#{corruptionCount}]");
    }

    private void Corrupt(byte[] bytes, int corruptionCount)
    {
        var start = 0;
        var end = bytes.Length - 2;

        for (var i = 0; i < end; i++)
        {
            // find START_OF_SCAN marker (FFDA)
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
            var glitch = glitches[i];
            if (glitch is 0xFF or 0xD9)
                glitch =  0x11; // don't put END_OF_IMAGE marker (FFD9) in the IMAGE_DATA segment

            var offset = Random.Shared.Next(start, end);
            bytes[offset] = glitch;
        }
    }
}