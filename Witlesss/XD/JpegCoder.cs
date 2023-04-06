using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Witlesss.XD
{
    public static class JpegCoder
    {
        private static int _temp;
        private static readonly ImageCodecInfo JpgEncoder;
        private static readonly EncoderParameters EncoderParameters;

        public static long Quality { get; private set; } = 120;

        static JpegCoder()
        {
            JpgEncoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
            EncoderParameters = new EncoderParameters(count: 1);
        }

        public  static void PassQuality(Witless witless) => PassQuality(witless.Meme.Quality);
        private static void PassQuality(int value)
        {
            if (Quality == value) return;

            Quality = value;
            EncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, value);
        }

        public static string SaveImage(Image image, string path)
        {
            path = UniquePath(path);
            image.Save(path, JpgEncoder, EncoderParameters);
            image.Dispose();

            return path;
        }

        public static string SaveImageTemp(Image image)
        {
            var path = UniquePath($@"{TEMP_FOLDER}\x_{_temp++}.png");
            image.Save(path);
            image.Dispose();

            return path;
        }
    }
}