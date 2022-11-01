using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Char;
using static System.Drawing.Drawing2D.CompositingMode;
using static System.Drawing.StringAlignment;
using static System.Drawing.StringTrimming;
using static Witlesss.Extension;
using static Witlesss.Strings;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Witlesss
{
    public class DemotivatorDrawer
    {
        private readonly int _w, _h;
        private readonly Size _size;
        private readonly Point _imageXY;
        private readonly Rectangle _frame;
        private readonly DrawableText _textA = new(), _textB = new();
        
        private static readonly Pen White;
        private static readonly ImageCodecInfo JpgEncoder = GetEncoder();
        private static readonly EncoderParameters EncoderParameters = new(1);
        private static readonly Dictionary<Image, Point> Logos = new();
        private static readonly Regex Ext = new("(.png)|(.jpg)"), Emoji = new(REGEX_EMOJI);
        private static readonly Random R = new();
        private static readonly StringFormat[] Formats;

        private static int _temp;
        private static long _jpegQuality = 120;

        static DemotivatorDrawer()
        {
            White = new Pen(Color.White, 2);
            Formats = new[]
            {
                new StringFormat(StringFormatFlags.NoWrap) {Alignment = Near, Trimming = None},
                new StringFormat(StringFormatFlags.NoWrap) {Alignment = Near, Trimming = EllipsisCharacter}
            };
            LoadLogos(WATERMARKS_FOLDER);
        }

        public DemotivatorDrawer(int width = 720, int height = 720)
        {
            _w = width;
            _h = height;

            var imageMarginT = 50;
            int imageMarginS = width == 1280 ? 144 : 50;
            var imageMarginB = 140;

            var imageW = _w - imageMarginS * 2;
            var imageH = _h - imageMarginT - imageMarginB;
            
            _size = new Size(imageW, imageH);

            var space = 5;
            int marginT = imageMarginT - space;
            int marginS = imageMarginS - space;
            int marginB = imageMarginB - space;

            _imageXY = new Point(imageMarginS, imageMarginT);
            _frame = new Rectangle(marginS, marginT, _w - 2 * marginS, _h - marginT - marginB);

            if (width == 1280)
            {
                _textA.P = TextParameters.LargeText(_h - imageMarginB + 28, _w);
                _textB.P = TextParameters.LowerText(_h, 0);
            }
            else
            {
                _textA.P = TextParameters.UpperText(_h - imageMarginB + 18, _w);
                _textB.P = TextParameters.LowerText(_h - imageMarginB + 84, _w);
            }
        }
        
        public Size Size => _size;
        public Point Pic => _imageXY;

        public string DrawDemotivator(string path, string a, string b) => PasteImage(DrawFrame(a, b), path);

        public string MakeFrame(string a, string b) => SaveImageTemp(DrawFrame(a, b));
        private Image DrawFrame(string a, string b)
        {
            Image background = new Bitmap(_w, _h);
            using var graphics = Graphics.FromImage(background);

            graphics.CompositingMode = SourceCopy;
            graphics.Clear(Color.Black);
            graphics.DrawRectangle(White, _frame);
            if (_w == 720)
            {
                var logo = PickRandomLogo();
                graphics.DrawImage(logo.Key, logo.Value);
            }

            graphics.CompositingMode = SourceOver;

            _textA.Pass(graphics, a);
            _textB.Pass(graphics, b);

            DrawText(_textA);
            DrawText(_textB);
            
            return background;
        }

        private string PasteImage(Image background, string picture)
        {
            using var graphics = Graphics.FromImage(background);
            using var image = new Bitmap(Image.FromFile(picture), _size);
            
            graphics.CompositingMode = SourceCopy;
            graphics.DrawImage(image, _imageXY);
            
            string output = Ext.Replace(picture, "-D.jpg");
            
            return SaveImage(background, output);
        }

        private KeyValuePair<Image, Point> PickRandomLogo() => Logos.ElementAt(R.Next(Logos.Count));

        private void DrawText(DrawableText x)
        {
            var emoji = Regex.Matches(x.S, REGEX_EMOJI);
            if (emoji.Count > 0)
            {
                DrawTextAndEmoji(x.G, x.S, emoji, x.P);
            }
            else
            {
                x.G.DrawString(x.S, x.P.Font, x.P.Color, x.P.Layout, x.P.Format);
            }
        }

        private void DrawTextAndEmoji(Graphics g, string text, IList<Match> matches, TextParameters p, int m = 0)
        {
            if (p.Lines > 1 && text.Contains('\n'))
            {
                var s = text.Split('\n');
                var index1 = s[0].Length;
                var index2 = s[0].Length + 1 + s[1].Length;
                var matchesA = matches.Where(u => u.Index < index1).ToArray();
                var matchesB = matches.Where(u => u.Index > index1 && u.Index < index2).ToArray();
                DrawTextAndEmoji(g, s[1], matchesB, p, 34);
                text = s[0];
                matches = matchesA;
            }
            
            var texts = Emoji.Replace(text, "\t").Split('\t');
            var emoji = GetEmojiPngs(matches);

            using var textArea = new Bitmap(_w, 100);
            using var graphics = Graphics.FromImage(textArea);

            graphics.CompositingMode = SourceOver;

            var x = 0;
            for (int i = 0; i < emoji.Count; i++)
            {
                DoText(texts[i]);

                foreach (string path in emoji[i])
                {
                    if (p.EmojiS + x > _w) break;

                    if (path.EndsWith(".png"))
                    {
                        var image = new Bitmap(Image.FromFile(path), p.EmojiSize);
                        graphics.DrawImage(image, x, 0);
                        x += p.EmojiS;
                    }
                    else DoText(path);
                }
            }
            DoText(texts[^1]);

            g.DrawImage(textArea, new Point((_w - x) / 2, (int) p.Layout.Y + m));

            void DoText(string s)
            {
                var rest = _w - x;
                var width = (int) Math.Min(graphics.MeasureString(s, p.Font).Width, rest);
                var format = width < rest ? Formats[0] : Formats[1];

                var layout = new RectangleF(x, 0, width, 100);
                graphics.DrawString(s, p.Font, p.Color, layout, format);
                x += width;
            }
        }

        private List<List<string>> GetEmojiPngs(IList<Match> matches)
        {
            var emoji = new List<List<string>>(matches.Count);
            
            for (var n = 0; n < matches.Count; n++)
            {
                var match = matches[n];
                var xd = match.Value;
                var cluster = new List<string>(xd.Length / 2);
                for (int i = 0; i < xd.Length; i += IsSurrogatePair(xd, i) ? 2 : 1)
                {
                    var c = ConvertToUtf32(xd, i).ToString("x4");
                    cluster.Add(c);
                }

                emoji.Add(new List<string>(cluster.Count));

                for (int i = 0; i < cluster.Count; i++)
                {
                    int j = i;
                    var name = cluster[i];
                    string file = null;
                    bool repeat;
                    do
                    {
                        repeat = false;
                        
                        var files = Directory.GetFiles(EMOJI_FOLDER, name + "*.png");
                        if (files.Length == 1) file = files[0];
                        else if (files.Length > 1)
                        {
                            file = files[^1];
                            if (cluster.Count > j + 1)
                            {
                                repeat = true;
                                j++;
                                name = name + "-" + cluster[j];
                            }
                        }
                    } while (repeat);

                    if (file != null)
                    {
                        emoji[n].Add(file);
                        var s = Path.GetFileNameWithoutExtension(file);
                        var split = s.Split('-');
                        for (int k = 1; k < split.Length && i + 1 < cluster.Count; k++)
                        {
                            if (split[k] == cluster[i + 1]) i++;
                        }
                    }
                    else
                    {
                        var character = ConvertFromUtf32(int.Parse(name, NumberStyles.HexNumber));
                        if (Regex.IsMatch(character, @"[\u231a-\u303d]")) emoji[n].Add(character);
                    }
                }
            }

            return emoji;
        }

        private string SaveImage(Image image, string path)
        {
            path = UniquePath(path, ".jpg");
            image.Save(path, JpgEncoder, EncoderParameters);
            image.Dispose();

            return path;
        }
        private string SaveImageTemp(Image image)
        {
            Directory.CreateDirectory(TEMP_FOLDER);
            var path = UniquePath($@"{TEMP_FOLDER}\x_{_temp++}.png", ".png");
            image.Save(path);
            image.Dispose();

            return path;
        }
        
        public static void PassQuality(int value)
        {
            if (_jpegQuality != value)
            {
                _jpegQuality = value;
                EncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, value);
            }
        }

        private static ImageCodecInfo GetEncoder() => ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
        
        private static void LoadLogos(string path)
        {
            var files = GetFilesInfo(path);
            foreach (var file in files)
            {
                var coords = file.Name.Replace(file.Extension, "").Split(' ');
                if (int.TryParse(coords[0], out int x) && int.TryParse(coords[^1], out int y))
                    Logos.Add(Image.FromFile(file.FullName), new Point(x, y));
            }
        }

        private class DrawableText
        {
            public Graphics G;
            public string S;
            public TextParameters P;

            public void Pass(Graphics g, string s) { G = g; S = s; }
        }
    }

    public class TextParameters
    {
        public int Lines, EmojiS;
        public Font Font;
        public SolidBrush Color;
        public RectangleF Layout;
        public StringFormat Format;
        
        public Size EmojiSize => new(EmojiS, EmojiS);

        public static TextParameters LargeText(int m, int w) => Construct(LargeFont, 1, 72, m, w);
        public static TextParameters UpperText(int m, int w) => Construct(UpperFont, 1, 54, m, w);
        public static TextParameters LowerText(int m, int w) => Construct(LowerFont, 2, 34, m, w);

        private static Font LargeFont => new(DEMOTIVATOR_UPPER_FONT, 48);
        private static Font UpperFont => new(DEMOTIVATOR_UPPER_FONT, 36);
        private static Font LowerFont => new(DEMOTIVATOR_LOWER_FONT, 18);

        private static TextParameters Construct(Font f, int l, int e, int margin, int width) => new()
        {
            Font   = f,
            Lines  = l,
            EmojiS = e,
            Color  = new SolidBrush(System.Drawing.Color.White),
            Layout = new RectangleF(0, margin, width, 100),
            Format = new StringFormat(StringFormatFlags.NoWrap) {Alignment = Center, Trimming = Word}
        };
    }
}