using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;
using static PF_Bot.Features_Main.Edit.Commands.Filter.Speed.Mode;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Speed : FileEditor_AudioVideo
{
    private double _speed, _value;
    private Mode _mode;

    public Speed SetMode(Mode mode)
    {
        _mode = mode;
        return this;
    }

    protected override string SyntaxManual => "/man_speed";

    protected override async Task Execute()
    {
        _value = Args.TryParseAsDouble(out var x) ? x : 2D;
        _speed = _mode == Fast ? _value : 1 / _value;
        _speed = Math.Clamp(_speed, 0.1, 94);
        _value = _mode == Fast ? _speed : 1 / _speed; // show clamped value in a filename

        var change_pitch   = Options.Contains('p');
        var use_rubberband = Options.Contains('r');

        var input = await GetFile();

        var (output, probe, options) = await input.InitEditing("Speed", Ext);

        if (probe.HasVideo && probe.GetVideoStream().IsLikelyImage.Janai())
        {
            var video = probe.GetVideoStream();
            var fps = Math.Min(video.AvgFramerate * _speed, 90D);

            options
                .VF($"setpts={1 / _speed}*PTS")
                .VF($"fps={fps}")
                .MP4_EnsureValidSize(video);
        }

        if (probe.HasAudio)
        {
            if (use_rubberband)
            {
                options.AF($"rubberband=tempo={_speed}");

                if (change_pitch)
                    options.AF($"rubberband=pitch={_speed}");

                // As 2 separate filters because of weird bug on prod:
                // "rubberband=tempo=0.95:pitch=0.95" ->
                // [libmp3lame @ 000000a24ceb9f40] inadequate AVFrame plane padding
                // Works fine with other values for the same file.
                // Works fine on dev machine.
            }
            else if (change_pitch)
            {
                var rate = probe.GetAudioStream().SampleRate;
                options.AF($"asetrate={_speed}*{rate}");
            }
            else
            {
                var speed = _speed;
                while (speed < 0.5) // speed = [0.1 - 94]
                {
                    options.AF("atempo=0.5"); // af.atempo: [0.5 - 94]
                    speed *= 2;
                }

                options.AF($"atempo={speed}");
            }
        }

        options.Fix_AudioVideo(probe);

        await FFMpeg.Command(input, output, options).FFMpeg_Run();

        SendResult(output);
        Log($"{Title} >> {ModeNameUpper} [{ModeIcon}]");
    }

    protected override string AudioFileName => SongNameOr($"[{Desert.GetSand()}] Are you {Sender.Before(' ')} or something.mp3");
    protected override string VideoFileName => $"piece_fap_bot-{ModeNameLower}-{_value}-{Desert.GetSand()}.mp4";

    private string ModeNameUpper => _mode == Fast ? "FAST" : "SLOW";
    private string ModeNameLower => _mode == Fast ? "fast" : "slow";
    private string ModeIcon      => _mode == Fast ? ">>"   :   "<<";

    public enum Mode
    {
        Fast, Slow,
    }
}