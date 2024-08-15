using SixLabors.ImageSharp;

namespace Witlesss.Backrooms;

public static partial class Extensions
{
    public static Size GrowSize(this Size size, int minSemiperimeter = 400)
    {
        if (size.Width + size.Height >= minSemiperimeter) return size;

        var ratio = size.AspectRatio();
        var wide = size.Width > size.Height;
        var limit = (int)(minSemiperimeter * ratio / (ratio + 1)); // i have no idea what it does
        return size.Normalize(limit, reduce: wide);
    }

    /// <summary>
    /// Makes sure image can go through a square hole with a side = <b>max</b>.
    /// </summary>
    public static Size FitSize(this Size size, int max = 1280)
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

    /*
    public static Size Normalize(this Size size, Size limit, bool reduce = true)
    {
        (double w, double h) = limit;
        var wide = size.Width > size.Height;
        return reduce == wide
            ? new Size(limit.Width, (int)(size.Height / (size.Width / lim)))
            : new Size((int)(size.Width / (size.Height / lim)), limit);
    }*/

    public static Size Normalize(this Size size, int limit = 512, bool reduce = true)
    {
        double lim = limit;
        var wide = size.Width > size.Height;
        return reduce == wide
            ? new Size(limit, (int)(size.Height / (size.Width / lim)))
            : new Size((int)(size.Width / (size.Height / lim)), limit);
    }

    public static Size EnureIsWideEnough(this Size size, int width = 240)
    {
        if (size.Width >= width) return size;

        var height = width / size.AspectRatio();
        return new Size(width, height.RoundInt());
    }

    public static double AspectRatio(this Size size) => size.Width / (double)size.Height;

    //public static bool SizeIsMp4Invalid(int w, int h) => ((w | h) & 1) == 1;

    public static Size ValidMp4Size(this Size size) => new(size.Width.ToEven(), size.Height.ToEven());

    public static Size CeilingInt(this SizeF size) => new(size.Width.CeilingInt(), size.Height.CeilingInt());

    public static System.Drawing.Size Ok(this Size size) => new(size.Width, size.Height);
    public static Size Ok(this System.Drawing.Size size) => new(size.Width, size.Height);
}