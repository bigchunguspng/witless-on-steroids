using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Char;
using static System.Drawing.Drawing2D.CompositingMode;
using static System.Drawing.StringAlignment;
using static System.Drawing.StringTrimming;
using static System.Environment;
using static Witlesss.Extension;
using static Witlesss.Strings;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Witlesss
{
    public class DemotivatorDrawer
    {
        private readonly int _w, _h, _imageW, _imageH;
        private readonly Point _imageXY;
        private readonly Rectangle _frame;
        private readonly TextParameters _upper, _lower;
        
        private static readonly Pen White;
        private static readonly string TEMP = $@"{CurrentDirectory}\{TEMP_FOLDER}";
        private static readonly ImageCodecInfo JpgEncoder = GetEncoder();
        private static readonly EncoderParameters EncoderParameters = new EncoderParameters(1);
        private static readonly Dictionary<int, EncoderParameter> Qualities = new Dictionary<int, EncoderParameter>();
        private static readonly Dictionary<Image, Point> Logos = new Dictionary<Image, Point>();
        private static readonly Regex Ext = new Regex("(.png)|(.jpg)"), Emoji = new Regex(REGEX_EMOJI);
        private static readonly Random R = new Random();
        private static readonly StringFormat[] Formats;

        private static long _jpgQuality = 120;
        private static KeyValuePair<Image, Point> _logo;

        static DemotivatorDrawer()
        {
            White = new Pen(Color.White, 2);
            Formats = new[]
            {
                new StringFormat(StringFormatFlags.NoWrap) {Alignment = Near, Trimming = None},
                new StringFormat(StringFormatFlags.NoWrap) {Alignment = Near, Trimming = EllipsisCharacter}
            };
            LoadLogos($@"{CurrentDirectory}\{WATERMARKS_FOLDER}");
        }

        public DemotivatorDrawer(int width = 720, int height = 720)
        {
            _w = width;
            _h = height;

            var imageMarginT = 50;
            int imageMarginS = width == 1280 ? 144 : 50;
            var imageMarginB = 140;

            _imageW = _w - imageMarginS * 2;
            _imageH = _h - imageMarginT - imageMarginB;

            var space = 5;
            int marginT = imageMarginT - space;
            int marginS = imageMarginS - space;
            int marginB = imageMarginB - space;

            _imageXY = new Point(imageMarginS, imageMarginT);
            _frame = new Rectangle(marginS, marginT, _w - 2 * marginS, _h - marginT - marginB);

            if (width == 1280)
            {
                _upper = TextParameters.LargeText(_h - imageMarginB + 28, _w);
                _lower = TextParameters.LowerText(_h, 0);
            }
            else
            {
                _upper = TextParameters.UpperText(_h - imageMarginB + 18, _w);
                _lower = TextParameters.LowerText(_h - imageMarginB + 84, _w); // + 34 for \n
            }
        }
        
        public string DrawDemotivator(string path, string a, string b)
        {
            var demotivator = MakeFrame(a, b);
            return PasteImage(demotivator, path);
        }

        public string MakeFrame(string a, string b)
        {
            using Image demotivator = new Bitmap(_w, _h);
            using var graphics = Graphics.FromImage(demotivator);

            graphics.CompositingMode = SourceCopy;
            graphics.Clear(Color.Black);
            graphics.DrawRectangle(White, _frame);
            if (_w == 720)
            {
                SetRandomLogo();
                graphics.DrawImage(_logo.Key, _logo.Value);
            }

            graphics.CompositingMode = SourceOver;
            DrawText(a, graphics, DrawTextA, _upper);
            DrawText(b, graphics, DrawTextB, _lower);
            
            return SaveImageTemp(demotivator);

            void DrawTextA(Graphics g) => Draw(g, a, _upper);
            void DrawTextB(Graphics g) => Draw(g, b, _lower);
            
            void Draw(Graphics g, string s, TextParameters p) => g.DrawString(s, p.Font, p.Color, p.Layout, p.Format);
        }

        public string PasteImage(string background, string picture)
        {
            using var demotivator = Image.FromFile(background);
            using var graphics = Graphics.FromImage(demotivator);
            using var image = Resize(Image.FromFile(picture), new Size(_imageW, _imageH));
            
            graphics.CompositingMode = SourceCopy;
            graphics.DrawImage(image, _imageXY);
            
            string output = Ext.Replace(picture, "-D.jpg");
            
            return SaveImage(demotivator, output);
            
            Image Resize(Image img, Size size) => new Bitmap(img, size);
        }

        private void SetRandomLogo() => _logo = Logos.ElementAt(R.Next(Logos.Count));

        private void DrawText(string text, Graphics g, Action<Graphics> drawSimple, TextParameters p)
        {
            var emoji = Regex.Matches(text, REGEX_EMOJI);
            if (emoji.Count > 0)
            {
                DrawTextAndEmoji(g, text, emoji, p);
            }
            else
            {
                drawSimple(g);
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

                    var image = new Bitmap(Image.FromFile(path), p.EmojiSize);
                    graphics.DrawImage(image, x, 0);
                    x += p.EmojiS;
                }
            }
            DoText(texts[^1]);

            void DoText(string s)
            {
                var rest = _w - x;
                var width = (int) Math.Min(graphics.MeasureString(s, p.Font).Width, rest);
                var format = width < rest ? Formats[0] : Formats[1];

                var layout = new RectangleF(x, 0, width, 100);
                graphics.DrawString(s, p.Font, p.Color, layout, format);
                x += width;
            }
            
            var save = SaveImageTemp(textArea);
            var y = (int) p.Layout.Y + m;
            var point = new Point((_w - x) / 2, y);

            g.DrawImage(new Bitmap(Image.FromFile(save)), point);
            
            // mb static emoji bitmaps cache to prevent file exceptions
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
                        
                        var files = Directory.GetFiles($@"{CurrentDirectory}\Emoji", name + "*.png");
                        if (files.Length == 1) file = files[0];
                        else if (files.Length > 1 && cluster.Count > j + 1)
                        {
                            file = files[^1];
                            repeat = true;
                            j++;
                            name = name + "-" + cluster[j];
                        }
                    } while (repeat);

                    if (file != null)
                    {
                        emoji[n].Add(file);
                        i += Path.GetFileName(file).Count(c => c == '-');
                    }
                }
            }

            return emoji;
        }

        private string SaveImage(Image image, string path)
        {
            path = UniquePath(path, ".jpg");
            image.Save(path, JpgEncoder, EncoderParameters);

            return path;
        }
        private string SaveImageTemp(Image image)
        {
            Directory.CreateDirectory(TEMP);
            var path = UniquePath($@"{TEMP}\x.png", ".png");
            image.Save(path);

            return path;
        }
        
        public static void PassQuality(int value)
        {
            if (value == _jpgQuality) return;

            if (!Qualities.ContainsKey(value)) Qualities.Add(value, new EncoderParameter(Encoder.Quality, value));
            EncoderParameters.Param[0] = Qualities[value];
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
    }

    public class TextParameters
    {
        public int Lines, EmojiS;
        public Font Font;
        public SolidBrush Color;
        public RectangleF Layout;
        public StringFormat Format;
        
        public Size EmojiSize => new Size(EmojiS, EmojiS);

        public static TextParameters LargeText(int m, int w) => Construct(LargeFont, 1, 72, m, w);
        public static TextParameters UpperText(int m, int w) => Construct(UpperFont, 1, 54, m, w);
        public static TextParameters LowerText(int m, int w) => Construct(LowerFont, 2, 34, m, w);

        private static Font LargeFont => new Font(DEMOTIVATOR_UPPER_FONT, 48);
        private static Font UpperFont => new Font(DEMOTIVATOR_UPPER_FONT, 36);
        private static Font LowerFont => new Font(DEMOTIVATOR_LOWER_FONT, 18);

        private static TextParameters Construct(Font f, int l, int e, int margin, int width) => new TextParameters
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