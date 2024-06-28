using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme;
using Witlesss.MediaTools;
using Random = System.Random;

namespace Witlesss.Services.Memes
{
    public class DemotivatorDrawer : IMemeGenerator<DgText>
    {
        public static bool AddLogo;

        private static readonly List<Logo> Logos = [];
        private static readonly EmojiTool _emojer = new() { MemeType = MemeType.Dg };

        private readonly int _w, _h;
        private readonly bool _square;
        private readonly RectangleF _frame;
        private readonly DgTextOptions _textA, _textB;


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

            if (_square)
            {
                _textA = DgTextOptions.UpperText(_h - imageMarginB + 13, _w);
                _textB = DgTextOptions.LowerText(_h - imageMarginB + 84, _w);
            }
            else
            {
                _textA = DgTextOptions.LargeText(_h - imageMarginB + 33, _w);
                _textB = DgTextOptions.LowerText(_h, 0); // not used
            }
        }

        public Rectangle ImagePlacement { get; }

        // LOGIC

        public string GenerateMeme(MemeFileRequest request, DgText text)
        {
            using var frame = DrawFrame(text);
            InsertImage(frame, request);
            return ImageSaver.SaveImage(frame, request.TargetPath, request.Quality);
        }

        public Task<string> GenerateVideoMeme(MemeFileRequest request, DgText text)
        {
            using var frame = DrawFrame(text);
            var frameAsFile = ImageSaver.SaveImageTemp(frame);
            return new F_Combine(request.SourcePath, frameAsFile)
                .Demo(request.GetCRF(), this)
                .OutputAs(request.TargetPath);
        }


        private Image DrawFrame(DgText text)
        {
            var background = new Image<Rgb24>(_w, _h, Color.Black);

            background.Mutate(x => x.Draw(_frameOptions, _framePen, _frame));

            if (_square && AddLogo)
            {
                var logo = PickRandomLogo();
                background.Mutate(x => x.DrawImage(logo.Image, logo.Point, _anyGraphicsOptions));
            }

            DrawText(background, text.A, _textA);
            DrawText(background, text.B, _textB);

            return background;
        }

        // todo text margin + do only 1 bottom text line if text is generated
        private void DrawText(Image image, string text, DgTextOptions o)
        {
            var emoji = EmojiRegex.Matches(text);
            if (emoji.Count > 0)
            {
                var options = new RichTextOptions(o.Options)
                {
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var parameters = new EmojiTool.Options(o.Color, o.EmojiSize);
                var textLayer = _emojer.DrawEmojiText(text, options, parameters, out _);
                var point = new Point((_w - textLayer.Width) / 2, o.Options.Origin.Y.RoundInt());
                image.Mutate(x => x.DrawImage(textLayer, point));
            }
            else
            {
                //image.Mutate(x => x.Fill(p.EmojiS > 40 ? Color.Purple : Color.Aqua, p.Layout));
                var lineBreak = TextMeasuring.DetectLineBreak(text, o.Options, o.Lines);
                var noLineBreaks = lineBreak == -1;
                var textToRender = noLineBreaks ? text : text[..lineBreak];

                image.Mutate(x => x.DrawText(_textOptions, o.Options, textToRender, brush: o.Color, pen: null));
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
                    var logo = new Logo(image, new Point(x, y));
                    Logos.Add(logo);
                }
            }
        }
    }

    public record Logo(Image Image, Point Point);

    public record DgTextOptions(RichTextOptions Options, int Lines, int EmojiSize)
    {
        public SolidBrush Color => _heisenberg;

        private static readonly SolidBrush _heisenberg = new(SixLabors.ImageSharp.Color.White);

        public static DgTextOptions LargeText(int m, int w) => Construct(LargeFont, 1, 72, m, w, 1.13F);
        public static DgTextOptions UpperText(int m, int w) => Construct(UpperFont, 1, 54, m, w, 1.36F);
        public static DgTextOptions LowerText(int m, int w) => Construct(LowerFont, 2, 34, m, w, 1.39F);

        private static Font LargeFont => new(SystemFonts.Get(DEMOTIVATOR_UPPER_FONT), 64);
        private static Font UpperFont => new(SystemFonts.Get(DEMOTIVATOR_UPPER_FONT), 48);
        private static Font LowerFont => new(SystemFonts.Get(DEMOTIVATOR_LOWER_FONT), 24);

        private static DgTextOptions Construct
        (
            Font font, int lines, int emojiSize, int margin, int width, float lineSpacing
        )
        {
            var options = new RichTextOptions(font)
            {
                Origin = new Point(0, margin),
                WrappingLength = width,
                LineSpacing = lineSpacing,
                TextAlignment = TextAlignment.Center,
                WordBreaking = WordBreaking.Standard,
                KerningMode = KerningMode.Standard,
                FallbackFontFamilies = ExtraFonts.FallbackFamilies
            };
            return new DgTextOptions(options, lines, emojiSize);
        }
    }
}