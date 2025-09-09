using PF_Bot.Core.Editing;
using PF_Bot.Core.Meme.Shared;
using PF_Tools.Backrooms.Helpers.ProcessRunning;
using PF_Tools.FFMpeg;

namespace PF_Bot.Core.Meme.Generators;

public class DukeNukem : IMemeGenerator<int>
{
    public static int Depth = 1;

    // todo make it Task<string>
    public string GenerateMeme(MemeFileRequest request, int text)
    {
        var probe = FFProbe.Analyze(request.SourcePath).Result;
        var result = new FFMpeg_Nuke(probe, request)
            .Nuke(Depth)
            .FFMpeg_Run().Result;

        LogNuke(result, request);

        return request.TargetPath;
    }

    public async Task GenerateVideoMeme(MemeFileRequest request, int text)
    {
        var probe = await FFProbe.Analyze(request.SourcePath);
        var result = await new FFMpeg_Nuke(probe, request)
            .Nuke(Depth.Clamp(3))
            .FFMpeg_Run();

        LogNuke(result, request);
    }

    // LOGS

    public static readonly object LogsLock = new();
    public static readonly Dictionary<long, List<NukeLogEntry>> Logs = new();

    private readonly Regex _nukeFilter = new(@"-filter_complex ""\[v:0\](.+?)"" ");

    // private readonly Regex _noAmplify = new("amplify=.+?,");
    // If you gonna implement presets (/anuke? /nuke info?),
    // remove this regex from preset filter when applying it to image (works only with videos) 

    private void LogNuke(ProcessResult process, MemeFileRequest request)
    {
        var chat = request.Origin.Chat;
        var time = DateTime.UtcNow;
        lock (LogsLock)
        {
            if (!Logs.ContainsKey(chat)) Logs.Add(chat, []);

            var command = _nukeFilter.ExtractGroup(1, process.Arguments, s => s, "[null]");
            Logs[chat].Add(new NukeLogEntry(time, request.Type, command));
        }
    }

    public record NukeLogEntry(DateTime Time, MemeSourceType Type, string Command);
}