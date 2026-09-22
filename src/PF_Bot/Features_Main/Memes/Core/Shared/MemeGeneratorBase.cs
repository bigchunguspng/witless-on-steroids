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
        var info = await WhenFileIsFree
        (
            request.SourcePath, nameof(FetchImageSize),
            () => Image.IdentifyAsync(request.SourcePath)
        );
        _sourceSizeOG = info.Size;
        _sourceSizeAdjusted = AdjustImageSize(request);
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
        var image = await WhenFileIsFree
        (
            path, nameof(GetImage),
            () => Image.LoadAsync<Rgba32>(path)
        );
        var resize = _sourceSizeOG != _sourceSizeAdjusted;
        if (resize)
            image.Mutate(x => x.Resize(_sourceSizeAdjusted));

        return image;
    }

    private static async Task<T> WhenFileIsFree<T>
        (FilePath path, string method, Func<Task<T>> task)
    {
        var i = 0;
        while (true)
        {
            try
            {
                return await task();
            }
            catch
            {
                LogError($"[WARNING] {method} -> CAN'T OPEN FILE \"{path}\", WAITING…");
                if (++i > 3) throw;

                await path.WaitForFile(checkEvery_ms: 125);
            }
        }
    }
}