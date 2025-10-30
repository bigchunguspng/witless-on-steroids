using System.Collections;
using SixLabors.ImageSharp;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Size
{
    public static Size GrowSize(this Size size, int minSemiperimeter = 400)
    {
        if (size.Width + size.Height >= minSemiperimeter) return size;

        var ratio = size.AspectRatio();
        var wide = size.Width > size.Height;
        var limit = (int)(minSemiperimeter * ratio / (ratio + 1)); // i have no idea what it does
        return size.Normalize(limit, reduce: wide);
    }

    /// Makes sure image can go through a square hole with a side = <b>max</b>.
    public static Size FitSize(this Size size, int max)
    {
        var tooBig = size.Width > max || size.Height > max;
        return tooBig ? size.Normalize(max) : size;
    }

    public static Size FitSize(this Size size, Size max)
    {
        if (size.Width <= max.Width && size.Height <= max.Height) return size;

        var ratio = size.AspectRatio();
        return ratio > max.AspectRatio()
            ? new Size(max.Width, (int)(max.Width / ratio))
            : new Size((int)(max.Height * ratio), max.Height);
    }

    public static Size AdjustBackgroundSize(this Size source, Size target)
    {
        var ratioSource = source.AspectRatio();
        var ratioTarget = target.AspectRatio();
        return ratioSource > ratioTarget
            ? new Size((int)(target.Height * ratioSource), target.Height)
            : new Size(target.Width, (int)(target.Width * ratioSource));
    }

    public static Size Normalize(this Size size, int limit, bool reduce = true)
    {
        double lim = limit;
        var wide = size.Width > size.Height;
        return reduce == wide
            ? new Size(limit, (int)(size.Height / (size.Width / lim)))
            : new Size((int)(size.Width / (size.Height / lim)), limit);
    }

    public static Size EnureIsWideEnough(this Size size, int width)
    {
        if (size.Width >= width) return size;

        var height = width / size.AspectRatio();
        return new Size(width, height.RoundInt());
    }

    public static double AspectRatio(this Size size) => size.Width / (double)size.Height;

    //public static bool SizeIsMp4Invalid(int w, int h) => ((w | h) & 1) == 1;

    public static Size ValidMp4Size(this Size size) => new(size.Width.ToEven(), size.Height.ToEven());

    public static Size CeilingInt(this SizeF size) => new(size.Width.CeilingInt(), size.Height.CeilingInt());
}

/// Iterates a <see cref="Size"/> object via 45° dot grid.
/// <param name="size">Object for iterating.</param>
/// <param name="step">Vertical and horizontal distance between dots on the grid.</param>
public class SizeIterator_45deg(Size size, int step) : IEnumerable<Point>
{
    public IEnumerator<Point> GetEnumerator()
    {
        var halfStep = step / 2;
        var row = 0;
        for (var y = 0; y < size.Height; y += halfStep)
        {
            var oddRow = row % 2 != 0;

            for (var x = oddRow ? halfStep : 0; x < size.Width; x += step)
            {
                yield return new Point(x, y);
            }

            row++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}