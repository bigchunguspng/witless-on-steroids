using Telegram.Bot.Types;

namespace Witlesss.Commands.Editing;

public class CorruptImage : PhotoCommand
{
    protected override async Task Execute()
    {
        var path = await DownloadFile();
        
        var jpeg = await path.UseFFMpeg(Origin).Out("-jpeg", ".jpg");
        var name = jpeg.RemoveExtension();

        var jpegBytes = await System.IO.File.ReadAllBytesAsync(jpeg);

        var match = Regex.Match(Context.Command!, @"g((\d{1,5})(-(\d{1,5}))?)?");
        var g = match.Success;
        if (g)
        {
            var v1 = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;
            var v2 = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : v1;

            var id = GetHashCode();

            for (var i = 0; i < 30; i++)
            {
                var bytes = jpegBytes.ToArray();
                var file = $"{name}-{id}-{i:d3}.jpg";

                var value = (v1 + (v2 - v1) * (i / 30F)).RoundInt();
                Corrupt(bytes, value);
                await System.IO.File.WriteAllBytesAsync(file, bytes);
            }

            var input  = $"{name}-{id}-*.jpg";
            var result = $"{name}-{id}.mp4";
            var args = $"-delay 5 \"{input}\" -loop 0 \"{result}\"";
            await SystemHelpers.StartProcess("magick", args).WaitForExitAsync();

            await using var stream = System.IO.File.OpenRead(result);
            Bot.SendAnimation(Origin, InputFile.FromStream(stream, "piece_fap_bot-hex.mp4"));
            Log($"{Title} >> HEX [#] VID");
        }
        else
        {
            var value = Context.HasIntArgument(out var x) ? Math.Clamp(x, 0, 1_000_000) : 1;
            var jpegCorrupted = name + "Hex.jpg";

            Corrupt(jpegBytes, value);
            await System.IO.File.WriteAllBytesAsync(jpegCorrupted, jpegBytes);

            var result = name + "-Hex-Fix.png";
            var exe = "magick";
            var args = $"\"{jpegCorrupted}\" \"{result}\"";
            await SystemHelpers.StartReadableProcess(exe, args).WaitForExitAsync();

            await using var stream = System.IO.File.OpenRead(result);
            Bot.SendPhoto(Origin, InputFile.FromStream(stream));
            Log($"{Title} >> HEX [#]");
        }
    }

    private void Corrupt(byte[] bytes, int corruptionLevel)
    {
        var start = 0;
        var length_m1 = bytes.Length - 1;

        for (var i = 0; i < length_m1; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0xDA) // start
            {
                start = i + 2;
                break;
            }
        }

        for (var i = start; i < length_m1; i++)
        {
            if (bytes[i] == 0xFF)
            {
                if (bytes[i + 1] == 0xD9) return; // end

                i++;
                continue; // don't touch FF bytes
            }

            if (LuckyFor(corruptionLevel, 1_000_000))
            {
                bytes[i] = Random.Shared.Next(255).ClampByte();
            }
        }
    }
}