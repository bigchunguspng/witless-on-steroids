using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Features_Main.Memes.Core.Shared; // ReSharper disable InconsistentNaming

public abstract class MemeGeneratorBase
{
    private   Size _sourceSizeOG;
    protected Size _sourceSizeAdjusted;

    protected async Task FetchImageSize(MemeRequest request)
    {
        for (var i = 3; i > 0; i--) // <-- retry mechanism
        {
            try
            {
                var info = await Image.IdentifyAsync(request.SourcePath);
                _sourceSizeOG = info.Size;
                _sourceSizeAdjusted = AdjustImageSize(request);
                i = 0;
            }
            catch
            {
                LogError($"[WARNING] FetchImageSize -> CAN'T OPEN FILE \"{request.SourcePath}\", WAITING…");
                await request.SourcePath.WaitForFile(checkEvery_ms: 125);
            }
        }
    }

    protected async Task FetchVideoSize(MemeRequest request)
    {
        var probe = await FFProbe.Analyze(request.SourcePath);
        _sourceSizeOG = probe.GetVideoStream().Size;
        _sourceSizeAdjusted = AdjustImageSize(request).ValidMp4Size();
    }

    private Size AdjustImageSize(MemeRequest request)
    {
        var size = request.ExportAsSticker
            ? _sourceSizeOG
            : _sourceSizeOG.EnureIsWideEnough(240);
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