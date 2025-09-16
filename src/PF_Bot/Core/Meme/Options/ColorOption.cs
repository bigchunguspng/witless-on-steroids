using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Core.Meme.Options;

public enum ColorOptionMode
{
    Off,
    Color,
    Coords,
}

public readonly record struct ColorOption
(
    ColorOptionMode Mode, Rgba32 Color, byte Coords
)
{
    public bool ByCoords => Mode == ColorOptionMode.Coords;

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