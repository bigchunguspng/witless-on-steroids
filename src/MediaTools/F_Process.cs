using FFMpegCore;
using Witlesss.Backrooms.Types.SerialQueue;

namespace Witlesss.MediaTools;

// todo: animation -> -an,   video | animation -> -pix…420,   …

public partial class F_Process
{
    private static readonly SerialTaskQueue _queueHard = new(), _queueEasy = new();

    private readonly long Chat;
    public  readonly string Input;

    private readonly FFMpegArguments Arguments;
    private Action<FFMpegArgumentOptions>? Options;

    public F_Process(string input, long chat)
    {
        Input = input;
        Chat = chat;
        Arguments = FFMpegArguments.FromFileInput(Input);
    }

    private F_Process ApplyEffects(Action<FFMpegArgumentOptions> args)
    {
        Options = args;
        return this;
    }

    public Task<string> OutAs(string path)
        => Cook(path);

    public Task<string> Out
        (string suffix = "", string extension = ".mp4")
        => Cook(GetOutputName(suffix, extension is ".webm" ? ".mp4" : extension));

    public string GetOutputName
        (string suffix, string extension) => Input.RemoveExtension() + suffix + extension;

    private async Task<string> Cook(string output)
    {
        var hard = Path.GetExtension(output) is ".mp4" or ".gif";
        await Run(Arguments.OutputToFile(output, addArguments: Options), hard);
        return output;
    }

    private async Task Run(FFMpegArgumentProcessor processor, bool hard)
    {
        var args = processor.Arguments;
        try
        {
            var queue = hard ? _queueHard : _queueEasy;
            await queue.Enqueue(() =>
            {
#if DEBUG
                Log($"[FFMPEG] >> ffmpeg {args}", LogLevel.Debug, 3);
#endif
                return processor.ProcessAsynchronously();
            });
        }
        catch (Exception e)
        {
            Bot.Instance.SendErrorDetails(Chat, $"ffmpeg {args}", e.GetFixedMessage());
            throw;
        }
    }


    /// <summary> Gets media info + adds fixes to the options </summary>
    private MediaInfo MediaInfoWithFixing(FFMpegArgumentOptions o)
    {
        var info = GetMediaInfo();
        AddFixes(o, info);
        return info;
    }

    private MediaInfo GetMediaInfo()
    {
        var info = FFProbe.Analyse(Input);
        var video = info.PrimaryVideoStream;
        var audio = info.PrimaryAudioStream;
        var hasAudio = audio is not null;
        var hasVideo = video is { AvgFrameRate: not double.NaN };

        return new MediaInfo(info, audio, video, hasAudio, hasVideo);
    }

    private void AddFixes(FFMpegArgumentOptions o, MediaInfo i)
    {
        if (i.HasVideo) o.FixWebmSize(i.Video!);
        if (i.HasAudio) o.FixSongArt(i.Info);
    }

    public static VideoStream? GetVideoStream(string path) => FFProbe.Analyse(path).PrimaryVideoStream;

    private double GetFramerate() => Math.Max(GetMediaInfo().Video!.AvgFrameRate, 13); // magic moment
}

public record MediaInfo(IMediaAnalysis Info, AudioStream? Audio, VideoStream? Video, bool HasAudio, bool HasVideo);