using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using Witlesss.Commands;
using static System.Drawing.StringAlignment;
using static System.Drawing.StringTrimming;
using static Witlesss.X.JpegCoder;

namespace Witlesss
{
    public class MemeGenerator
    {
        private int _w, _h, _s, _d;
        private Pen _outline;
        private bool _resize;
        private const int _m = 10;
        private readonly FontFamily    _font = new("Impact");
        private readonly StringFormat _upper = new() { Alignment = Center, Trimming = Word };
        private readonly StringFormat _lower = new() { Alignment = Center, Trimming = Word, LineAlignment = Far};
        private readonly Dictionary<ColorMode, PaintBrush> _brushes;
        private static readonly Regex Ext = new("(.png)|(.jpg)");

        public MemeGenerator() => _brushes = new Dictionary<ColorMode, PaintBrush>
        {
            { ColorMode.Color, new RandomBrush(this) },
            { ColorMode.White, new WhiteBrush() }
        };

        public void SetUp(Size size)
        {
            _w = size.Width;
            _h = size.Height;
            _s = Math.Max((int)Math.Min(_w, 1.5 * _h) / 12, 12);
            _d = _h / 3 * 2;    // upper margin for bottom text
        }

        public int OutlineWidth => (int)Math.Round(_s / 6D);

        public string MakeImpactMeme(string path, DgText text)
        {
            var image = DrawCaption(text, GetImage(path));

            if (_resize) image = CropFix(image);

            return SaveImage(image, Ext.Replace(path, "-M.jpg"));
        }

        public string BakeCaption(DgText text) => SaveImageTemp(DrawCaption(text, new Bitmap(_w, _h)));
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
            path.AddString(text, _font, 0, size, rect, f);
            for (int i = OutlineWidth; i > 0; i--)
            {
                _outline = new Pen(Color.FromArgb(128, 0, 0, 0), i);
                _outline.LineJoin = LineJoin.Round;
                g.DrawPath(_outline, path);
                _outline.Dispose();
            }
            g.FillPath(Brush(), path);
        }

        private SolidBrush Brush() => _brushes[WitlessCommand.Baka.Meme.Dye].Brush;

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
    }
}