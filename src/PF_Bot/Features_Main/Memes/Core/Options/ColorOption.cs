using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Features_Main.Memes.Core.Options;

public enum ColorOptionMode : byte
{
    Off,
    Color,
    Coords,
}

public readonly struct ColorOption(ColorOptionMode mode, Rgba32 color = default, byte coords = 0)
{
    public Rgba32         Color { get; } = color;
    public ColorOptionMode Mode { get; } = mode;
    public byte          Coords { get; } = coords;

    public bool ByCoords => Mode == ColorOptionMode.Coords;

    public static ColorOption FromColor(Rgba32 color) => new(ColorOptionMode.Color, color);
    public static ColorOption FromCoords(byte coords) => new(ColorOptionMode.Coords, coords: coords);

    public Rgba32? GetColor(Image<Rgba32>? image) => Mode switch
    {
        ColorOptionMode.Off        => null,
        ColorOptionMode.Color => Color,
        ColorOptionMode.Coords   => PickColor(image!),
        _ => throw new ArgumentOutOfRangeException(),
    };

    private Rgba32 PickColor(Image<Rgba32> image)
    {
        var x = Coords / 10;
        var y = Coords % 10;
        var ix = image.Width  * 0.05F + image.Width  * (x / 10F);
        var iy = image.Height * 0.05F + image.Height * (y / 10F);
        return image[ix.RoundInt(), iy.RoundInt()];
    }
}