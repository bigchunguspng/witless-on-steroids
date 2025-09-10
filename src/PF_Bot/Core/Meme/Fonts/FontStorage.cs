using SixLabors.Fonts;

namespace PF_Bot.Core.Meme.Fonts;

/// Storage for main and fallback <see cref="FontFamily">fonts</see>.
public static class FontStorage
{
    /// Main fonts by their key.
    public static readonly Dictionary<string, FontFamily> Families;

    /// Falback fonts: Exotic.
    public static readonly IReadOnlyList<FontFamily> Fallback_Default;

    /// Falback fonts: Main regular > Exotic. 
    public static readonly IReadOnlyList<FontFamily> Fallback_Regular;

    /// Falback fonts: Main comic > Main regular > Exotic.
    public static readonly IReadOnlyList<FontFamily> Fallback_Comic;

    static FontStorage()
    {
        var collectionMain     = new FontCollection();
        var collectionFallback = new FontCollection();

        var fontFiles = Dir_Fonts.GetFiles()
            .OrderBy(x => x).ToArray();
        var familyCodes = fontFiles
            .Select(Path.GetFileNameWithoutExtension).OfType<string>()
            .Where(x => x.Contains('-') == false).ToArray();

        fontFiles
            .ForEach(x => collectionMain.Add(x));
        var families    = collectionMain.Families.ToArray();

        Families = new Dictionary<string, FontFamily>(familyCodes.Length);
        for (var i = 0; i < familyCodes.Length; i++)
        {
            Families.Add(familyCodes[i], families[i]);
        }

        Dir_Fonts_Fallback.GetFiles()
            .OrderBy(x => x)
            .ForEach(x =>  collectionFallback.Add(x));
        Fallback_Default = collectionFallback.Families.ToList();
        Fallback_Regular = new[] { Families["sg"] }.Concat(Fallback_Default).ToList();
        Fallback_Comic   = new[] { Families["co"] }.Concat(Fallback_Regular).ToList();
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