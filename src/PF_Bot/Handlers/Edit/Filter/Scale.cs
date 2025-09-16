using PF_Bot.Handlers.Edit.Core;
using PF_Bot.Handlers.Edit.Shared;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;

namespace PF_Bot.Handlers.Edit.Filter
{
    public class Scale : VideoPhotoCommand
    {
        private readonly Regex _number = new(@"^\d+([\.,]\d+)?$");

        protected override string SyntaxManual => "/man_scale";

        protected override async Task Execute()
        {
            if (Args is null)
            {
                Bot.SendMessage(Origin, SCALE_MANUAL);
            }
            else
            {
                var args = Args.Split(' ').ToArray();

                MultiplyIfArgIsNumber(0, 'w');
                MultiplyIfArgIsNumber(1, 'h');

                void MultiplyIfArgIsNumber(int i, char side)
                {
                    if (args.Length > i && _number.IsMatch(args[i]))
                    {
                        var d = double.TryParse(args[i].Replace(',', '.'), out var value);
                        if (d && value < 5) args[i] = $"{value}*{side}";
                    }
                }

                for (var i = 0; i < Math.Min(args.Length, 2); i++)
                {
                    var w = Regex.Replace(args[i], "(?<=[^io_]|^)w",           "iw");
                    var h = Regex.Replace(w,       "(?<=[^io_]|^)h(?=[^s]|$)", "ih");
                    args[i] = h;
                }

                if (args.Length == 1) args = [args[0], "-1"];

                for (var i = 0; i < Math.Min(args.Length, 2); i++) // fixing oddness and large size
                {
                    var w = i == 0;
                    if (args[i] == "-1") args[i] = w ? "iw*oh/ih" : "ih*ow/iw";
                    args[i] = $"min({(w ? "1920" : "1080")},ceil(({args[i]})/2)*2)";
                }

                if (args.Length == 3  && args[2].StartsWith("flags=").Janai())
                    args[2] = "flags=" + args[2];

                var scaleArgs = string.Join(':', args);

                var input = await DownloadFile();
                var output = input.GetOutputFilePath("scale", Ext);

                var options = FFMpeg.OutputOptions().VF($"scale='{scaleArgs}'");
                var motionPicture = Type is MediaType.Video or MediaType.Anime;
                if (motionPicture) options.FixVideo_Playback();

                await FFMpeg.Command(input, output, options).FFMpeg_Run();

                SendResult(output);
                Log($"{Title} >> SCALE [{scaleArgs}]");
            }
        }

        protected override string VideoFileName { get; } = "piece_fap_bot-scale.mp4";
    }
}