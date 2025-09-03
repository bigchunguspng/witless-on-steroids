using PF_Bot.Features.Edit.Shared;
using PF_Bot.Telegram;
using PF_Tools.FFMpeg;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Media.MediaDB;

public class GIF_DB : MediaDB<Animation>
{
    public static readonly GIF_DB Instance = new GIF_DB().LoadDB<GIF_DB>();

    protected override string Name { get; } = "GIF_DB";
    protected override string What { get; } = "GIFS";
    protected override string WhatSingle { get; } = "GIF";
    protected override string DB_Path { get; } = File_GIFs;

    protected override async Task<Animation> UploadFile(string path, long channel)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var temp = Path.Combine(Dir_Temp, $"{name}.mp4");

        await FFMpeg_VideoToGIF(path, temp);

        await using var stream = File.OpenRead(temp);
        var message = await Bot.Instance.Client.SendAnimation(channel, stream);
        return message.Animation!;
    }

    // todo partitial duplicate from ToGIF! - split image/video ToGIF
    private async Task FFMpeg_VideoToGIF(string input, string output)
    {
        var options = FFMpeg.OutputOptions().FixVideo_Playback();

        var video = await FFProbe.GetVideoStream(input);
        options.MP4_EnsureValidSize(video, out var sizeIsValid);

        // video of valid size -> copy codec
        var copyCodec = sizeIsValid && input.GetExtension(".mp4") != ".gif";
        _ = copyCodec
            ? options
                .Options(FFMpegOptions.Out_cv_copy)
            : options
                .Options(FFMpegOptions.Out_cv_libx264)
                .Options(FFMpegOptions.Out_crf_30);

        await FFMpeg.Command(input, output, options.Options("-an")).FFMpeg_Run();
    }
}