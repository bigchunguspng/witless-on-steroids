using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Core.Meme.Shared; // ReSharper disable InconsistentNaming

public abstract class MemeGeneratorBase
{
    private   Size _sourceSizeOG;
    protected Size _sourceSizeAdjusted;

    protected void FetchImageSize(MemeFileRequest request)
    {
        _sourceSizeOG = Image.Identify(request.SourcePath).Size;
        _sourceSizeAdjusted = AdjustImageSize(request);
    }

    protected void FetchVideoSize(MemeFileRequest request)
    {
        // todo task
        _sourceSizeOG = FFProbe.Analyze(request.SourcePath).Result.GetVideoStream().Size;
        _sourceSizeAdjusted = AdjustImageSize(request).ValidMp4Size();
    }

    private Size AdjustImageSize(MemeFileRequest request)
    {
        var size = request.ExportAsSticker
            ? _sourceSizeOG
            : _sourceSizeOG.EnureIsWideEnough();
        return size.FitSize(new Size(1280, 720));
    }

    protected Image<Rgba32> GetImage(string path)
    {
        var image = Image.Load<Rgba32>(path);
        var resize = _sourceSizeOG != _sourceSizeAdjusted;
        if (resize)
            image.Mutate(x => x.Resize(_sourceSizeAdjusted));

        return image;
    }
}