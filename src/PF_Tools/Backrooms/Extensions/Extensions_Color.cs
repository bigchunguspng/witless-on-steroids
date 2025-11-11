using ColorHelper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Color
{
    public static Rgba32 ToRgba32(this Spectre.Console.Color color) => new (color.R, color.G, color.B);

    public static Rgb24 ToRgb24(this RGB color) => new(color.R, color.G, color.B);
    public static RGB   ToRGB(this Rgb24 color) => new(color.R, color.G, color.B);

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

    public static bool WhiteTextIsBetter(this Rgb24 background)
    {
        return background.ToOklch().L < 0.65F;
    }

    // OKLCH | Sauce: https://gist.github.com/dkaraush/65d19d61396f5f3cd8ba7d1b4b3c9432

    public static Oklch ToOklch(this Rgb24 color) => color.To_sRGB().ToXYZ().ToOklab().ToOklch();
    public static Rgb24 ToRgb24(this Oklch color) => color.ToOklab().ToXYZ().To_sRGB().ToRgb24();

    public static Oklab ToOklab(this Oklch color) => new
    (
        color.L,
        color.H is double.NaN ? 0 : color.C * Math.Cos(color.H * Math.PI / 180),
        color.H is double.NaN ? 0 : color.C * Math.Sin(color.H * Math.PI / 180)
    );

    public static Oklch ToOklch(this Oklab color) => new
    (
        color.L,
        Math.Sqrt(color.A * color.A + color.B * color.B),
        Math.Abs(color.A) < 0.0002 
     && Math.Abs(color.B) < 0.0002 
            ? double.NaN
            : (Math.Atan2(color.B, color.A) * 180 / Math.PI % 360 + 360) % 360
    );

    public static sRGB To_sRGB(this Rgb24 color) => new
    (
        Channel_RgbToLinearRGB(color.R / 255.0),
        Channel_RgbToLinearRGB(color.G / 255.0),
        Channel_RgbToLinearRGB(color.B / 255.0)
    );

    public static Rgb24 ToRgb24(this sRGB color) => new
    (
        (255 * Channel_LinearRGBToRgb(color.R)).RoundInt().ClampByte(),
        (255 * Channel_LinearRGBToRgb(color.G)).RoundInt().ClampByte(),
        (255 * Channel_LinearRGBToRgb(color.B)).RoundInt().ClampByte()
    );

    //
    private static double Channel_RgbToLinearRGB
        (double c) => Math.Abs(c) <= 0.04045
        ? c / 12.92
        : Sign(c) * Math.Pow((Math.Abs(c) + 0.055) / 1.055, 2.4);

    private static double Channel_LinearRGBToRgb
        (double c) => Math.Abs(c) > 0.0031308
        ? Sign(c) * (1.055 * Math.Pow(Math.Abs(c), 1 / 2.4) - 0.055)
        : 12.92 * c;

    private static int Sign(double c) => c < 0 ? -1 : 1;
    //

    public static   XYZ ToXYZ(this Oklab color)
    {
        var lms = MultiplyMatrices_9x3_3(M0, color.ToArray()).Select(x => Math.Pow(x, 3));
        return new XYZ(MultiplyMatrices_9x3_3(M1, lms.ToArray()));
    }

    public static Oklab ToOklab(this XYZ color)
    {
        var lms = MultiplyMatrices_9x3_3(M2, color.ToArray()).Select(Math.Cbrt);
        return new Oklab(MultiplyMatrices_9x3_3(M3, lms.ToArray()));
    }

    public static  sRGB To_sRGB
        (this  XYZ color) => new(MultiplyMatrices_9x3_3(M4, color.ToArray()));

    public static   XYZ ToXYZ
        (this sRGB color) => new(MultiplyMatrices_9x3_3(M5, color.ToArray()));

    //
    private static double[] MultiplyMatrices_9x3_3(Span<double> A9, Span<double> B3) =>
    [
        A9[0] * B3[0] + A9[1] * B3[1] + A9[2] * B3[2],
        A9[3] * B3[0] + A9[4] * B3[1] + A9[5] * B3[2],
        A9[6] * B3[0] + A9[7] * B3[1] + A9[8] * B3[2],
    ];

    private static Span<double> M0 => M.AsSpan(0 * 9, 9);
    private static Span<double> M1 => M.AsSpan(1 * 9, 9);
    private static Span<double> M2 => M.AsSpan(2 * 9, 9);
    private static Span<double> M3 => M.AsSpan(3 * 9, 9);
    private static Span<double> M4 => M.AsSpan(4 * 9, 9);
    private static Span<double> M5 => M.AsSpan(5 * 9, 9);

    private static readonly double[] M =
    [
        +1,                    0.39633777737617490,  0.21580375730991360, // M0
        +1,                   -0.10556134581565860, -0.06385417282581330, // M0
        +1,                   -0.08948417752981190, -1.29148554801940920, // M0
        +1.22687987584592430, -0.55781499446021710,  0.28139104566596470,    // M1
        -0.04057574521480080,  1.11228680328031700, -0.07171105806551640,    // M1
        -0.07637293667466010, -0.42149333240224320,  1.58692401983678160,    // M1
        +0.81902243799670300,  0.36190626005289040, -0.12887378152098790, // M2
        +0.03298365393238850,  0.92928686158634340,  0.03614466635064240, // M2
        +0.04817718935962420,  0.26423953175273080,  0.63354782846943090, // M2
        +0.21045426830931400,  0.79361777470230540, -0.00407204301161930,    // M3
        +1.97799853243116840, -2.42859224204857990,  0.45059370961741100,    // M3
        +0.02590404246554780,  0.78277171245752960, -0.80867575492307740,    // M3
        +3.24096994190452260, -1.53738317757009400, -0.49861076029300340, // M4
        -0.96924363628087960,  1.87596750150772020,  0.04155505740717559, // M4
        +0.05563007969699366, -0.20397695888897652,  1.05697151424287860, // M4
        +0.41239079926595934,  0.35758433938387800,  0.18048078840183430,    // M5
        +0.21263900587151027,  0.71516867876775600,  0.07219231536073371,    // M5
        +0.01933081871559182,  0.11919477979462598,  0.95053215224966070,    // M5
    ];
}

// TYPES

public readonly record struct Oklch(double L, double C, double H);
public readonly record struct Oklab(double L, double A, double B)
{
    public Oklab(double[] a) : this(a[0], a[1], a[2]) { }
    public double[] ToArray() => [L, A, B];
}

public readonly record struct   XYZ(double X, double Y, double Z)
{
    public   XYZ(double[] a) : this(a[0], a[1], a[2]) { }
    public double[] ToArray() => [X, Y, Z];
}

public readonly record struct  sRGB(double R, double G, double B)
{
    public  sRGB(double[] a) : this(a[0], a[1], a[2]) { }
    public double[] ToArray() => [R, G, B];
}