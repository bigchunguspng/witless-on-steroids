using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp.PixelFormats;
using Witlesss.Commands.Meme;
using SpectreColor = Spectre.Console.Color;

namespace Witlesss.Backrooms.Types;

public struct CustomColorOption
{
    private static readonly List<string> _colorNames;

    public bool IsActive;
    public Rgba32 Color;

    static CustomColorOption()
    {
        _colorNames = SpectreColor.Black.GetType().GetProperties()
            .Where(x => x.PropertyType == SpectreColor.Black.GetType())
            .Skip(1).Select(x => x.Name.ToLower()).ToList();
    }

    public Rgba32? GetColor() => IsActive ? Color : null;

    public void CheckAndCut(MemeRequest request, Regex regex)
    {
        IsActive = false;

        var value = OptionsParsing.GetValue(request, regex);
        if (value is null) return;

        var index = _colorNames.IndexOf(value);
        if (index == -1) index = _colorNames.IndexOf(value + "1");
        if (index == -1)
        {
            // check for hex code
            return;
        }

        var color = SpectreColor.FromInt32(index);
        Color = new Rgba32(color.R, color.G, color.B);
        IsActive = true;
    }
}