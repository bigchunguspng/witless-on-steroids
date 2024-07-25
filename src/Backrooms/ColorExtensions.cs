using System;
using ColorHelper;
using SixLabors.ImageSharp.PixelFormats;

namespace Witlesss.Backrooms;

public static class ColorExtensions
{
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
}