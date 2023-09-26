using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Witlesss.Commands;
using static System.Drawing.StringAlignment;
using static System.Drawing.StringTrimming;

namespace Witlesss
{
    public class MemeGenerator
    {
        public static bool WrapText = true, UseCustomBack, UseRoboto, UseItalic;
        public static Color CustomBackground;
        
        private int _w, _h, _s, _d, _m, _size;
        private Pen _outline;
        private readonly FontFamily[] _fonts = new[] { new FontFamily("Impact"), new FontFamily("Roboto") };
        private readonly SolidBrush   _white = new(Color.White);
        private readonly StringFormat _upper = new() { Alignment = Center, Trimming = Word };
        private readonly StringFormat _lower = new() { Alignment = Center, Trimming = Word, LineAlignment = Far};
        private readonly Dictionary<ColorMode, Func<SolidBrush>> _brushes;

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
            _m = Math.Min(_h / 72, 10); // margin
        }

        public string MakeImpactMeme(string path, DgText text)
        {
            return JpegCoder.SaveImage(DrawCaption(text, GetImage(path)), PngJpg.Replace(path, "-M.jpg"));
        }

        public string BakeCaption(DgText text) => JpegCoder.SaveImageTemp(DrawCaption(text, new Bitmap(_w, _h)));
        private Image DrawCaption(DgText text, Image image)
        {
            var back = Memes.Sticker ? new Bitmap(image.Width, image.Height) : image;
            using var graphics = Graphics.FromImage(back);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (Memes.Sticker)
            {
                graphics.Clear(UseCustomBack ? CustomBackground : Color.Black);
                graphics.DrawImage(image, Point.Empty);
            }

            AddText(text.A, _s, graphics, _upper, new Rectangle(_m, _m, _w - 2 * _m, _h / 3 - _m));
            AddText(text.B, _s, graphics, _lower, new Rectangle(_m, _d, _w - 2 * _m, _h / 3 - _m));

            return back;
        }

        private void AddText(string text, int size, Graphics g, StringFormat f, Rectangle rect)
        {
            if (string.IsNullOrEmpty(text)) return;

            text = EmojiTool.RemoveEmoji(text);
            text = text.TrimStart('\n');

            var maxLines = text.Count(c => c == '\n') + 1;

            var s = size * 0.75f;
            var r = rect.Size with { Height = rect.Size.Height * 3 };
            var go = true;
            while (go)
            {
                var ms = g.MeasureString(text, SelectFont(s), r, f, out _, out var lines);
                go = ms.Height > rect.Size.Height && s > 2 || !WrapText && lines > maxLines;
                s *= go ? lines > 2 ? 0.8f : 0.9f : 1;
            }
            size = (int)(s / 0.75f);
            _size = size;
            
            using var path = new GraphicsPath();
            path.AddString(text, CaptionFont, (int)CaptionStyle, size, rect, f);
            for (var i = OutlineWidth; i > 0; i--)
            {
                _outline = new Pen(Color.FromArgb(128, 0, 0, 0), i);
                _outline.LineJoin = LineJoin.Round;
                g.DrawPath(_outline, path);
                _outline.Dispose();
            }
            g.FillPath(Brush, path);
        }

        private int OutlineWidth => (int)Math.Round(_size / 6D);
        private SolidBrush Brush => _brushes[MakeMeme.Dye].Invoke();

        private Font SelectFont(float size) => new(CaptionFont, size, CaptionStyle);
        private FontFamily CaptionFont => _fonts[UseRoboto ? 1 : 0];
        private FontStyle CaptionStyle => UseItalic ? FontStyle.Italic | FontStyle.Bold : UseRoboto ? FontStyle.Bold : FontStyle.Regular;

        private Image GetImage(string path)
        {
            var i = Image.FromFile(path);
            var image = i.Width < 200 ? new Bitmap(i, new Size(200, i.Height * 200 / i.Width)) : new Bitmap(i);

            SetUp(image.Size);

            return image;
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
            var dye = Convert.ToInt32(triangle) % 6;
            var fraction = sextants - triangle;

            value = value * 255;
            var v = Convert.ToInt32(value);
            var p = Convert.ToInt32(value * (1 - saturation));
            var q = Convert.ToInt32(value * (1 - saturation * fraction));
            var t = Convert.ToInt32(value * (1 - saturation * (1 - fraction)));

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