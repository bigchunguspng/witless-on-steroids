using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Crop : FileEditor_VideoPhoto // todo shake is only for video
{
    private const string _crop  = "piece_fap_bot-crop.mp4";
    private const string _shake = "piece_fap_bot-shake.mp4";

    private static readonly Regex
        _rgx_tlbr = new("[tlbr]",                   RegexOptions.Compiled | RegexOptions.IgnoreCase),
        _rgx_iw   = new("(?<=[^io_]|^)w",           RegexOptions.Compiled),
        _rgx_ih   = new("(?<=[^io_]|^)h(?=[^s]|$)", RegexOptions.Compiled);

    private bool _isShakeMode; 

    public Crop UseShakeMode()
    {
        _isShakeMode = true;
        return this;
    }

    public Crop UseDefaultMode()
    {
        _isShakeMode = false;
        return this;
    }

    protected override string SyntaxManual => _isShakeMode ? "/man_shake" : "/man_crop";

    protected override async Task Execute()
    {
        if (Args != null || _isShakeMode)
        {
            string[]? log = null;
            var args = Args?.Split(' ');

            if (_isShakeMode)
            {
                var crop   = args?.Length > 0 ? args[0] : "0.95";
                var speed  = args?.Length > 1 ? args[1] : "random(0)";
                var offset = args?.Length > 2 ? args[2] : "random(0)";
                args = F_Shake(crop, speed, offset).Split();
                log  = [crop, speed, offset];
            }
            else if (args?.Length == 2 && _rgx_tlbr.IsMatch(args[0]))
            {
                var match = _rgx_tlbr.Match(args[0].ToLower());
                var a0 = match.Value;
                var a1 = int.Parse(args[1]) / 100F;
                if      (a0 == "t") args = ["iw", $"(1-{a1})*ih", "0", $"ih*{a1}"];
                else if (a0 == "l") args = [$"(1-{a1})*iw", "ih", $"iw*{a1}", "0"];
                else if (a0 == "b") args = ["iw", $"(1-{a1})*ih", "0", "0"];
                else if (a0 == "r") args = [$"(1-{a1})*iw", "ih", "0", "0"];
            }
                
            for (var i = 0; i < Math.Min(args!.Length, 4); i++)
            {
                args[i] = _rgx_iw.Replace(args[i], "iw");
                args[i] = _rgx_ih.Replace(args[i], "ih");
            }

            if (args.Length > 4) args = args.Take(4).ToArray();

            var cropArgs = string.Join(':', args);

            var input = await GetFile();
            var output = input.GetOutputFilePath("crop", Ext);

            var options = FFMpeg.OutputOptions().VF($"crop={cropArgs}");
            var motionPicture = Type is MediaType.Video or MediaType.Anime;
            if (motionPicture) options.FixVideo_Playback();

            await FFMpeg.Command(input, output, options).FFMpeg_Run();

            SendResult(output);
            Log($"{Title} >> {CropOrShake} [{(_isShakeMode ? string.Join(':', log!) : cropArgs)}]");
        }
        else
            SendManual(CROP_MANUAL);
    }

    protected override string VideoFileName => _isShakeMode ? _shake : _crop;

    private string CropOrShake => _isShakeMode ? "SHAKE" : "CROP";

    private static string F_Shake(string a, string b, string c)
    {
        return $"w*({a}) h*({a}) (w-out_w)/2+((w-out_w)/2)*sin(t*({b})-{c}) (h-out_h)/2+((h-out_h)/2)*sin(t*({b})+2*{c})";
    }
}