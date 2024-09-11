using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Witlesss.Backrooms;

public static partial class Extensions
{
    public static T WithDefaultAlignment<T>(this T options) where T : TextOptions
    {
        options.TextAlignment = TextAlignment.Start;
        options.HorizontalAlignment = HorizontalAlignment.Left;
        options.VerticalAlignment = VerticalAlignment.Top;

        return options;
    }

    public static T WithoutWrapping<T>(this T options) where T : TextOptions
    {
        options.WrappingLength = -1;
        options.Origin = Vector2.Zero;

        return options;
    }

    public static IImageProcessingContext DrawImage(this IImageProcessingContext source, Image foreground)
    {
        return source.DrawImage(foreground, opacity: 1);
    }
    
    public static IImageProcessingContext DrawImage(this IImageProcessingContext source, Image foreground, Point point)
    {
        return source.DrawImage(foreground, point, opacity: 1);
    }

    public static void DrawFrame(this Image<Rgb24> image, Rectangle rectangle, int thickness, int margin, Rgb24 color)
    {
        var both = thickness + margin;
        var w = rectangle.Width  + 2 * both;
        var h = rectangle.Height + 2 * margin;
        var y0 = rectangle.Y - both;
        var x0 = rectangle.X - both;
        var xR = rectangle.Right  + margin;
        var yB = rectangle.Bottom + margin;
        var yM = rectangle.Y - margin;

        image.Mutate(x => x.Fill(color, new Rectangle(x0, y0, w, thickness)));
        image.Mutate(x => x.Fill(color, new Rectangle(x0, yB, w, thickness)));
        image.Mutate(x => x.Fill(color, new Rectangle(x0, yM, thickness, h)));
        image.Mutate(x => x.Fill(color, new Rectangle(xR, yM, thickness, h)));
    }

    public static void ApplyQuality(this Image<Rgba32> png, Image<Rgb24> jpg)
    {
        var width = png.Width;
        var height = png.Height;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var rgba32 = png[x, y];
            if (rgba32.A == 0) continue;

            var rgb24 = jpg[x, y];
            png[x, y] = new Rgba32(rgb24.R, rgb24.G, rgb24.B, rgba32.A);
        }
    }

    public static void ApplyPressure(this Image image, float press)
    {
        if (press == 0) return;

        var size = image.Size;
        image.Mutate(x => x.Resize((size * press).CeilingInt()).Resize(size));
    }
}