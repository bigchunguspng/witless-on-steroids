using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using Witlesss.Commands;
using static System.Drawing.StringAlignment;
using static System.Drawing.StringTrimming;

namespace Witlesss
{
    public class MemeGenerator
    {
        private int _w, _h, _s, _d;
        private Pen _outline;
        private bool _resize;
        private const int _m = 10;
        private readonly FontFamily    _font = new("Impact");
        private readonly SolidBrush   _white = new(Color.White);
        private readonly StringFormat _upper = new() { Alignment = Center, Trimming = Word };
        private readonly StringFormat _lower = new() { Alignment = Center, Trimming = Word, LineAlignment = Far};
        private readonly Dictionary<ColorMode, Func<SolidBrush>> _brushes;
        private static readonly Regex Ext = new("(.png)|(.jpg)");

        public MemeGenerator() => _brushes = new Dictionary<ColorMode, Func<SolidBrush>>
        {
            { ColorMode.Color, RandomColor },
            { ColorMode.White, WhiteColor }
        };

        public void SetUp(Size size)
        {
            _w = size.Width;
            _h = size.Height;
            _s = Math.Max((int)Math.Min(_w, 1.5 * _h) / 12, 12);
            _d = _h / 3 * 2;    // upper margin for bottom text
        }

        public string MakeImpactMeme(string path, DgText text)
        {
            var image = DrawCaption(text, GetImage(path));

            if (_resize) image = CropFix(image);

            return JpegCoder.SaveImage(image, Ext.Replace(path, "-M.jpg"));
        }

        public string BakeCaption(DgText text) => JpegCoder.SaveImageTemp(DrawCaption(text, new Bitmap(_w, _h)));
        private Image DrawCaption(DgText text, Image image)
        {
            using var graphics = Graphics.FromImage(image);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            AddText(text.A, _s, graphics, _upper, new Rectangle(_m, _m, _w - 2 * _m, _h / 3 - _m));
            AddText(text.B, _s, graphics, _lower, new Rectangle(_m, _d, _w - 2 * _m, _h / 3 - _m));

            return image;
        }

        private void AddText(string text, int size, Graphics g, StringFormat f, Rectangle rect)
        {
            if (string.IsNullOrEmpty(text)) return;

            using var path = new GraphicsPath();
            path.AddString(RemoveEmoji(text), _font, 0, size, rect, f);
            for (int i = OutlineWidth; i > 0; i--)
            {
                _outline = new Pen(Color.FromArgb(128, 0, 0, 0), i);
                _outline.LineJoin = LineJoin.Round;
                g.DrawPath(_outline, path);
                _outline.Dispose();
            }
            g.FillPath(Brush(), path);
        }

        private static string RemoveEmoji(string text)
        {
            var matches = Regex.Matches(text, REGEX_EMOJI);
            if (matches.Count == 0) return text;

            var emoji = DemotivatorDrawer.GetEmojiPngs(matches);
            int m = 0;
            foreach (var cluster in emoji)
            {
                var cleared = cluster.Select(path => path.EndsWith(".png") ? "" : path);
                text = text.Replace(matches[m++].Value, string.Join("", cleared));
            }

            return text;
        }

        private int OutlineWidth => (int)Math.Round(_s / 6D);

        private SolidBrush Brush() => _brushes[MakeMeme.Dye].Invoke();

        private Image GetImage(string path)
        {
            _resize = false;
            var image = new Bitmap(Image.FromFile(path));
            if (image.Width < 200)
            {
                _resize = true;
                image = new Bitmap(Image.FromFile(path), new Size(200, image.Height * 200 / image.Width));
            }

            SetUp(image.Size);

            return image;
        }

        private Image CropFix(Image image)
        {
            var crop = new Rectangle(0, 0, image.Width - 1, image.Height - 1);
            var bitmap = new Bitmap(image);
            return bitmap.Clone(crop, bitmap.PixelFormat);
        }

        private SolidBrush WhiteColor() => _white;
        
        private SolidBrush RandomColor()
        {
            var h = Extension.Random.Next(360);
            var s = Extension.Random.NextDouble();
            var v = Extension.Random.NextDouble();

            var o = Math.Min(OutlineWidth,       6);
            var x = Math.Min(Math.Abs(240 - h), 60);

            s = s * (0.75 + x / 240D);  // <-- removes dark blue
            s = s * (0.25 + 0.125 * o); // <-- makes small text brighter

            v = 1 - 0.3 * v * Math.Sqrt(s);

            return new SolidBrush(ColorFromHSV(h, s, v));
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            var sextants = hue / 60;
            var triangle = Math.Floor(sextants);
            int dye = Convert.ToInt32(triangle) % 6;
            var fraction = sextants - triangle;

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - saturation * fraction));
            int t = Convert.ToInt32(value * (1 - saturation * (1 - fraction)));

            return dye switch
            {
                0 => Color.FromArgb(v, t, p),
                1 => Color.FromArgb(q, v, p),
                2 => Color.FromArgb(p, v, t),
                3 => Color.FromArgb(p, q, v),
                4 => Color.FromArgb(t, p, v),
                _ => Color.FromArgb(v, p, q)
            };
        }
    }
}