using PF_Bot.Core.Editing;
using PF_Bot.Core.Meme.Options;
using PF_Bot.Core.Meme.Shared;
using PF_Bot.Tools_Legacy.Technical;
using PF_Tools.FFMpeg;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Random = System.Random;
using Logo = (SixLabors.ImageSharp.Image Image, SixLabors.ImageSharp.Point Point);

namespace PF_Bot.Core.Meme.Generators
{
    public class DemotivatorDrawer : IMemeGenerator<TextPair>
    {
        public static bool SingleLine, AddLogo = true;
        public static bool BottomTextIsGenerated;

        public static FontOption FontOptionA, FontOptionB;

        private static readonly List<Logo> Logos = [];

        private readonly int _w, _h, _textW;
        private readonly bool _square;


        private static readonly SolidBrush _heisenberg = new(Color.White);

        private readonly GraphicsOptions _anyGraphicsOptions = new();

        private readonly DrawingOptions _textOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 1 }
        };


        static DemotivatorDrawer() => LoadLogos(Dir_Water);

        public DemotivatorDrawer(int width = 720, int height = 720)
        {
            _square = width == 720;

            _w = width;
            _h = height;

            _textW = _w - (_square ? 40 : 100);

            var imageMarginT = 50;
            var imageMarginS = _square ? 50 : 144;
            var imageMarginB = 140;

            var imageW = _w - imageMarginS * 2;
            var imageH = _h - imageMarginT - imageMarginB;

            ImagePlacement = new Rectangle(imageMarginS, imageMarginT, imageW, imageH);
        }

        public Rectangle ImagePlacement { get; }

        // LOGIC

        public string GenerateMeme(MemeFileRequest request, TextPair text)
        {
            using var frame = DrawFrame(text);
            InsertImage(frame, request);
            frame.ApplyPressure(request.Press);
            return ImageSaver.SaveImage(frame, request.TargetPath, request.Quality);
        }

        public async Task GenerateVideoMeme(MemeFileRequest request, TextPair text)
        {
            using var frame = DrawFrame(text);
            var frameAsFile = ImageSaver.SaveImageTemp(frame);
            var probe = await request.ProbeSource();
            await new FFMpeg_Meme(probe, request, frameAsFile)
                .Demotivator(ImagePlacement.Size, ImagePlacement.Location)
                .FFMpeg_Run();
        }


        private Image DrawFrame(TextPair text)
        {
            var background = new Image<Rgb24>(_w, _h, Color.Black);

            background.DrawFrame(ImagePlacement, _square ? 2 : 3, _square ? 4 : 5, Color.White);

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
            DrawText(background, text.A, typeA);

            // LOWER TEXT
            if (_square && !SingleLine)
                DrawText(background, text.B, TextType.Lower);

            return background;
        }

        private void DrawText(Image image, string text, TextType type)
        {
            var options = GetTextOptions(type);
            var emoji = EmojiTool.FindEmoji(text);
            var lines = type != TextType.Lower || BottomTextIsGenerated ? 1 : emoji.Count > 0 ? -1 : 2;
            if (emoji.Count == 0)
            {
                options.Origin = GetTextOrigin(() => text, type, options.Font.Size, out _, out _, out _);
                var lineBreak = Ruler.DetectLineBreak(text, options, lines);
                var textToRender = lineBreak == -1 ? text : text[..lineBreak];

                image.Mutate(x => x.DrawText(_textOptions, options, textToRender, brush: _heisenberg, pen: null));
            }
            else
            {
                var pngs = EmojiTool.GetEmojiPngs(emoji);
                options.Origin = GetTextOrigin(GetCaseOffsetText, type, options.Font.Size, out var offset, out var fontOffset, out var caseOffset);
                var optionsE = new EmojiTool.Options(_heisenberg, _w, GetEmojiSize(type), fontOffset, lines);
                var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs.AsQueue());
                var x = _w.Gap(textLayer.Width);
                var y = offset - textLayer.Height / 2F + caseOffset;
                var point = new Point(x.RoundInt(), y.RoundInt());
                image.Mutate(ctx => ctx.DrawImage(textLayer, point));

                string GetCaseOffsetText() => EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs);
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

        private static void LoadLogos(FilePath path)
        {
            var files = path.GetFilesInfo(recursive: true);
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

        private RichTextOptions GetTextOptions(TextType type)
        {
            var lower = type is TextType.Lower;
            var fontOption = lower ? FontOptionB : FontOptionA;
            var family = fontOption.GetFontFamily();
            var style = fontOption.GetFontStyle(family);

            var baseFontSize = type switch
            {
                TextType.Lower => 26.4696F, // 24
                TextType.Large => 59.8208F, // 64
                _              => 44.8656F, // 48
            };
            var fontSize = baseFontSize * fontOption.GetSizeMultiplier();

            return new RichTextOptions(family.CreateFont(fontSize, style))
            {
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = _textW,
                LineSpacing = fontOption.GetLineSpacing() * (lower ? 1.39F : 1.2F),
                FallbackFontFamilies = fontOption.GetFallbackFamilies(),
            };
        }

        private PointF GetTextOrigin
        (
            Func<string> getCaseOffsetText, TextType type, float fontSize,
            out float offset, out float fontOffset, out float caseOffset
        )
        {
            var lower = type is TextType.Lower;
            var fontOption = lower ? FontOptionB : FontOptionA;

            offset = 650 + type switch
            {
                TextType.Upper => -25.15F,
                TextType.Lower =>  31.50F,
                _              =>   0,
            };
            fontOffset = fontSize * fontOption.GetFontDependentOffset();
            caseOffset = _square && !SingleLine
                ? 0
                : fontSize * fontOption.GetCaseDependentOffset(getCaseOffsetText());
            var y = offset + fontOffset - caseOffset;

            return new PointF(_w / 2F, y);
        }

        private int GetEmojiSize(TextType type) => type switch
        {
            TextType.Lower => 34,
            TextType.Large => 72,
            _              => 54,
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