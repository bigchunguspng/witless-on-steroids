using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Meme.Core;
using SpectreColor = Spectre.Console.Color;

namespace Witlesss.Memes.Shared;

public struct CustomColorOption([StringSyntax("Regex")] string mark)
{
    private static readonly List<string> _colorNames;
    private static readonly Regex _hex = new("^[0-9a-f]{3}$|^[0-9a-f]{6}$"), _coords = new(@"^\d{2}$");

    private readonly Regex _color = new($"{mark}([a-z0-9_]+){mark}");
    private Rgba32 Color;
    private byte Coords;
    public  bool IsActive, ByCoords;

    static CustomColorOption()
    {
        _colorNames = SpectreColor.Black.GetType().GetProperties()
            .Where(x => x.PropertyType == SpectreColor.Black.GetType())
            .Skip(1).Select(x => x.Name.ToLower()).ToList();
    }

    public Rgba32? GetColor(Image<Rgba32>? image)
    {
        return IsActive ? ByCoords && image != null ? PickColor(image) : Color : null;
    }

    public void CheckAndCut(MemeRequest request, Regex? regex = null)
    {
        IsActive = false;
        ByCoords = false;

        regex ??= _color;

        var value = OptionsParsing.GetValue(request, regex);
        if (value is null) return;

        var hexProvided = _hex.IsMatch(value) && Rgba32.TryParseHex(value, out Color);
        if (hexProvided == false)
        {
            var coordsProvided = _coords.IsMatch(value);
            if (coordsProvided)
            {
                Coords = byte.Parse(value);
                ByCoords = true;
            }
            else
            {
                var index = _colorNames.IndexOf(value);
                if (index == -1) index = _colorNames.IndexOf(value + "1");
                if (index == -1) return;

                var color = SpectreColor.FromInt32(index);
                Color = new Rgba32(color.R, color.G, color.B);
            }
        }

        IsActive = true;
    }

    private Rgba32 PickColor(Image<Rgba32> image)
    {
        var x = Coords / 10;
        var y = Coords % 10;
        var ix = image.Width  * 0.05F + image.Width  * (x / 10F);
        var iy = image.Height * 0.05F + image.Height * (y / 10F);
        return image[ix.RoundInt(), iy.RoundInt()];
    }
}