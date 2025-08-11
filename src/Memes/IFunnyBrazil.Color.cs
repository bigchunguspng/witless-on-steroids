using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Witlesss.Memes;

public partial class IFunnyBrazil
{
    private static readonly SolidBrush _white = new(Color.White);
    private static readonly SolidBrush _black = new(Color.Black);

    private Rgba32     Background;
    private SolidBrush TextBrush = default!;

    private void SetColor(Image<Rgba32>? image)
    {
        var custom = CustomColor.IsActive;
        var pick = PickColor && image != null;

        Background = CustomColor.GetColor(image) ?? (pick ? PickColorFromImage(image!) : Color.White);
        TextBrush  = (custom || pick) && Background.Rgb.WhiteTextIsBetter() ? _white : _black;
    }

    private Rgba32 PickColorFromImage(Image<Rgba32> image)
    {
        var xd = ForceCenter ? 2 : 0;

        var colors = new Rgba32[7];
        colors[0] = AverageColorOnOffset(0);
        colors[1] = AverageColorOnOffset(image.Width * 1 / 8);
        colors[2] = AverageColorOnOffset(image.Width * 2 / 8);
        colors[3] = AverageColorOnOffset(image.Width * 4 / 8);
        colors[4] = AverageColorOnOffset(image.Width * 3 / 8);
        colors[5] = AverageColorOnOffset(image.Width * 7 / 8);
        colors[6] = AverageColorOnOffset(image.Width - 5);

        var difference = new int[7 - xd * 2];
        for (var i = 0; i < colors.Length - xd * 2; i++)
        {
            difference[i] = colors[xd..^xd].Select(c => Difference(colors[i + xd], c)).OrderBy(x => x).Take(3).Sum();
        }

        var min = difference.Min();

        return min > 950 ? Average(colors[0], colors[^1]) : colors[difference.ToList().IndexOf(min) + xd];

        Rgba32 AverageColorOnOffset(int x)
        {
            var avg = AverageColor(image, new Rectangle(x, _cropOffset, 5, 5));
            return BackInBlack ? avg : PutOver(Color.White, avg);
        }
    }

    private static Rgba32 AverageColor(Image<Rgba32> image, Rectangle area)
    {
        int a = 0, r = 0, g = 0, b = 0;
        int w = area.Width, h = area.Height, s = w * h;
        int maxX = area.X + w, maxY = area.Y + h;

        for (var x = area.X; x < maxX; x++)
        for (var y = area.Y; y < maxY; y++)
        {
            var p = image[x, y];
            a += p.A;
            r += p.R;
            b += p.B;
            g += p.G;
        }

        return new Rgba32((r / s).ClampByte(), (g / s).ClampByte(), (b / s).ClampByte(), (a / s).ClampByte());
    }

    private static int Difference(Rgba32 a, Rgba32 b)
    {
        return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
    }

    private static Rgba32 Average(Rgba32 a, Rgba32 b)
    {
        return a.CombineWith(b, (x, y) => ((x + y) / 2).ClampByte());
    }

    private static Rgba32 PutOver(Rgba32 a, Rgba32 b)
    {
        return a.CombineWith(b, (x, y) => (x * (255 - b.A) / 255 + y * b.A / 255).ClampByte()); // lerp
    }
}