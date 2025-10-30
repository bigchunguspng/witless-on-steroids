using ColorHelper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Color
{
    public static Rgba32 ToRgba32(this Spectre.Console.Color color) => new (color.R, color.G, color.B);

    public static Rgb24 ToRgb24(this RGB color) => new(color.R, color.G, color.B);
    public static RGB   ToRGB(this Rgb24 color) => new(color.R, color.G, color.B);

    public static bool WhiteTextIsBetter(this Rgb24 background)
    {
        var value =
            0.299F * background.R
          + 0.587F * background.G
          + 0.114F * background.B;

        var hls = ColorConverter.RgbToHsl(background.ToRGB());
        value *= 1 + (hls.S / 100F - 0.5F) * 0.3F;

        return value < 128;
    }

    public static Rgba32 CombineWith(this Rgba32 a, Rgba32 b, Func<byte, byte, byte> func)
    {
        return new Rgba32(func(a.R, b.R), func(a.G, b.G), func(a.B, b.B));
    }

    public static bool HasTransparentAreas(this Image<Rgba32> image, byte treshold = 255)
    {
        var sw = Stopwatch.StartNew();
        var size = image.Size;
        var side = Math.Min(size.Width, size.Height);
        var step = Math.Clamp(side / 50, 4, 64);
        foreach (var (x, y) in new SizeIterator_45deg(size, step))
        {
            if (image[x, y].A < treshold)
            {
                sw.Log($"HasTransparentAreas -> true | A={image[x, y].A} @{x}x{y}");
                return true;
            }
        }

        sw.Log("HasTransparentAreas -> false");
        return false;
    }
}