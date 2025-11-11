using ColorHelper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Tools.Backrooms.Extensions;

/// Lightness, Chroma, Hue.
public readonly struct Oklch(double l, double c, int h)
{
    public readonly double
        L = Math.Clamp(l, 0.0, 1.0),
        C = Math.Clamp(c, 0.0, 1.0);
    public readonly int
        H = Math.Clamp(h, 0, 360);
}

public static class Extensions_Color
{
    public static Rgba32 ToRgba32(this Spectre.Console.Color color) => new (color.R, color.G, color.B);

    public static Rgb24 ToRgb24(this RGB color) => new(color.R, color.G, color.B);
    public static RGB   ToRGB(this Rgb24 color) => new(color.R, color.G, color.B);

    /// Sauce: https://gist.github.com/ronniebasak/e5331e54cf9414ab0fec23b4f6a27e2a
    public static Oklch ToOklch(this Rgb24 color)
    {
        // RGB 0-255 -> RGB 0-1F
        var R = Linearize(color.R / 255.0);
        var G = Linearize(color.G / 255.0);
        var B = Linearize(color.B / 255.0);

        double Linearize(double channel) =>
            channel <= 0.04045
                ? channel / 12.92
                : Math.Pow((channel + 0.055) / 1.055, 2.4);

        // RGB 0-1F -> XYZ -> Lab
        var x = (0.4124 * R + 0.3576 * G + 0.1805 * B) / 0.95047;
        var y = (0.2126 * R + 0.7152 * G + 0.0722 * B) / 1.0;
        var z = (0.0193 * R + 0.1192 * G + 0.9505 * B) / 1.08883;

        var fx = F(x);
        var fy = F(y);
        var fz = F(z);

        double F(double d) =>
            d > 0.008856
                ? Math.Pow(d, 1.0 / 3.0)
                : 7.787 * d + 16.0 / 116.0;

        var l = 116.0 * fy - 16.0;
        var a = 500.0 * (fx - fy);
        var b = 200.0 * (fy - fz);

        // Lab -> LCH
        var c = Math.Sqrt(a * a + b * b);
        var h = Math.Atan2(b, a) * (180.0 / Math.PI);
        if (h < 0)
            h += 360.0;

        return new Oklch(l / 100.0, c / 100.0, (int)h);
    }

    public static bool WhiteTextIsBetter(this Rgb24 background)
    {
        return background.ToOklch().L < 0.6F;
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