using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp.PixelFormats;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Meme.Core;
using SpectreColor = Spectre.Console.Color;

namespace Witlesss.Memes.Shared;

public struct CustomColorOption
{
    private static readonly List<string> _colorNames;
    private static readonly Regex _color = new(@"#([a-z0-9_]+)#"), _hex = new(@"^[0-9a-f]{3}$|^[0-9a-f]{6}$");

    public bool IsActive;
    public Rgba32 Color;

    static CustomColorOption()
    {
        _colorNames = SpectreColor.Black.GetType().GetProperties()
            .Where(x => x.PropertyType == SpectreColor.Black.GetType())
            .Skip(1).Select(x => x.Name.ToLower()).ToList();
    }

    public Rgba32? GetColor() => IsActive ? Color : null;

    public void CheckAndCut(MemeRequest request, Regex? regex = null)
    {
        IsActive = false;

        regex ??= _color;

        var value = OptionsParsing.GetValue(request, regex);
        if (value is null) return;

        var hexProvided = _hex.IsMatch(value) && Rgba32.TryParseHex(value, out Color);
        if (hexProvided == false)
        {
            var index = _colorNames.IndexOf(value);
            if (index == -1) index = _colorNames.IndexOf(value + "1");
            if (index == -1) return;

            var color = SpectreColor.FromInt32(index);
            Color = new Rgba32(color.R, color.G, color.B);
        }

        IsActive = true;
    }
}