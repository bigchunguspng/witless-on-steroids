using System.Diagnostics.CodeAnalysis;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Handlers.Memes.Core;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;

namespace PF_Bot.Core.Memes.Options;

public class ColorWizard([StringSyntax("Regex")] string marker)
{
    private static readonly List<string> _colorNames;
    private static readonly Regex
        _rgx_hex    = new("^[0-9a-f]{3}$|^[0-9a-f]{6}$",  RegexOptions.Compiled),
        _rgx_coords = new(@"^\d{2}$",                     RegexOptions.Compiled);

    private readonly Regex
        _rgx_color = new($"{marker}([a-z0-9_]+){marker}");

    static ColorWizard()
    {
        var type = Color.Black.GetType();
        _colorNames = type.GetProperties()
            .Where(x => x.PropertyType == type)
            .Skip(1)
            .Select(x => x.Name.ToLower()).ToList();
    }

    public ColorOption CheckAndCut(MemeRequest request)
    {
        var value = OptionsParsing.GetValue(request, _rgx_color);
        if (value == null)
            return new ColorOption(ColorOptionMode.Off);

        if (_rgx_hex   .IsMatch(value) && Rgba32.TryParseHex(value, out var color))
            return ColorOption.FromColor(color);

        if (_rgx_coords.IsMatch(value))
            return ColorOption.FromCoords(byte.Parse(value));

        var              index = _colorNames.IndexOf(value);
        if (index == -1) index = _colorNames.IndexOf(value + "1");
        if (index == -1)
            return new ColorOption(ColorOptionMode.Off);

        return ColorOption.FromColor(Color.FromInt32(index).ToRgba32());
    }
}