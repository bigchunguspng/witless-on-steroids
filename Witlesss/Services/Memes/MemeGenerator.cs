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
        public static bool WrapText = true, ColorText, ForceImpact;
        public static int FontMultiplier = 10;
        public static bool UseCustomBg;
        public static Color   CustomBg;
        public static readonly ExtraFonts ExtraFonts = new("meme");

        private int _w, _h, _margin, _startingSize, _size;
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

            var s1 = AddText(background, text.A, _startingSize, new Rectangle(_w / 2,      _margin, width, height));
            var s2 = AddText(background, text.B, _startingSize, new Rectangle(_w / 2, _h - _margin, width, height));

            var image = DrawShadow(background, s1, s2);
            background.Dispose();
            return image;
        }

        private Size? AddText(Image<Rgba32> background, string text, int size, Rectangle rect)
        {
            if (string.IsNullOrEmpty(text)) return null;

            text = EmojiTool.RemoveEmoji(text);
            text = text.TrimStart('\n');

            // adjust font size
            var maxLines = text.Count(c => c == '\n') + 1;
            var s = (float)size;
            var go = true;
            var textSize = Size.Empty;
            while (go)
            {
                // todo replace with more efficient algorithm (or not, it takes ~ 1 millisecond per loop iter)
                // ok, then replace with algorithm that gives more equal text distribution

                var sw = Helpers.GetStartedStopwatch();
                textSize = TextMeasuringHelpers.MeasureTextSize(text, DefaultTextOptions(s, rect), out var lines);
                sw.Log("TextMeasuringHelpers.MeasureTextHeight");
                go = textSize.Height > rect.Size.Height && s > 5 || WrapText == false && lines > maxLines;
                s *= go ? lines > 2 ? 0.8f : 0.9f : 1;
            }

            _size = (int)s;

            // write
            var options = DefaultTextOptions(s, rect);
            background.Mutate(x => x.DrawText(options, text, new SolidBrush(Color.White)));

            var space = _size; // todo make all fonts same size >> space = _size / 2;
            return new Size(Math.Min(_w, textSize.Width + space), textSize.Height + space);
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

        private SolidBrush Brush => _brushes[ColorText].Invoke();

        private Font SelectFont(float size) => new(CaptionFont, size, ExtraFonts.GetFontStyle());

        private FontFamily CaptionFont => ExtraFonts.GetFontFamily("im", forceDefault: ForceImpact);

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

        private Image<Rgba32> DrawShadow(Image<Rgba32> image, Size? top, Size? bottom)
        {
            var shadowRealm = new Image<Rgba32>(image.Width, image.Height);

            var nokia = CaptionFont.Name.Contains("Nokia");

            var w = _size / (nokia ? 12D : 15D);
            var w2 = (int)Math.Ceiling(w) + 2;

            Func<int, int, double, double> func = nokia ? SquareShadow : RoundShadow;

            var sw = Helpers.GetStartedStopwatch();

            if (top.HasValue)
            {
                var x = (_w - top.Value.Width) / 2;
                ShadowImagePart(new Rectangle(new Point(x, 00), top.Value));
            }

            if (bottom.HasValue)
            {
                var x2 = (_w - bottom.Value.Width) / 2;
                var y2 =  _h - bottom.Value.Height;
                ShadowImagePart(new Rectangle(new Point(x2, y2), bottom.Value));
            }

            sw.Log("DrawShadow");
            shadowRealm.Mutate(x => x.DrawImage(image, opacity: 1));
            return shadowRealm;

            void ShadowImagePart(Rectangle rectangle)
            {
                for (var y = rectangle.Y; y < rectangle.Bottom; y++)
                for (var x = rectangle.X; x < rectangle.Right; x++)
                {
                    var textA = image[x, y].A;
                    if (textA == 0) continue;

                    for (var ky = y - w2; ky <= y + w2; ky++)
                    for (var kx = x - w2; kx <= x + w2; kx++)
                    {
                        var sx = kx - x;
                        var sy = ky - y;

                        if (kx < 0 || kx >= image.Width || ky < 0 || ky >= image.Height) continue; // outside image

                        var shadowA = shadowRealm[kx, ky].A;
                        if (shadowA == 255) continue; // already shadowed

                        var k = func(sx, sy, w);
                        if (k == 0) continue; // too far from text

                        var a = Math.Max(shadowA, k * textA).RoundInt().ClampByte();
                        shadowRealm[kx, ky] = new Rgba32(0, 0, 0, a);
                    }
                }
            }
        }

        private double RoundShadow(int kx, int ky, double w)
        {
            var r = Math.Sqrt(kx * kx + ky * ky);
            return Math.Clamp(1 - 2 * (r - w), 0, 1);
        }

        private double SquareShadow(int kx, int ky, double w)
        {
            var x = Math.Abs(kx);
            var y = Math.Abs(ky);
            var b = x > 0 && x < w && y > 0 && y < w;
            return b ? 1 : 0;
        }
    }
}