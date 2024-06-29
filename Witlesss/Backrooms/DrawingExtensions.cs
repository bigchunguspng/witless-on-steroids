using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Witlesss.Backrooms;

public static class DrawingExtensions
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
}