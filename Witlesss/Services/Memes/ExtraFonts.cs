using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Witlesss.Commands.Meme;

namespace Witlesss.Services.Memes
{
    public class ExtraFonts
    {
        public readonly Regex OtherFonts;
        public static bool UseOtherFont;
        public static string OtherFontKey;
        public static readonly Dictionary<string, FontFamily> Fonts;

        static ExtraFonts()
        {
            var files = Directory.GetFiles(Config.Fonts);
            Fonts = new(files.Length);
            var collection = new PrivateFontCollection();
            for (var i = 0; i < files.Length; i++)
            {
                collection.AddFontFile(files[i]);
                var key = Path.GetFileNameWithoutExtension(files[i]);
                Fonts.Add(key!, collection.Families[i]);
            }
        }

        public ExtraFonts(string cmd, params string[] exclude)
        {
            var names = Fonts.Keys.Select(Path.GetFileNameWithoutExtension).Where(x => !exclude.Contains(x));
            OtherFonts = new Regex($@"^\/{cmd}\S*({string.Join('|', names)})\S*", RegexOptions.IgnoreCase);
        }

        public static FontFamily GetOtherFont(string @default) => Fonts[UseOtherFont ? OtherFontKey : @default];

        public void CheckKey(bool empty, ref string dummy)
        {
            var match = OtherFonts.Match(dummy);

            UseOtherFont = !empty && match.Success;
            if (UseOtherFont)
            {
                var group = match.Groups[1];
                OtherFontKey = group.Value;
                MakeMeme.CaptureOut(group, ref dummy);
            }
        }
    }
}