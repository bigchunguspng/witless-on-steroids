using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;

namespace Witlesss.Services
{
    public class ExtraFonts
    {
        private readonly Regex _regex;
        private static readonly Dictionary<string, FontFamily> _fonts;

        public static bool UseOtherFont;
        public static string  OtherFontKey;

        static ExtraFonts()
        {
            var files = Directory.GetFiles(Config.Fonts);
            var collection = new FontCollection();
            files.ForEach(file => collection.Add(file));

            _fonts = new Dictionary<string, FontFamily>(files.Length);
            var families = collection.Families.ToArray();
            for (var i = 0; i < files.Length; i++)
            {
                var key = Path.GetFileNameWithoutExtension(files[i]);
                _fonts.Add(key, families[i]);
            }
        }

        public ExtraFonts(string cmd, params string[] exclude)
        {
            var names = _fonts.Keys.Select(Path.GetFileNameWithoutExtension).Where(x => !exclude.Contains(x));
            _regex = new Regex($@"^\/{cmd}\S*({string.Join('|', names)})\S*", RegexOptions.IgnoreCase);
        }

        public static FontFamily GetOtherFont(string @default) => _fonts[UseOtherFont ? OtherFontKey : @default];

        public void CheckKey(bool empty, ref string dummy)
        {
            var match = _regex.Match(dummy);

            UseOtherFont = !empty && match.Success;
            if (UseOtherFont)
            {
                var group = match.Groups[1];
                OtherFontKey = group.Value;
                CutCaptureOut(group, ref dummy);
            }
        }
    }
}