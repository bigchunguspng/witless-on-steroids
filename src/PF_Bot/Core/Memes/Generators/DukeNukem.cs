using PF_Bot.Core.Editing;
using PF_Bot.Core.Memes.Shared;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;

namespace PF_Bot.Core.Memes.Generators;

public struct MemeOptions_Nuke()
{
    public int Depth = 1;
}

public class DukeNukem(MemeOptions_Nuke op) : IMemeGenerator<int>
{
    public async Task GenerateMeme(MemeFileRequest request, FilePath output, int text)
    {
        //throw new InvalidOperationException($"Surprize mazafaka! {DateTime.Now.Ticks}");
        var probe = await FFProbe.Analyze(request.SourcePath);
        var result = await new FFMpeg_Nuke(probe, request, output)
            .Nuke(op.Depth)
            .FFMpeg_Run();

        LogNuke(result, request);
    }

    public async Task GenerateVideoMeme(MemeFileRequest request, FilePath output, int text)
    {
        var probe = await FFProbe.Analyze(request.SourcePath);
        var result = await new FFMpeg_Nuke(probe, request, output)
            .Nuke(op.Depth.Clamp(3))
            .FFMpeg_Run();

        LogNuke(result, request);
    }

    // LOGS

    public record struct NukeLogEntry(DateTime Time, MemeSourceType Type, string Command);

    public static readonly SyncDictionary<long, List<NukeLogEntry>> Logs = new();

    private static readonly Regex _rgx_nukeFilter = new(@"-filter_complex ""\[v:0\](.+?)"" ");

    // private readonly Regex _noAmplify = new("amplify=.+?,");
    // If you gonna implement presets (/anuke? /nuke info?),
    // remove this regex from preset filter when applying it to image (works only with videos) 

    private static void LogNuke(ProcessResult process, MemeFileRequest request)
    {
        var chat = request.Origin.Chat;
        var time = DateTime.UtcNow;

        if (Logs.ContainsKey(chat).Janai()) Logs.Add(chat, []);

        var command = _rgx_nukeFilter.ExtractGroup(1, process.Arguments, s => s, "[null]");
        Logs[chat].Add(new NukeLogEntry(time, request.Type, command));
    }
}