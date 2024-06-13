using SixLabors.Fonts;

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
}