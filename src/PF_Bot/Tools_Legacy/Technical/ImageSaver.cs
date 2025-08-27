using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Tools_Legacy.Technical // ReSharper disable MemberCanBePrivate.Global
{
    public static class ImageSaver
    {
        private static int _temp;

        public static string SaveImage(Image image, string path, int quality)
        {
            path = UniquePath(path);
            image.SaveAsJpeg(path, GetJpegEncoder(quality));
            image.Dispose();

            return path;
        }

        public static string SaveImageWebp(Image<Rgba32> image, string path, int quality)
        {
            if (quality <= 25)
            {
                var sw = GetStartedStopwatch();
                using var memory = new MemoryStream();
                image.SaveAsJpeg(memory, GetJpegEncoder(quality));
                memory.Position = 0;
                var jpeg = Image.Load<Rgb24>(memory);
                image.ApplyQuality(jpeg);
                sw.Log("+compression");
            }

            path = UniquePath(path);
            image.SaveAsWebp(path, GetWebpEncoder(quality));
            image.Dispose();

            return path;
        }

        public static string SaveImageTemp(Image? image)
        {
            var path = GetTempPicName();
            image.SaveAsPng(path);
            image?.Dispose();

            return path;
        }

        public static JpegEncoder GetJpegEncoder(int quality) => new() { Quality = Math.Clamp(quality, 1, 100) };
        public static WebpEncoder GetWebpEncoder(int quality) => new() { Quality = Math.Clamp(quality, 1, 100) };

        public static string GetTempPicName() => UniquePath(Dir_Temp, $"x_{_temp++}.png");
    }
}