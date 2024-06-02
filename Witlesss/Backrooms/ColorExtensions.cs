using ColorHelper;
using SixLabors.ImageSharp.PixelFormats;

namespace Witlesss.Backrooms;

public static class ColorExtensions
{
    public static Rgb24 ToRgb24(this RGB color) => new(color.R, color.G, color.B);
}