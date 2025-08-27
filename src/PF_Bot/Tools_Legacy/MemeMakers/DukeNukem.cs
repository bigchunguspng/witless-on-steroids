using PF_Bot.Features.Generate.Memes.Core;
using PF_Bot.Tools_Legacy.FFMpeg;
using PF_Bot.Tools_Legacy.MemeMakers.Shared;

namespace PF_Bot.Tools_Legacy.MemeMakers;

public class DukeNukem : IMemeGenerator<int>
{
    public static int Depth = 1;

    public string GenerateMeme(MemeFileRequest request, int text)
    {
        var path = request.SourcePath;

        for (var i = 0; i < Depth; i++)
        {
            var process = request.UseFFMpeg();
            path = process
                .Nuke(request.GetQscale())
                .OutAs(UniquePath(request.TargetPath)).Result;
            LogNuke(process, request);
        }

        return path;
    }

    public async Task<string> GenerateVideoMeme(MemeFileRequest request, int text)
    {
        var size = FFMpegXD.GetPictureSize(request.SourcePath).GrowSize().ValidMp4Size();

        var path = request.SourcePath;

        for (var i = 0; i < Depth.Clamp(3); i++)
        {
            var process = request.UseFFMpeg();
            path = await process
                .NukeVideo(size.Ok(), request.GetCRF())
                .OutAs(UniquePath(request.TargetPath));
            LogNuke(process, request);
        }

        return path;
    }

    // LOGS

    public static readonly object LogsLock = new();
    public static readonly Dictionary<long, List<NukeLogEntry>> Logs = new();

    private readonly Regex _nukeFilter = new(@"-filter_complex ""\[v:0\](.+)"" """);

    // private readonly Regex _noAmplify = new("amplify=.+?,");
    // If you gonna implement presets (/anuke? /nuke info?),
    // remove this regex from preset filter when applying it to image (works only with videos) 

    private void LogNuke(F_Process process, MemeFileRequest request)
    {
        var chat = request.Origin.Chat;
        var time = DateTime.UtcNow;
        lock (LogsLock)
        {
            if (!Logs.ContainsKey(chat)) Logs.Add(chat, []);

            var command = _nukeFilter.ExtractGroup(1, process.ArgumentsText, s => s, "[null]");
            Logs[chat].Add(new NukeLogEntry(time, request.Type, command));
        }
    }

    public record NukeLogEntry(DateTime Time, MemeSourceType Type, string Command);
}