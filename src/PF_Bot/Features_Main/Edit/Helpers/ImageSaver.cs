using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Features_Main.Edit.Helpers;

public static class ImageSaver
{
    public static async Task SaveImageJpeg(Image image, FilePath path, Quality quality)
    {
        await image.SaveAsJpegAsync(path, GetJpegEncoder(quality));
    }

    public static async Task SaveImageWebp(Image<Rgba32> image, FilePath path, Quality quality)
    {
        if (quality <= 25)
        {
            var sw = Stopwatch.StartNew();
            using var memory = new MemoryStream();
            await image.SaveAsJpegAsync(memory, GetJpegEncoder(quality));
            memory.Position = 0;
            var jpeg = Image.Load<Rgb24>(memory);
            image.ApplyQuality(jpeg);
            sw.Log("SaveImageWebp -> Apply JPEG compression");
        }

        await image.SaveAsWebpAsync(path, GetWebpEncoder(quality));
        image.Dispose();
    }

    public static async Task<string> SaveImageTemp(Image? image)
    {
        var path = GetTempFileName("png");
        await image.SaveAsPngAsync(path);

        return path;
    }

    private static JpegEncoder GetJpegEncoder(Quality quality) => new() { Quality = quality.GetImageQuality() };
    private static WebpEncoder GetWebpEncoder(Quality quality) => new() { Quality = quality.GetImageQuality() };
}