using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Meme;
using Witlesss.MediaTools;
using Witlesss.Memes.Shared;
using Random = System.Random;
using Logo = (SixLabors.ImageSharp.Image Image, SixLabors.ImageSharp.Point Point);

namespace Witlesss.Memes
{
    public class DemotivatorDrawer : IMemeGenerator<TextPair>
    {
        public static bool SingleLine, AddLogo = true;
        public static bool BottomTextIsGenerated;

        public static readonly ExtraFonts ExtraFontsA = new("d[vg]", "&"), ExtraFontsB = new("d[vg]", @"\*");

        private static readonly List<Logo> Logos = [];

        private readonly int _w, _h;
        private readonly bool _square;
        private readonly RectangleF _frame;


        private static readonly SolidBrush _heisenberg = new(Color.White);

        private readonly GraphicsOptions _anyGraphicsOptions = new();

        private readonly DrawingOptions _textOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 1 }
        };

        private readonly DrawingOptions _frameOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = false }
        };

        private readonly SolidPen _framePen = new(new PenOptions(Color.White, 1.5F)
        {
            JointStyle = JointStyle.Miter,
            EndCapStyle = EndCapStyle.Polygon
        });


        static DemotivatorDrawer() => LoadLogos(Paths.Dir_Water);

        public DemotivatorDrawer(int width = 720, int height = 720)
        {
            _square = width == 720;

            _w = width;
            _h = height;

            var imageMarginT = 50;
            var imageMarginS = _square ? 50 : 144;
            var imageMarginB = 140;

            var imageW = _w - imageMarginS * 2;
            var imageH = _h - imageMarginT - imageMarginB;

            var space = 5;
            var marginT = imageMarginT - space;
            var marginS = imageMarginS - space;
            var marginB = imageMarginB - space;

            ImagePlacement = new Rectangle(imageMarginS, imageMarginT, imageW, imageH);
            _frame = new RectangleF
            (
                marginS - 0.5F,
                marginT - 0.5F,
                _w - 2 * marginS,
                _h - marginT - marginB
            );
        }

        public Rectangle ImagePlacement { get; }

        // LOGIC

        public string GenerateMeme(MemeFileRequest request, TextPair text)
        {
            using var frame = DrawFrame(text);
            InsertImage(frame, request);
            return ImageSaver.SaveImage(frame, request.TargetPath, request.Quality);
        }

        public Task<string> GenerateVideoMeme(MemeFileRequest request, TextPair text)
        {
            using var frame = DrawFrame(text);
            var frameAsFile = ImageSaver.SaveImageTemp(frame);
            return new F_Combine(request.SourcePath, frameAsFile)
                .Demo(request.GetCRF(), this)
                .OutputAs(request.TargetPath);
        }


        private Image DrawFrame(TextPair text)
        {
            var background = new Image<Rgb24>(_w, _h, Color.Black);

            background.Mutate(x => x.Draw(_frameOptions, _framePen, _frame));

            if (_square && AddLogo)
            {
                var logo = PickRandomLogo();
                background.Mutate(x => x.DrawImage(logo.Image, logo.Point, _anyGraphicsOptions));
            }

            // UPPER TEXT
            var typeA = _square
                ? SingleLine
                    ? TextType.Single
                    : TextType.Upper
                : TextType.Large;
            DrawText(background, text.A, typeA, 1);

            // LOWER TEXT
            if (_square && !SingleLine)
                DrawText(background, text.B, TextType.Lower, BottomTextIsGenerated ? 1 : 2);

            return background;
        }

        private void DrawText(Image image, string text, TextType type, int lines)
        {
            var options = GetTextOptions(type, text, out var offset, out var fontOffset, out var caseOffset);
            var emoji = EmojiRegex.Matches(text);
            if (emoji.Count > 0)
            {
                var pngs = EmojiTool.GetEmojiPngs(emoji).AsQueue();
                var optionsE = new EmojiTool.Options(_heisenberg, GetEmojiSize(type), fontOffset);
                var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs, out _);
                var x = _w.Gap(textLayer.Width);
                var y = offset - textLayer.Height / 2F + caseOffset;
                var point = new Point(x.RoundInt(), y.RoundInt());
                image.Mutate(ctx => ctx.DrawImage(textLayer, point));
            }
            else
            {
                var lineBreak = TextMeasuring.DetectLineBreak(text, options, lines);
                var textToRender = lineBreak == -1 ? text : text[..lineBreak];

                image.Mutate(x => x.DrawText(_textOptions, options, textToRender, brush: _heisenberg, pen: null));
            }
        }

        private void InsertImage(Image frame, MemeFileRequest request)
        {
            using var image = Image.Load(request.SourcePath);

            image.Mutate(x => x.Resize(ImagePlacement.Size));
            frame.Mutate(x => x.DrawImage(image, ImagePlacement.Location, _anyGraphicsOptions));
        }


        // LOGOS (WATERMARKS)

        private static Logo PickRandomLogo() => Logos[Random.Shared.Next(Logos.Count)];

        private static void LoadLogos(string path)
        {
            var files = GetFilesInfo(path, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var coords = file.Name.Replace(file.Extension, "").Split(' ');
                if (int.TryParse(coords[0], out var x) && int.TryParse(coords[^1], out var y))
                {
                    var image = Image.Load(file.FullName);
                    Logos.Add((image, new Point(x, y)));
                }
            }
        }


        // TEXT

        private RichTextOptions GetTextOptions
        (
            TextType type, string text,
            out float offset, out float fontOffset, out float caseOffset
        )
        {
            var lower = type is TextType.Lower;
            var extraFonts = lower ? ExtraFontsB : ExtraFontsA;
            var family = extraFonts.GetFontFamily(lower ? "co" : "ro");
            var style = extraFonts.GetFontStyle(family);

            var baseFontSize = type switch
            {
                TextType.Lower => 26.4696F, // 24
                TextType.Large => 59.8208F, // 64
                _              => 44.8656F  // 48
            };
            var fontSize = baseFontSize * extraFonts.GetSizeMultiplier();

            offset = 650 + type switch
            {
                TextType.Upper => -25.15F,
                TextType.Lower =>  31.50F,
                _ => 0
            };
            fontOffset = fontSize * extraFonts.GetFontDependentOffset();
            caseOffset = fontSize * extraFonts.GetCaseDependentOffset(EmojiTool.ReplaceEmoji(text, "👌"));
            var y = offset + fontOffset - caseOffset;

            return new RichTextOptions(family.CreateFont(fontSize, style))
            {
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(_w / 2F, y),
                WrappingLength = _w,
                LineSpacing = extraFonts.GetLineSpacing() * (lower ? 1.39F : 1.2F),
                FallbackFontFamilies = ExtraFonts.FallbackFamilies
            };
        }

        private int GetEmojiSize(TextType type) => type switch
        {
            TextType.Lower => 34,
            TextType.Large => 72,
            _              => 54
        };

        private enum TextType
        {
            Upper,
            Lower,
            Single,
            Large
        }
    }
}