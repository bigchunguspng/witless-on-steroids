using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;

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

        public ExtraFonts(string cmd, params string[] exclude)
        {
            var codes = string.Join('|', _families.Keys.Where(x => !exclude.Contains(x)));
            _regex = new Regex($@"^\/{cmd}\S*({codes})(-[bi]+)?\S*", RegexOptions.IgnoreCase);
        }

        public FontFamily GetFontFamily(string @default, bool forceDefault = false)
        {
            return _families[forceDefault ? @default : _fontKey ?? @default];
        }

        public FontStyle GetFontStyle()
        {
            if (_styleKey is null) return FontStyle.Regular;

            var b = _styleKey.Contains('b');
            var i = _styleKey.Contains('i');

            return (b, i) switch
            {
                (false, false) => FontStyle.Regular,
                (false, true ) => FontStyle.Italic,
                (true , false) => FontStyle.Bold,
                (true , true ) => FontStyle.BoldItalic
            };
        }

        public float GetLineSpacing() => GetRelativeSize();
        public float GetSizeMultiplier() => 1 / GetRelativeSize();

        private float GetRelativeSize() => _fontKey switch
        {
            "vn" => 32.25F / 26.25F,
            "im" => 29.25F / 26.25F,
            "vp" => 29.25F / 26.25F,
            "vb" => 27F    / 26.25F,
            "cr" => 25.5F  / 26.25F,
            "tm" => 24.75F / 26.25F,
            "cb" => 23F    / 26.25F,
            "vg" => 21F    / 26.25F,
            _    => 26.25F / 26.25F,
        };

        public void CheckKey(bool empty, ref string dummy)
        {
            var match = _regex.Match(dummy);

            var success = !empty && match.Success;
            if (success)
            {
                var g1 = match.Groups[1];
                _fontKey = g1.Value;
                var g2 = match.Groups[2];
                _styleKey = g2.Success ? g2.Value : null;

                CutCaptureOut(g1, ref dummy);
            }
            else
            {
                _fontKey = _styleKey = null;
            }
        }
    }
}