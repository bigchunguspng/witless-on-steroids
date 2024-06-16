using System;
using System.Linq;
using ColorHelper;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Backrooms.Types;

namespace Witlesss.Services.Memes
{
    public class MemeGenerator
    {
        public static bool WrapText = true, ColorText;
        public static int FontMultiplier = 10, ShadowOpacity = 100;
        public static CustomColorOption CustomColorOption;

        public static readonly ExtraFonts ExtraFonts = new("meme");

        private int _w, _h, _margin, _centerX;
        private Size _captionArea;
        private float _startingFontSize;
        private readonly SolidBrush _white = new(Color.White);

        private readonly DrawingOptions _textDrawingOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 16 }
        };

        public void SetUp(Size size)
        {
            _w = size.Width;
            _h = size.Height;

            _centerX = _w / 2;
            _margin = Math.Min(_h / 72, 10);

            var minSide = (int)Math.Min(_w, 1.5 * _h);
            _startingFontSize = Math.Max(minSide * FontMultiplier * ExtraFonts.GetSizeMultiplier() / 120, 12);
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
            var canvas = new Image<Rgba32>(_w, _h);

            _captionArea = new Size(_w - 2 * _margin, _h / 3 - _margin);

            var s1 = AddText(canvas, text.A, _startingFontSize,      _margin, out var lines1, out var height1);
            var s2 = AddText(canvas, text.B, _startingFontSize, _h - _margin, out var lines2, out var height2);

            var avgTextHeight = (height1 + height2) / (lines1 + lines2);
            return ShadowOpacity > 0 ? DrawShadow(canvas, s1, s2, avgTextHeight) : canvas;
        }

        private Size? AddText
        (
            Image<Rgba32> background, string text,
            float size, int y,
            out int lines, out float textHeight
        )
        {
            lines = 0;
            textHeight = 0F;

            if (string.IsNullOrEmpty(text)) return null;

            text = EmojiTool.RemoveEmoji(text);
            text = text.TrimStart('\n');

            // adjust font size
            var maxLines = text.Count(c => c == '\n') + 1;
            var go = true;
            var textSize = Size.Empty;
            RichTextOptions options = null!;
            while (go)
            {
                // todo replace with more efficient algorithm (or not, it takes ~ 1 millisecond per loop iter)
                // ok, then replace with algorithm that gives more equal text distribution

                var sw = Helpers.GetStartedStopwatch();
                options = GetDefaultTextOptions(size, y);
                textSize = TextMeasuring.MeasureTextSize(text, options, out lines).CeilingInt();
                sw.Log("TextMeasuringHelpers.MeasureTextHeight");
                go = textSize.Height > _captionArea.Height && size > 5 || WrapText == false && lines > maxLines;
                size *= go ? lines > 2 ? 0.8f : 0.9f : 1;
            }

            // write
            background.Mutate(x => x.DrawText(_textDrawingOptions, options, text, GetBrush(), pen: null));

            textHeight = textSize.Height;
            var space = (textSize.Height / 2F / lines).RoundInt();
            return new Size(Math.Min(_w, textSize.Width + 2 * space), textSize.Height + space);
        }

        private Image<Rgba32> Combine(Image<Rgba32> image, Image<Rgba32> caption)
        {
            image.Mutate(x => x.DrawImage(caption, opacity: 1));
            return image;
        }


        // TEXT OPTIONS

        private RichTextOptions GetDefaultTextOptions(float fontSize, int y) => new(GetFont(fontSize))
        {
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = y == _margin ? VerticalAlignment.Top : VerticalAlignment.Bottom,
            Origin = new Point(_centerX, y),
            WrappingLength = _captionArea.Width,
            LineSpacing = ExtraFonts.GetLineSpacing(),
            FallbackFontFamilies = ExtraFonts.FallbackFamilies
        };

        private Font GetFont(float size) => ExtraFonts.GetFont("im", size);

        private FontFamily GetFontFamily() => ExtraFonts.GetFontFamily("im");

        private SolidBrush GetBrush() => ColorText ? RandomColor() : _white;

        private SolidBrush RandomColor()
        {
            var h =       Random.Shared.Next(360);
            var s = (byte)Random.Shared.Next(50, 100);
            var l = (byte)Random.Shared.Next(50,  95);

            var rgb = ColorConverter.HslToRgb(new HSL(h, s, l));
            return new SolidBrush(rgb.ToRgb24());
        }


        // IMAGE

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
                var color = CustomColorOption.GetColor() ?? Color.Black;
                var background = new Image<Rgba32>(image.Width, image.Height, color);
                background.Mutate(x => x.DrawImage(image, opacity: 1));
                image.Dispose();
                return background;
            }

            //image.Mutate(x => x.Dither(new OrderedDither((uint)d)));
            return image;
        }


        // SHADOW (THE HEDGEHOG THE ULTIMATE LIFE FORM)

        private Image<Rgba32> DrawShadow(Image<Rgba32> image, Size? top, Size? bottom, float avgTextHeight)
        {
            var shadowRealm = new Image<Rgba32>(image.Width, image.Height);

            var nokia = GetFontFamily().Name.Contains("Nokia");

            var w = avgTextHeight / (nokia ? 12D : 15D);
            var w2 = (int)Math.Ceiling(w) + 2;

            var opacity = ShadowOpacity / 100F;
            var maxOpacity = (255 * opacity).RoundInt().ClampByte();

            Func<int, int, double, double> getShadowOpacity = nokia ? SquareShadow : RoundShadow;

            var sw = Helpers.GetStartedStopwatch();

            if (top.HasValue)
            {
                var x = (_w - top.Value.Width) / 2;
                ShadowImagePart(new Rectangle(new Point(x, 0), top.Value));
            }

            if (bottom.HasValue)
            {
                var x = (_w - bottom.Value.Width) / 2;
                var y =  _h - bottom.Value.Height;
                ShadowImagePart(new Rectangle(new Point(x, y), bottom.Value));
            }

            sw.Log("DrawShadow");
            shadowRealm.Mutate(x => x.DrawImage(image, opacity: 1));
            image.Dispose();
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

                        var outsideImage = kx < 0 || kx >= image.Width || ky < 0 || ky >= image.Height;
                        if (outsideImage) continue;

                        var shadowA = shadowRealm[kx, ky].A;
                        if (shadowA == maxOpacity) continue;

                        var shadowOpacity = opacity * getShadowOpacity(sx, sy, w);
                        if (shadowOpacity == 0) continue;

                        var a = Math.Max(shadowA, shadowOpacity * textA).RoundInt().ClampByte();
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