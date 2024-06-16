using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Witlesss.Services.Technical // ReSharper disable MemberCanBePrivate.Global
{
    public static class ImageSaver
    {
        private static int _temp;

        public static int Quality { get; private set; }

        static ImageSaver()
        {
            Quality = 75;
        }

        public static void PassQuality(Witless witless) => PassQuality(witless.Meme.Quality);
        public static void PassQuality(int value)
        {
            Quality = value;
        }

        public static string SaveImage(Image image, string path)
        {
            path = UniquePath(path);
            image.SaveAsJpeg(path, GetJpegEncoder());
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

        public static JpegEncoder GetJpegEncoder() => new() { Quality = Quality };

        public static string GetTempPicName() => UniquePath(Paths.Dir_Temp, $"x_{_temp++}.png");
    }
}