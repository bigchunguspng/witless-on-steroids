using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ColorHelper;
using SixLabors.Fonts;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Text;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;

namespace Witlesss.Services.Memes
{
    public class MemeGenerator
    {
        public static bool WrapText = true, UseItalic, ColorText, ForceImpact;
        public static int FontMultiplier = 10;
        public static bool UseCustomBg;
        public static Color   CustomBg;
        public static readonly ExtraFonts ExtraFonts = new("meme", exclude: "ft");
        
        private int _w, _h, _marginBottomText, _margin, _startingSize, _size;
        private Pen _outline;
        private readonly FontFamily  _impact = SystemFonts.Get("Impact");
        private readonly SolidBrush   _white = new(Color.White);
        //private readonly StringFormat _upper = new() { Alignment = Center, Trimming = Word };
        //private readonly StringFormat _lower = new() { Alignment = Center, Trimming = Word, LineAlignment = Far};
        private readonly Dictionary<bool, Func<SolidBrush>> _brushes;

        public MemeGenerator() => _brushes = new Dictionary<bool, Func<SolidBrush>>
        {
            { true, RandomColor },
            { false, WhiteColor }
        };

        public void SetUp(Size size)
        {
            _w = size.Width;
            _h = size.Height;
            
            var minSide = (int)Math.Min(_w, 1.5 * _h);
            _startingSize = Math.Max(minSide * FontMultiplier / 120, 12);
            
            _marginBottomText = _h / 3 * 2;
            _margin = Math.Min(_h / 72, 10);
        }

        public string MakeImpactMeme(string path, DgText text)
        {
            return ImageSaver.SaveImage(DrawCaption(text, GetImage(path)), PngJpg.Replace(path, "-M.jpg"));
        }

        public string BakeCaption(DgText text)
        {
            var caption = DrawCaption(text, new Image<Rgba32>(_w, _h));
            return ImageSaver.SaveImageTemp(caption);
        }

        private Image DrawCaption(DgText text, Image<Rgba32> image)
        {
            var back = Witlesss.Memes.Sticker ? new Image<Rgba32>(image.Width, image.Height) : image;

            //graphics.InterpolationMode = InterpolationMode.Bilinear;

            if (Witlesss.Memes.Sticker)
            {
                back.Mutate(x => x.Fill(UseCustomBg ? CustomBg : Color.Black).DrawImage(image, opacity: 1));
            }

            //graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var width  = _w - 2 * _margin;
            var height = _h / 3 - _margin;

            AddText(text.A, _startingSize, back, /*_upper, */new Rectangle(_w / 2,      _margin, width, height));
            AddText(text.B, _startingSize, back, /*_lower, */new Rectangle(_w / 2, _h - _margin, width, height));

            return back;
        }

        private void AddText(string text, int size, Image<Rgba32> img, Rectangle rect)
        {
            if (string.IsNullOrEmpty(text)) return;

            text = EmojiTool.RemoveEmoji(text);
            text = text.TrimStart('\n');

            // adjust font size
            //var maxLines = text.Count(c => c == '\n') + 1;
            var s = size * 0.75f;
            //var r = rect.Size with { Height = rect.Size.Height * 3 };
            /*var go = true;
            while (go)
            {
                var ms = img.MeasureString(text, SelectFont(s), r, f, out _, out var lines);
                go = ms.Height > rect.Size.Height && s > 2 || !WrapText && lines > maxLines;
                s *= go ? lines > 2 ? 0.8f : 0.9f : 1;
            }*/
            size = (int)(s / 0.75f);
            _size = size;

            // write
            var options = new RichTextOptions(SelectFont(s))
            {
                Dpi = 96,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = rect.Y == _margin ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                Origin = rect.Location,
                WrappingLength = _w - 2 * _margin,
            };
            var ow = OutlineWidth;
            var penOptions = new PenOptions(Color.Black, ow)
            {
                JointStyle = GetRandomMemeber<JointStyle>(),
                EndCapStyle = GetRandomMemeber<EndCapStyle>()
            };
            img.Mutate(x => x.DrawText(options, text, new SolidPen(penOptions)));
            options.Origin += new Vector2(ow / 2F, ow / 3F);
            img.Mutate(x => x.DrawText(options, text, new SolidBrush(Color.White)));

            // todo implement outline using moving kernel
            /*using var path = new GraphicsPath();
            path.AddString(text, CaptionFont, (int)CaptionStyle, size, rect, f);
            for (var i = OutlineWidth; i > 0; i--)
            {
                _outline = new Pen(Color.FromArgb(128, 0, 0, 0), i);
                _outline.LineJoin = LineJoin.Round;
                img.DrawPath(_outline, path);
                _outline.Dispose();
            }
            img.FillPath(Brush, path);*/
        }

        private int OutlineWidth => (int)Math.Round(_size / 7D);
        private SolidBrush Brush => _brushes[ColorText].Invoke();

        private Font SelectFont(float size) => new(CaptionFont, size, CaptionStyle);
        private FontFamily CaptionFont => ForceImpact
            ? _impact
            : ExtraFonts.UseOtherFont
                ? ExtraFonts.GetOtherFont("rg")
                : _impact;
        private FontStyle CaptionStyle => UseItalic
            ? FontStyle.Italic | FontStyle.Bold
            : ExtraFonts.OtherFontKey is "rg" or "cb"
                ? FontStyle.Bold
                : FontStyle.Regular;

        private Image<Rgba32> GetImage(string path)
        {
            var info = Image.Identify(path);
            var size = info.Width < 200 ? new Size(new Size(200, info.Height * 200 / info.Width)) : info.Size;

            SetUp(size);

            var image = Image.Load<Rgba32>(path);
            if (size != info.Size)
            {
                image.Mutate(x => x.Resize(size));
            }

            //image.Mutate(x => x.Dither(new OrderedDither((uint)d)));
            return image;
        }


        private SolidBrush WhiteColor() => _white;
        
        private SolidBrush RandomColor()
        {
            var h = Extension.Random.Next(360);
            var s = (byte)Extension.Random.Next(byte.MaxValue);
            var v = (byte)Extension.Random.Next(byte.MaxValue);

            /*var o = Math.Min(OutlineWidth,       6);
            var x = Math.Min(Math.Abs(240 - h), 60);

            s = s * (0.75 + x / 240D);  // <-- removes dark blue
            s = s * (0.25 + 0.125 * o); // <-- makes small text brighter

            v = 1 - 0.3 * v * Math.Sqrt(s);*/

            var c = ColorConverter.HslToRgb(new HSL(h, s, v));
            return new SolidBrush(new Color(new Rgb24(c.R, c.G, c.B)));
        }
    }
}