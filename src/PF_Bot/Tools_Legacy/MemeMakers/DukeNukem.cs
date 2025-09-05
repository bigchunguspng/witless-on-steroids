using PF_Bot.Core.Editing;
using PF_Bot.Features.Generate.Memes.Core;
using PF_Bot.Tools_Legacy.MemeMakers.Shared;
using PF_Tools.Backrooms.Helpers.ProcessRunning;
using PF_Tools.FFMpeg;
using PF_Tools.FFMpeg.Tasks;

namespace PF_Bot.Tools_Legacy.MemeMakers;

public class DukeNukem : IMemeGenerator<int>
{
    public static int Depth = 1;

    // todo make it Task<string>
    public string GenerateMeme(MemeFileRequest request, int text)
    {
        var (input, output) = (request.SourcePath, request.TargetPath);

        var probe = FFProbe.Analyze(input).Result;
        var args = new FFMpeg_Nuke(input, probe)
            .Nuke(Depth)
            .Out(output, o => o.ApplyPostNuking(probe, request.GetQscale()));
        var result = args.FFMpeg_Run().Result;

        LogNuke(result, request);

        return output;
    }

    public async Task GenerateVideoMeme(MemeFileRequest request, int text)
    {
        var (input, output) = (request.SourcePath, request.TargetPath);

        var probe = await FFProbe.Analyze(input);
        var result = await new FFMpeg_Nuke(input, probe)
            .Nuke(Depth.Clamp(3), isVideo: true)
            .Out(output, o => o.ApplyPostNuking(probe, request.GetCRF(), isVideo: true))
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