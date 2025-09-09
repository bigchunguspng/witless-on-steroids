using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Generate.Memes.Core;
using SixLabors.Fonts;
using FontSpecificData = (float size, float offset, float caps);

namespace PF_Bot.Core.Memes.Shared
{
    public class FontWizard
    {
        private static readonly Dictionary<string, FontFamily> _families;
        private static readonly IReadOnlyList<FontFamily> _fallbackDefault, _fallbackCo, _fallbackSg;

        private readonly Regex _regex;

        private string? _fontKey, _styleKey;

        public bool UseRandom { get; private set; }

        static FontWizard()
        {
            var files = Directory.GetFiles(Dir_Fonts).OrderBy(x => x).ToArray();
            var collection = new FontCollection();
            files.ForEach(file => collection.Add(file));

            var familyCodes = files
                .Select(Path.GetFileNameWithoutExtension)
                .OfType<string>()
                .Where(x => !x.Contains('-')).ToArray();

            _families = new Dictionary<string, FontFamily>(familyCodes.Length);
            var families = collection.Families.ToArray();
            for (var i = 0; i < familyCodes.Length; i++)
            {
                _families.Add(familyCodes[i], families[i]);
            }

            var fallback = new FontCollection();
            Directory.GetFiles(Dir_Fonts_Fallback)
                .OrderBy(x => x)
                .ForEach(x => fallback.Add(x));
            _fallbackDefault = fallback.Families.ToList();
            _fallbackSg = new[] { _families["sg"] }.Concat(_fallbackDefault).ToList();
            _fallbackCo = new[] { _families["co"] }.Concat(_fallbackSg).ToList();
        }

        public static IEnumerable<string> Keys => _families.Keys;

        public IReadOnlyList<FontFamily> GetFallbackFamilies()
        {
            return FontIsFallback()
                ? _fallbackDefault
                : FallbackWithComicSans()
                    ? _fallbackCo
                    : _fallbackSg;
        }

        public FontWizard
        (
            [StringSyntax("Regex")] string cmdRegex,
            [StringSyntax("Regex")] string? x = null
        )
        {
            var codes = string.Join('|', _families.Keys);
            _regex = new Regex($@"^\/{cmdRegex}\S*(?:({codes}|\^\^)(-[bi]{{1,2}})?){x}\S*", RegexOptions.IgnoreCase);
        }

        public FontFamily GetFontFamily(string @default)
        {
            _fontKey ??= @default;
            if (UseRandom) _fontKey = _families.Keys.PickAny();
            return _families[_fontKey];
        }

        public FontStyle GetFontStyle(FontFamily family)
        {
            var available = family.GetAvailableStyles().ToHashSet();

            var aR = available.Contains(FontStyle.Regular);
            var aI = available.Contains(FontStyle.Italic);

            if (_styleKey is null)
            {
                return UseRandom
                    ? available.PickAny()
                    : aR
                        ? FontStyle.Regular
                        : FontStyle.Bold;
            }

            var b = _styleKey.Contains('b');
            var i = _styleKey.Contains('i');

            return (b, i) switch
            {
                (false, false) => aR ? FontStyle.Regular : FontStyle.Bold,
                (false, true ) => aI ? FontStyle.Italic : FontStyle.BoldItalic,
                (true , false) => FontStyle.Bold,
                (true , true ) => FontStyle.BoldItalic
            };
        }

        public float GetLineSpacing() => GetRelativeSize();
        public float GetSizeMultiplier() => 1 / GetRelativeSize();

        public float GetRelativeSize       () => _fontKey is null ? 1F : GetFontSpecific(_fontKey).size;
        public float GetCapitalsOffset     () => _fontKey is null ? 0F : GetFontSpecific(_fontKey).caps;
        public float GetFontDependentOffset() => _fontKey is null ? 0F : GetFontSpecific(_fontKey).offset;

        public float GetCaseDependentOffset(string text)
        {
            return FontIsMulticase() ? GetSizeMultiplier() * GetCapitalsOffset() * text.GetLowercaseRatio() : 0F;
        }

        private FontSpecificData GetFontSpecific(string key) => key switch
        {
            //          size    offset     caps
            "ap" => (1.0271F,  0.0562F, 0.0000F),
            "bb" => (1.0024F,  0.1200F, 0.0762F),
            "bc" => (1.1534F,  0.0335F, 0.0000F),
            "bl" => (0.9388F,  0.0650F, 0.0000F),
            "co" when _styleKey is not null and not "-b"
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

        private bool FontIsMulticase() => _fontKey switch
        {
            "ap" => false,
            "bc" => false,
            "bl" => false,
            "mc" => false,
            "st" => false,
            "vb" => false,
            "vp" => false,
            _ => true,
        };

        private bool FontIsFallback() => _fontKey switch
        {
            "co" => true,
            "sg" => true,
            _    => false
        };

        private bool FallbackWithComicSans() => _fontKey switch
        {
            "bc" => true,
            "bl" => true,
            "go" => true,
            "mc" => true,
            "ro" => true,
            "ru" => true,
            "vb" => true,
            "vg" => true,
            _    => false
        };

        public bool FontIsPixelated() => _fontKey == "vn";

        public void CheckAndCut(MemeRequest request)
        {
            var match = _regex.Match(request.Dummy);

            var success = !request.Empty && match.Success;
            if (success)
            {
                var g1 = match.Groups[1];
                _fontKey = g1.Value;
                var g2 = match.Groups[2];
                _styleKey = g2.Success ? g2.Value : null;

                for (var i = match.Groups.Count - 1; i > 0; i--)
                {
                    OptionsParsing.CutCaptureOut(match.Groups[i], request);
                }
            }
            else
            {
                _fontKey = _styleKey = null;
            }

            UseRandom = _fontKey is "^^";
        }

        public static void Debug_GetFontData()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            foreach (var pair in _families)
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
}