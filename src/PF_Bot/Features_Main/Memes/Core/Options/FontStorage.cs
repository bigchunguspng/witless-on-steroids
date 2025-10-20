using System.Collections.Frozen;
using SixLabors.Fonts;

namespace PF_Bot.Features_Main.Memes.Core.Options;

/// Storage for main and fallback <see cref="FontFamily">fonts</see>.
public static class FontStorage
{
    /// Main fonts by their key.
    public static readonly FrozenDictionary<string, FontFamily> Families;

    /// Falback fonts: Exotic.
    public static readonly IReadOnlyList<FontFamily> Fallback_Default;

    /// Falback fonts: Main regular > Exotic. 
    public static readonly IReadOnlyList<FontFamily> Fallback_Regular;

    /// Falback fonts: Main comic > Main regular > Exotic.
    public static readonly IReadOnlyList<FontFamily> Fallback_Comic;

    static FontStorage()
    {
        var families_main = LoadFontFamilies(Dir_Fonts,          out var files_main);
        var families_fall = LoadFontFamilies(Dir_Fonts_Fallback, out _);

        Families = files_main
            .Select(Path.GetFileNameWithoutExtension).OfType<string>()
            .Where(key => key.Contains('-').Janai())
            .Zip(families_main, (key, font) => new KeyValuePair<string, FontFamily>(key, font))
            .ToFrozenDictionary();

        Fallback_Default = families_fall.ToArray();
        Fallback_Regular = new[] { Families["sg"] }.Concat(Fallback_Default).ToList();
        Fallback_Comic   = new[] { Families["co"] }.Concat(Fallback_Regular).ToList();
    }

    private static IEnumerable<FontFamily> LoadFontFamilies(FilePath directory, out string[] fontFiles)
    {
        var collection = new FontCollection();
        fontFiles = directory.GetFiles().OrderBy(x => x).ToArray();
        fontFiles.ForEach(x => collection.Add(x));
        return collection.Families;
    }

    public static void Debug_GetFontData()
    {
        foreach (var pair in Families)
        {
            var bound1 = TextMeasurer.MeasureBounds("И", new TextOptions(pair.Value.CreateFont(48)));
            var bound2 = TextMeasurer.MeasureBounds("и", new TextOptions(pair.Value.CreateFont(48)));
            var marginT =  00 + bound1.Top     / 48;
            var marginB = (48 - bound1.Bottom) / 48;
            var offset = (marginB - marginT) / 2F;
            var relativeSize = bound1.Height / 34;
            var caps = (bound1.Height - bound2.Height) / 2 / (48 / relativeSize);
            Print($"\"{pair.Key}\" => ({relativeSize:F4}F, {offset:F4}F, {caps:F4}F),");
        }
    }
}