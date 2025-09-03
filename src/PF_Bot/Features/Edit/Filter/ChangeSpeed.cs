using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Edit.Core;
using PF_Bot.Features.Edit.Shared;
using PF_Tools.FFMpeg;
using static PF_Bot.Features.Edit.Filter.ChangeSpeed.Mode;

namespace PF_Bot.Features.Edit.Filter
{
    public class ChangeSpeed : AudioVideoCommand
    {
        private double _speed, _value;
        private Mode _mode;

        public ChangeSpeed SetMode(Mode mode)
        {
            _mode = mode;
            return this;
        }

        protected override async Task Execute()
        {
            _value = Context.HasDoubleArgument(out var x) ? x : 2D;
            _speed = _mode == Fast ? _value : 1 / _value;
            _speed = Math.Clamp(_speed, 0.1, 94);
            _value = _mode == Fast ? _speed : 1 / _speed; // show clamped value in a filename

            var input = await DownloadFile();
            var output = EditingHelpers.GetOutputFilePath(input, "Speed", Ext);

            var options = FFMpeg.OutputOptions();

            var probe = await FFProbe.Analyze(input);
            if (probe.HasVideo())
            {
                var video = probe.GetVideoStream();
                var fps = Math.Min(video.AvgFramerate * _speed, 90D);

                options
                    .VF($"setpts={1 / _speed}*PTS")
                    .VF($"fps={fps}")
                    .MP4_EnsureValidSize(video);
            }

            if (probe.HasAudio())
            {
                var speed = _speed;
                while (speed < 0.5) // speed = [0.1 - 94]
                {
                    options.AF("atempo=0.5"); // af.atempo: [0.5 - 94]
                    speed *= 2;
                }

                options.AF($"atempo={speed}");
            }

            options.Fix_AudioVideo(probe);

            await FFMpeg.Command(input, output, options).FFMpeg_Run();

            SendResult(output);
            Log($"{Title} >> {ModeNameUpper} [{ModeIcon}]");
        }

        protected override string AudioFileName => SongNameOr($"Are you {Sender.Split()[0]} or something.mp3");
        protected override string VideoFileName => $"piece_fap_bot-{ModeNameLower}-{_value.Format()}.mp4";

        private string ModeNameUpper => _mode == Fast ? "FAST" : "SLOW";
        private string ModeNameLower => _mode == Fast ? "fast" : "slow";
        private string ModeIcon      => _mode == Fast ? ">>"   :   "<<";

        public enum Mode
        {
            Fast, Slow,
        }
    }
}