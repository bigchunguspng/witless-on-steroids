using System;
using System.Collections.Generic;
using System.Linq;
using ColorHelper;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Backrooms;

namespace Witlesss.Services.Memes
{
    public class MemeGenerator
    {
        public static bool WrapText = true, UseItalic, ColorText, ForceImpact;
        public static int FontMultiplier = 10;
        public static bool UseCustomBg;
        public static Color   CustomBg;
        public static readonly ExtraFonts ExtraFonts = new("meme", exclude: "ft");

        private Size _meme;
        private int _w, _h, _margin, _startingSize, _size;
        private Pen _outline;
        private readonly FontFamily  _impact = SystemFonts.Get("Impact");
        private readonly SolidBrush   _white = new(Color.White);
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

            _margin = Math.Min(_h / 72, 10);

            var minSide = (int)Math.Min(_w, 1.5 * _h);
            _startingSize = Math.Max(minSide * FontMultiplier / 120, 12);
        }

        public string MakeMeme(string path, DgText text)
        {
            var sw = Helpers.GetStartedStopwatch();
            var (size, info) = GetImageSize(path);
            SetUp(size);

            var image = GetImage(path, size, info);
            var caption = DrawCaption(text);
            var meme = Combine(image, caption);

            var result = ImageSaver.SaveImage(meme, PngJpg.Replace(path, "-M.jpg"));
            sw.Log("MakeMeme");
            return result;
        }

        public string MakeCaption(DgText text) => ImageSaver.SaveImageTemp(DrawCaption(text));

        private Image<Rgba32> DrawCaption(DgText text)
        {
            var background = new Image<Rgba32>(_w, _h);

            var width  = _w - 2 * _margin;
            var height = _h / 3 - _margin;

            AddText(background, /*_upper, */text.A, _startingSize, new Rectangle(_w / 2,      _margin, width, height));
            AddText(background, /*_lower, */text.B, _startingSize, new Rectangle(_w / 2, _h - _margin, width, height));

            var image = DrawShadow(background);
            background.Dispose();
            return image;
        }

        private void AddText(Image<Rgba32> background, string text, int size, Rectangle rect)
        {
            if (string.IsNullOrEmpty(text)) return;

            text = EmojiTool.RemoveEmoji(text);
            text = text.TrimStart('\n');

            // adjust font size
            var maxLines = text.Count(c => c == '\n') + 1;
            var s = (float)size;
            var go = true;
            while (go)
            {
                // todo replace with more efficient algorithm (or not, it takes ~ 1 millisecond per loop iter)
                // ok, then replace with algorithm that gives more equal text distribution

                var sw = Helpers.GetStartedStopwatch();
                var height = TextMeasuringHelpers.MeasureTextHeight(text, DefaultTextOptions(s, rect), out var lines);
                sw.Log("TextMeasuringHelpers.MeasureTextHeight");
                go = height > rect.Size.Height && s > 5 || WrapText == false && lines > maxLines;
                s *= go ? lines > 2 ? 0.8f : 0.9f : 1;
            }

            _size = (int)s;

            // write
            var options = DefaultTextOptions(s, rect);
            /*var ow = OutlineWidth;
            var penOptions = new PenOptions(Color.Black, ow)
            {
                JointStyle = GetRandomMemeber<JointStyle>(),
                EndCapStyle = GetRandomMemeber<EndCapStyle>()
            };
            background.Mutate(x => x.DrawText(options, text, new SolidPen(penOptions)));
            options.Origin += new Vector2(ow / 2F, ow / 3F);*/
            background.Mutate(x => x.DrawText(options, text, new SolidBrush(Color.White)));

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

        private Image<Rgba32> Combine(Image<Rgba32> image, Image<Rgba32> caption)
        {
            image.Mutate(x => x.DrawImage(caption, opacity: 1));
            return image;
        }

        private RichTextOptions DefaultTextOptions(float fontSize, Rectangle rect) => new(SelectFont(fontSize))
        {
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = rect.Y == _margin ? VerticalAlignment.Top : VerticalAlignment.Bottom,
            Origin = rect.Location,
            WrappingLength = _w - 2 * _margin
        };

        private int GetOutlineWidth() => (int)Math.Round(_size / 15D);
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

        private (Size size, ImageInfo info) GetImageSize(string path)
        {
            var info = Image.Identify(path);
            return (info.Size.EnureIsWideEnough(), info);
        }

        private Image<Rgba32> GetImage(string path, Size size, ImageInfo info)
        {
            var image = Image.Load<Rgba32>(path);
            if (size != info.Size)
            {
                image.Mutate(x => x.Resize(size));
            }

            if (Witlesss.Memes.Sticker /* && not transparent */)
            {
                var color = UseCustomBg ? CustomBg : Color.Black;
                var background = new Image<Rgba32>(image.Width, image.Height, color);
                background.Mutate(x => x.DrawImage(image, opacity: 1));
                image.Dispose();
                return background;
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

            var rgb = ColorConverter.HslToRgb(new HSL(h, s, v));
            return new SolidBrush(rgb.ToRgb24());
        }

        // SHADOW (THE HEDGEHOG THE ULTIMATE LIFE FORM)

        private Image<Rgba32> DrawShadow(Image<Rgba32> image)
        {
            var shadowRealm = new Image<Rgba32>(image.Width, image.Height);

            var w = GetOutlineWidth();

            var sw = Helpers.GetStartedStopwatch();
            for (var y = 0; y < image.Height; y++)
            for (var x = 0; x < image.Width;  x++)
            {
                var textA = image[x, y].A;
                if (textA == 0) continue;

                for (var ky = -w; ky <= w; ky++)
                for (var kx = -w; kx <= w; kx++)
                {
                    var sx = x + kx;
                    var sy = y + ky;

                    if (sx < 0 || sx >= image.Width || sy < 0 || sy >= image.Height) continue;

                    var shadowA = shadowRealm[sx, sy].A;
                    if (shadowA == 255) continue;

                    var r = Math.Sqrt(kx * kx + ky * ky);
                    var k = Math.Clamp(1 - (r - w), 0, 1);
                    var a = (shadowA + k * textA).RoundInt().ClampByte();
                    shadowRealm[sx, sy] = new Rgba32(0, 0, 0, a);
                }
            }

            sw.Log("DrawShadow");
            shadowRealm.Mutate(x => x.DrawImage(image, opacity: 1));
            return shadowRealm;
        }
    }
}