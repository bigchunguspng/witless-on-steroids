using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Convert;

public class ToGIF : FileEditor_VideoPhoto
{
    protected override string SyntaxManual => "/man_g";

    protected override async Task Execute()
    {
        var input = await GetFile();

        var options = FFMpeg.OutputOptions().FixVideo_Playback();

        var photo = Type is MediaType.Photo or MediaType.Stick;
        var round = Type is MediaType.Round;
        if (round)
        {
            // videonote -> only crop
            options.Crop(FFMpegOptions.VIDEONOTE_CROP);
        }
        else
        {
            // photo / video -> ensure valid size
            var video = await FFProbe.GetVideoStream(input);
            options.MP4_EnsureValidSize(video, out var sizeIsValid);

            // video of valid size -> copy codec
            var copyCodec = sizeIsValid && photo == false && input.Extension != ".gif";
            _ = copyCodec
                ? options.Options(FFMpegOptions.Out_cv_copy)
                : options.SetCRF(30);
        }

        _ = photo
            ? options.Options("-t " + GetImageLoopDuration())
            : options.Options("-an");

        var args = photo
            ? FFMpeg.Args().Input(input, o => o.Options("-loop 1"))
            : FFMpeg.Args().Input(input);

        var suffix = photo ? "loop" : "GIF";
        var output = input.GetOutputFilePath(suffix, ".mp4");

        await args.Out(output, options).FFMpeg_Run();

        SendFile(output, MediaType.Anime, "piece_fap_bot-gif.mp4");
        Log($"{Title} >> GIF [~]");
    }

    private double GetImageLoopDuration() =>
        Args.TryParseAsDouble(out var value) ? Math.Clamp(value, 0.01, 120) : 5;
}