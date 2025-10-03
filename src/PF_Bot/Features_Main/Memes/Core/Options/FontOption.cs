using SixLabors.Fonts;
using FontSpecificData = (float size, float offset, float caps);

namespace PF_Bot.Features_Main.Memes.Core.Options;

public class FontOption(string fontKey, string? styleKey, bool random, bool @default)
{
    public string   FontKey { get; set; } = fontKey;
    public string? StyleKey { get; }     = styleKey;

    public bool IsRandom  => random;
    public bool IsDefault => @default;

    public FontFamily GetFontFamily()
    {
        if (IsRandom)
        {
            FontKey = FontStorage.Families.Keys.PickAny();
        }

        return FontStorage.Families[FontKey];
    }

    public FontStyle GetFontStyle(FontFamily family)
    {
        var available = family.GetAvailableStyles().ToHashSet();

        var aR = available.Contains(FontStyle.Regular);
        var aI = available.Contains(FontStyle.Italic);

        if (StyleKey is null)
        {
            return IsRandom
                ? available.PickAny()
                : aR
                    ? FontStyle.Regular
                    : FontStyle.Bold;
        }

        var b = StyleKey.Contains('b');
        var i = StyleKey.Contains('i');

        return (b, i) switch
        {
            (false, false) => aR ? FontStyle.Regular : FontStyle.Bold,
            (false, true ) => aI ? FontStyle.Italic : FontStyle.BoldItalic,
            (true , false) => FontStyle.Bold,
            (true , true ) => FontStyle.BoldItalic,
        };
    }

    public IReadOnlyList<FontFamily> GetFallbackFamilies()
    {
        return FontIsFallback()
            ? FontStorage.Fallback_Default
            : FallbackWithComicSans()
                ? FontStorage.Fallback_Comic
                : FontStorage.Fallback_Regular;
    }

    public float GetLineSpacing   () =>     GetRelativeSize();
    public float GetSizeMultiplier() => 1 / GetRelativeSize();

    public float GetRelativeSize       () => GetFontSpecific(FontKey).size;
    public float GetCapitalsOffset     () => GetFontSpecific(FontKey).caps;
    public float GetFontDependentOffset() => GetFontSpecific(FontKey).offset;

    public float GetCaseDependentOffset(string text)
    {
        return FontIsAllUPPERCASE() 
            ? 0F 
            : GetSizeMultiplier() * GetCapitalsOffset() * text.GetLowercaseRatio();
    }

    private FontSpecificData GetFontSpecific(string key) => key switch
    {
        //          size    offset     caps
        "ap" => (1.0271F,  0.0562F, 0.0000F),
        "bb" => (1.0024F,  0.1200F, 0.0762F),
        "bc" => (1.1534F,  0.0335F, 0.0000F),
        "bl" => (0.9388F,  0.0650F, 0.0000F),
        "co" when StyleKey is not null and not "-b"
            => (1.1029F,  0.1033F, 0.1325F),
        "co" => (1.1029F, -0.0400F, 0.1325F),
        "ft" => (1.0064F, -0.0435F, 0.1071F),
        "go" => (1.0955F, -0.1280F, 0.1139F),
        "im" => (1.1167F, -0.0034F, 0.0799F),
        "mc" => (1.0913F,  0.1345F, 0.0000F),
        "rg" => (1.0037F,  0.0137F, 0.0916F),
        "ro" => (0.9347F,  0.0149F, 0.1004F),
        "ru" => (0.9388F,  0.1090F, 0.1051F),
        "sg" => (0.9885F, -0.0640F, 0.0929F),
        "st" => (1.0165F,  0.0430F, 0.0000F),
        "ug" => (0.9911F,  0.1025F, 0.1308F),
        "vb" => (0.8093F, -0.0255F, 0.0000F),
        "vg" => (0.8273F,  0.0330F, 0.0811F),
        "vn" => (1.2353F,  0.1250F, 0.1544F),
        "vp" => (1.0712F, -0.0249F, 0.0000F),
        _    => (1.0000F,  0.0000F, 0.1000F),
    };

    private bool FontIsAllUPPERCASE() => FontKey switch
    {
        "ap" => true,
        "bc" => true,
        "bl" => true,
        "mc" => true,
        "st" => true,
        "vb" => true,
        "vp" => true,
        _ => false,
    };

    private bool FontIsFallback() => FontKey switch
    {
        "co" => true,
        "sg" => true,
        _    => false,
    };

    private bool FallbackWithComicSans() => FontKey switch
    {
        "bc" => true,
        "bl" => true,
        "go" => true,
        "mc" => true,
        "ro" => true,
        "ru" => true,
        "vb" => true,
        "vg" => true,
        _    => false,
    };

    public bool FontIsPixelated() => FontKey == "vn";
}