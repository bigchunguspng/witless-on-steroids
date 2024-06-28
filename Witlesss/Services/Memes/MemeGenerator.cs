using System;
using System.Linq;
using System.Threading.Tasks;
using ColorHelper;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Backrooms.Types;
using Witlesss.Commands.Meme;
using Witlesss.MediaTools;

namespace Witlesss.Services.Memes
{
    public partial class MemeGenerator : IMemeGenerator<DgText>
    {
        // OPTIONS

        public static bool WrapText = true, ColorText;
        public static int FontMultiplier = 10, ShadowOpacity = 100;
        public static CustomColorOption CustomColorOption;

        // SIZE

        private int _w, _h, _margin, _centerX;
        private Size _captionArea;

        // FONT

        public static readonly ExtraFonts ExtraFonts = new("meme");

        private float _startingFontSize;

        // DATA

        private readonly SolidBrush _white = new(Color.White);

        private readonly DrawingOptions _textDrawingOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 16 }
        };

        // LOGIC

        public string GenerateMeme(MemeFileRequest request, DgText text)
        {
            var (size, info) = GetImageSize(request.SourcePath);
            SetUp(size);

            using var image = GetImage(request, size, info);
            using var caption = DrawCaption(text);
            using var meme = Combine(image, caption);

            return ImageSaver.SaveImage(meme, request.TargetPath, request.Quality);
        }

        public Task<string> GenerateVideoMeme(MemeFileRequest request, DgText text)
        {
            var size = SizeHelpers.GetImageSize_FFmpeg(request.SourcePath).GrowSize().ValidMp4Size();
            SetUp(size);

            using var caption = DrawCaption(text);
            var captionAsFile = ImageSaver.SaveImageTemp(caption);
            return new F_Combine(request.SourcePath, captionAsFile)
                .Meme(request.GetCRF(), size)
                .OutputAs(request.TargetPath);
        }

        private void SetUp(Size size)
        {
            _w = size.Width;
            _h = size.Height;

            _centerX = _w / 2;
            _margin = Math.Min(_h / 72, 10);

            var minSide = (int)Math.Min(_w, 1.5 * _h);
            _startingFontSize = Math.Max(minSide * FontMultiplier * ExtraFonts.GetSizeMultiplier() / 120, 12);
        }

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
            image.Mutate(x => x.DrawImage(caption));
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

        private Image<Rgba32> GetImage(MemeFileRequest request, Size size, ImageInfo info)
        {
            var image = Image.Load<Rgba32>(request.SourcePath);
            if (size != info.Size)
            {
                image.Mutate(x => x.Resize(size));
            }

            if (request.IsSticker /* && not send as sticker ? */)
            {
                var color = CustomColorOption.GetColor() ?? Color.Black;
                var background = new Image<Rgba32>(image.Width, image.Height, color);
                background.Mutate(x => x.DrawImage(image));
                image.Dispose();
                return background;
            }

            //image.Mutate(x => x.Dither(new OrderedDither((uint)d)));
            return image;
        }
    }
}