using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Meme;

namespace Witlesss.Memes.Shared
{
    public class ExtraFonts // todo rename since it is the main font source now
    {
        private static readonly Dictionary<string, FontFamily> _families;
        //private static readonly FontCollection _fallback;

        private readonly Regex _regex;

        private string? _fontKey, _styleKey;

        static ExtraFonts()
        {
            var files = Directory.GetFiles(Paths.Dir_Fonts);
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

            /*
            var fallback = Directory.GetFiles(Path.Combine(Paths.Dir_Fonts, "Fallback"));
            _fallback = new FontCollection();
            fallback.ForEach(file => _fallback.Add(file));

            FallbackFamilies = _fallback.Families.ToList();
            */
            /*.Append(SystemFonts.Get("MS PGothic"))
            .Append(SystemFonts.Get("Segoe UI Symbol"))
            .ToList();*/
        }

        public static List<FontFamily> FallbackFamilies { get; } = [];/* =
        [
            SystemFonts.Get("MS PGothic"),
            SystemFonts.Get("Segoe UI Symbol")
        ];*/

        public ExtraFonts(string cmd, params string[] exclude)
        {
            var codes = string.Join('|', _families.Keys.Where(x => !exclude.Contains(x)));
            _regex = new Regex($@"^\/{cmd}\S*({codes})(-[bi]{{1,2}})?\S*", RegexOptions.IgnoreCase);
        }

        public Font GetFont(string @default, float size)
        {
            var family = GetFontFamily(@default);
            return family.CreateFont(size, GetFontStyle(family));
        }

        public FontFamily GetFontFamily(string @default)
        {
            return _families[_fontKey ??= @default];
        }

        public FontStyle GetFontStyle(FontFamily family)
        {
            var available = family.GetAvailableStyles().ToHashSet();

            var aR = available.Contains(FontStyle.Regular);
            var aI = available.Contains(FontStyle.Italic);

            if (_styleKey is null) return aR ? FontStyle.Regular : FontStyle.Bold;

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

        public float GetVerticalOffset() => _fontKey is null ? 0F : _fontData[_fontKey].offset;
        public float GetCapitalsOffset() => _fontKey is null ? 0F : _fontData[_fontKey].caps;
        public float GetRelativeSize  () => _fontKey is null ? 1F : _fontData[_fontKey].size;

        private static readonly Dictionary<string, (float offset, float size, float caps)> _fontData = new()
        {
            { "ap", (-0.0058F, 1.0230F, 0.0000F) },
            { "bb", (-0.0251F, 1.0024F, 0.0936F) },
            { "bc", (-0.0096F, 1.1238F, 0.0000F) },
            { "bl", (-0.0005F, 0.9388F, 0.0000F) },
            { "cr", (-0.0528F, 0.9586F, 0.0947F) },
            { "ft", (-0.1894F, 1.0064F, 0.1071F) },
            { "go", (-0.2978F, 1.1209F, 0.1252F) },
            { "im", (-0.1350F, 1.1160F, 0.0796F) },
            { "mc", ( 0.0793F, 1.0828F, 0.0000F) },
            { "rg", (-0.1318F, 1.0037F, 0.0916F) },
            { "sg", (-0.2090F, 0.9885F, 0.0989F) },
            { "st", (-0.0002F, 1.0165F, 0.0000F) },
            { "tm", (-0.1346F, 0.9347F, 0.1004F) },
            { "vb", (-0.0524F, 0.7793F, 0.0000F) },
            { "vg", (-0.1007F, 0.8400F, 0.0894F) },
            { "vn", (-0.1254F, 1.2353F, 0.1544F) },
            { "vp", (-0.0732F, 1.0836F, 0.0000F) },
        };

        public bool FontIsMulticase() => _fontKey switch
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

                OptionsParsing.CutCaptureOut(g2, request);
                OptionsParsing.CutCaptureOut(g1, request);
            }
            else
            {
                _fontKey = _styleKey = null;
            }
        }
    }
}