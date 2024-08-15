using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme.Core;

namespace Witlesss.Memes.Shared; // ReSharper disable InconsistentNaming

public abstract class MemeGeneratorBase
{
    private   Size _sourceSizeOG;
    protected Size _sourceSizeAdjusted;

    protected void FetchImageSize(MemeFileRequest request)
    {
        _sourceSizeOG = Image.Identify(request.SourcePath).Size;
        _sourceSizeAdjusted = AdjustImageSize();
    }

    protected void FetchVideoSize(MemeFileRequest request)
    {
        _sourceSizeOG = FFMpegXD.GetPictureSize(request.SourcePath);
        _sourceSizeAdjusted = AdjustImageSize().ValidMp4Size();
    }

    private Size AdjustImageSize() => _sourceSizeOG.EnureIsWideEnough().FitSize(new Size(1280, 720));

    protected Image<Rgba32> GetImage(string path)
    {
        var image = Image.Load<Rgba32>(path);
        var resize = _sourceSizeOG != _sourceSizeAdjusted;
        if (resize)
            image.Mutate(x => x.Resize(_sourceSizeAdjusted));

        return image;
    }
}