using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Witlesss.Services.Technical // ReSharper disable MemberCanBePrivate.Global
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

        public static string SaveImageTemp(Image image)
        {
            var path = GetTempPicName();
            image.SaveAsPng(path);
            image.Dispose();

            return path;
        }

        public static JpegEncoder GetJpegEncoder(int quality) => new() { Quality = quality };

        public static string GetTempPicName() => UniquePath(Paths.Dir_Temp, $"x_{_temp++}.png");
    }
}