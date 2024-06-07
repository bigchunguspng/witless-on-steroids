using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;
using Witlesss.Commands.Meme;

namespace Witlesss.Services
{
    public class ExtraFonts
    {
        private static readonly Dictionary<string, FontFamily> _families;

        private readonly Regex _regex;

        private string? _fontKey, _styleKey;

        static ExtraFonts()
        {
            var files = Directory.GetFiles(Config.Fonts);
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
        }

        public static List<FontFamily> FallbackFamilies { get; } =
        [
            SystemFonts.Get("Segoe UI"),
            SystemFonts.Get("Segoe UI Symbol"),
            SystemFonts.Get("Arial"),
            SystemFonts.Get("Tahoma"),
            SystemFonts.Get("Times New Roman")
        ];

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

        public float GetVerticalOffset() => _fontKey switch
        {
            "bb" =>  0.035F,
            "cb" =>  0.056F,
            "cr" =>  0,
            "ft" => -0.153F,
            "im" => -0.076F,
            "rg" => -0.076F,
            "sg" => -0.16F,
            "tm" => -0.083F,
            "vb" => -0.021F,
            "vg" => -0.076F,
            "vp" => -0.021F,
            "vn" =>  0,
            _    =>  0,
        };

        private float GetRelativeSize() => _fontKey switch
        {
            "vn" => 1.229F,
            "im" => 1.114F,
            "vp" => 1.114F,
            "vb" => 1.029F,
            "cr" => 0.971F,
            "tm" => 0.943F,
            "cb" => 0.876F,
            "vg" => 0.8F,
            _    => 1F,
        };

        private bool FontIsUppercase() => _fontKey switch
        {
            "cb" => true,
            "vp" => true,
            "vb" => true,
            _ => false,
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