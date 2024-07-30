﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Meme;
using FontSpecificData = (float size, float marginT, float marginB, float offset, float caps);

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
            var files = Directory.GetFiles(Paths.Dir_Fonts).OrderBy(x => x).ToArray();
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

        public static IEnumerable<string> Keys => _families.Keys;

        public static List<FontFamily> FallbackFamilies { get; } = [];/* =
        [
            SystemFonts.Get("MS PGothic"),
            SystemFonts.Get("Segoe UI Symbol")
        ];*/

        public ExtraFonts(string cmdRegex, string? x = null)
        {
            var codes = string.Join('|', _families.Keys);
            _regex = new Regex($@"^\/{cmdRegex}\S*(?:({codes})(-[bi]{{1,2}})?){x}\S*", RegexOptions.IgnoreCase);
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

        public float GetRelativeSize       () => _fontKey is null ? 1F : GetFontSpecific(_fontKey).size;
        public float GetCapitalsOffset     () => _fontKey is null ? 0F : GetFontSpecific(_fontKey).caps;
        public float GetFontDependentOffset() => _fontKey is null ? 0F : GetFontSpecific(_fontKey).offset;

        public float GetCaseDependentOffset(string text)
        {
            var move = FontIsMulticase() && text.IsMostlyLowercase();
            return move ? GetSizeMultiplier() * GetCapitalsOffset() : 0;
        }

        private static FontSpecificData GetFontSpecific(string key) => key switch
        {
            //          size   marginT   marginB    offset     caps
            "ap" => (1.0271F,  0.0801F,  0.1924F,  0.0562F, 0.0000F),
            "bb" => (1.0024F,  0.0250F,  0.2650F,  0.1200F, 0.0762F),
            "bc" => (1.1534F,  0.0580F,  0.1250F,  0.0335F, 0.0000F),
            "bl" => (0.9388F,  0.0890F,  0.2190F,  0.0650F, 0.0000F),
            "co" => (1.1029F,  0.1494F,  0.0693F, -0.0400F, 0.1325F),
            "cr" => (0.9388F,  0.0585F,  0.2765F,  0.1090F, 0.1051F),
            "ft" => (1.0064F,  0.1870F,  0.1001F, -0.0435F, 0.1071F),
            "go" => (1.0955F,  0.2400F, -0.0160F, -0.1280F, 0.1139F),
            "im" => (1.1167F,  0.1079F,  0.1011F, -0.0034F, 0.0799F),
            "mc" => (1.0913F, -0.0210F,  0.2480F,  0.1345F, 0.0000F),
            "rg" => (1.0037F,  0.1309F,  0.1582F,  0.0137F, 0.0916F),
            "ro" => (0.9347F,  0.1541F,  0.1838F,  0.0149F, 0.1004F),
            "sg" => (0.9885F,  0.2139F,  0.0859F, -0.0640F, 0.0929F),
            "st" => (1.0165F,  0.0970F,  0.1830F,  0.0430F, 0.0000F),
            "vb" => (0.8093F,  0.3060F,  0.2550F, -0.0255F, 0.0000F),
            "vg" => (0.8273F,  0.1740F,  0.2400F,  0.0330F, 0.0811F),
            "vn" => (1.2353F, -0.0625F,  0.1875F,  0.1250F, 0.1544F),
            "vp" => (1.0712F,  0.1455F,  0.0957F, -0.0249F, 0.0000F),
            _    => (1.0000F,  0.1500F,  0.1500F,  0.0000F, 0.1000F),

            // NOTE: offset = (marginB - marginT) / 2
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

                OptionsParsing.CutCaptureOut(g2, request);
                OptionsParsing.CutCaptureOut(g1, request);
            }
            else
            {
                _fontKey = _styleKey = null;
            }
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
                Log($"\"{pair.Key}\" => ({relativeSize:F4}F, {marginT:F4}F, {marginB:F4}F, {offset:F4}F, {caps:F4}F),");
            }
        }
    }
}