using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Core.Memes.Shared; // ReSharper disable InconsistentNaming

public abstract class MemeGeneratorBase
{
    private   Size _sourceSizeOG;
    protected Size _sourceSizeAdjusted;

    protected async Task FetchImageSize(MemeFileRequest request)
    {
        var info = await Image.IdentifyAsync(request.SourcePath);
        _sourceSizeOG = info.Size;
        _sourceSizeAdjusted = AdjustImageSize(request);
    }

    protected async Task FetchVideoSize(MemeFileRequest request)
    {
        var probe = await FFProbe.Analyze(request.SourcePath);
        _sourceSizeOG = probe.GetVideoStream().Size;
        _sourceSizeAdjusted = AdjustImageSize(request).ValidMp4Size();
    }

    private Size AdjustImageSize(MemeFileRequest request)
    {
        var size = request.ExportAsSticker
            ? _sourceSizeOG
            : _sourceSizeOG.EnureIsWideEnough();
        return size.FitSize(new Size(1280, 720));
    }

    protected async Task<Image<Rgba32>> GetImage(string path)
    {
        var image = await Image.LoadAsync<Rgba32>(path);
        var resize = _sourceSizeOG != _sourceSizeAdjusted;
        if (resize)
            image.Mutate(x => x.Resize(_sourceSizeAdjusted));

        return image;
    }
}