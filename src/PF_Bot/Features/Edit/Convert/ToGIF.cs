using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Edit.Core;
using PF_Bot.Features.Edit.Shared;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Edit.Convert;

public class ToGIF : VideoPhotoCommand
{
    protected override string SyntaxManual => "/man_g";

    protected override async Task Execute()
    {
        var input = await DownloadFile();

        var options = FFMpeg.OutputOptions().FixVideoPlayback();

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
            var video = await EditingHelpers.GetVideoStream(input);
            var size = video.Size.Ok();
            var sizeMp4 = size.ValidMp4Size();

            var sizeIsValid = size == sizeMp4;
            if (sizeIsValid == false)
                options.Resize(sizeMp4);

            // video of valid size -> copy codec
            var copyCodec = sizeIsValid && photo == false && input.GetExtension(".mp4") != ".gif";
            _ = copyCodec
                ? options
                    .Options(FFMpegOptions.Out_cv_copy)
                : options
                    .Options(FFMpegOptions.Out_cv_libx264)
                    .Options(FFMpegOptions.Out_crf_30);
        }

        _ = photo
            ? options.Options("-t " + GetImageLoopDuration())
            : options.Options("-an");

        var args = photo
            ? FFMpeg.Args().Input(input, o => o.Options("-loop 1"))
            : FFMpeg.Args().Input(input);

        var suffix = photo ? "loop" : "GIF";
        var output = EditingHelpers.GetOutputFilePath(input, suffix, ".mp4");
        args.Out(output, options);

        await EditingHelpers.FFMpeg_Run(args);
        await using var stream = System.IO.File.OpenRead(output);
        Bot.SendAnimation(Origin, InputFile.FromStream(stream, VideoFileName));
        Log($"{Title} >> GIF [~]");
    }

    private double GetImageLoopDuration() =>
        Context.HasDoubleArgument(out var value) ? Math.Clamp(value, 0.01, 120) : 5;

    private new const string VideoFileName = "piece_fap_bot-gif.mp4";
}