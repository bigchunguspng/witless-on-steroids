using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Witlesss.Services.Memes
{
    public class DemotivatorDrawer
    {
        public static bool AddLogo;

        private readonly int _w, _h;
        private readonly RectangleF _frame;
        private readonly DrawableText _textA = new(), _textB = new();
        //private readonly EmojiTool _emojer = new() { MemeType = MemeType.Dg };

        private static readonly List<Logo> Logos = new();

        static DemotivatorDrawer() => LoadLogos(WATERMARKS_FOLDER);

        public DemotivatorDrawer(int width = 720, int height = 720)
        {
            _w = width;
            _h = height;

            var imageMarginT = 50;
            var imageMarginS = width == 1280 ? 144 : 50;
            var imageMarginB = 140;

            var imageW = _w - imageMarginS * 2;
            var imageH = _h - imageMarginT - imageMarginB;
            
            Size = new Size(imageW, imageH);

            var space = 5;
            var marginT = imageMarginT - space;
            var marginS = imageMarginS - space;
            var marginB = imageMarginB - space;

            Pic = new Point(imageMarginS, imageMarginT);
            _frame = new RectangleF(marginS - 0.5F, marginT - 0.5F, _w - 2 * marginS, _h - marginT - marginB);

            if (width == 1280)
            {
                _textA.P = DgTextParameters.LargeText(_h - imageMarginB + 33, _w);
                _textB.P = DgTextParameters.LowerText(_h, 0);
            }
            else
            {
                _textA.P = DgTextParameters.UpperText(_h - imageMarginB + 13, _w);
                _textB.P = DgTextParameters.LowerText(_h - imageMarginB + 84, _w);
            }
        }

        public Size Size { get; }
        public Point Pic { get; }

        public string DrawDemotivator(string path, DgText text)
        {

            var sw = Helpers.GetStartedStopwatch();
            var result = PasteImage(DrawFrame(text), path);
            sw.Log("DrawDemotivator");
            return result;
        }

        public string MakeFrame(DgText text) => ImageSaver.SaveImageTemp(DrawFrame(text));
        private Image DrawFrame(DgText text)
        {
            var background = new Image<Rgb24>(_w, _h, Color.Black);

            var ops_frame = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions
                {
                    Antialias = false,
                },
            };
            var penOptions = new PenOptions(Color.White, 1.5F)
            {
                JointStyle = JointStyle.Miter,
                EndCapStyle = EndCapStyle.Polygon,
            };
            var pen = new SolidPen(penOptions);
            background.Mutate(x => x.Draw(ops_frame, pen, _frame));

            if (_w == 720 && AddLogo)
            {
                var logo = PickRandomLogo();
                background.Mutate(x => x.DrawImage(logo.Image, logo.Point, new GraphicsOptions()));
            }

            /*var options = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions
                {
                    AntialiasSubpixelDepth = 0,
                    BlendPercentage = 0,
                    Antialias = false,
                    ColorBlendingMode = PixelColorBlendingMode.Normal,
                    AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver
                },
                ShapeOptions = new ShapeOptions
                {
                    ClippingOperation = ClippingOperation.None,
                    IntersectionRule = IntersectionRule.EvenOdd
                },
                Transform = default
            };*/
            _textA.Pass(text.A);
            _textB.Pass(text.B);

            DrawText(background, _textA);
            DrawText(background, _textB);

            return background;
        }

        private string PasteImage(Image background, string picture)
        {
            //var options = new DecoderOptions { TargetSize = Size };
            using var image = Image.Load(picture);

            image.Mutate(x => x.Resize(Size));
            background.Mutate(x => x.DrawImage(image, Pic, new GraphicsOptions()));

            return ImageSaver.SaveImage(background, PngJpg.Replace(picture, "-D.jpg"));
        }

        private void DrawText(Image image, DrawableText t)
        {
            /*var emoji = EmojiRegex.Matches(t.S);
            if (emoji.Count > 0) _emojer.DrawTextAndEmoji(t.G,   t.S, emoji, t.P);
            else*/
            {
                var options = new DrawingOptions
                {
                    GraphicsOptions = new GraphicsOptions
                    {
                        AntialiasSubpixelDepth = 1,
                        Antialias = true,
                    },
                };
                //image.Mutate(x => x.Fill(t.P.EmojiS > 40 ? Color.Purple : Color.Aqua, t.P.Layout));
                TextMeasurer.TryMeasureCharacterBounds(t.S, t.P.RTO, out var bounds);
                var lines = 0;
                for (var i = 0; i < bounds.Length - 1; i++)
                {
                    // detect 1st line break
                    if (bounds[i].Bounds.X > bounds[i + 1].Bounds.X)
                    {
                        lines++;
                        if (lines == t.P.Lines)
                        {
                            t.S = t.S[..i];
                            break;
                        }
                    }
                }
                image.Mutate(x => x.DrawText(options, t.P.RTO, t.S, brush: t.P.Color, pen: null));
            }
        }

        /*private void DrawText(DrawableText x)
        {
            var emoji = EmojiRegex.Matches(x.S);
            if (emoji.Count > 0) _emojer.DrawTextAndEmoji(x.G,   x.S, emoji, x.P);
            else x.G.DrawString(x.S, x.P.Font, x.P.Color, x.P.Layout, x.P.Format);
        }*/

        private static Logo PickRandomLogo() => Logos[Extension.Random.Next(Logos.Count)];

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

    public class DrawableText
    {
        public string S;
        public DgTextParameters P;

        public void Pass(string s)
        {
            S = s;
        }
    }

    public interface TextParameters
    {
        int Lines           { get; }
        int EmojiS          { get; }
        Font Font           { get; }
        SolidBrush Color    { get; }
        RectangleF Layout   { get; }
        //StringFormat Format { get; }

        public Size EmojiSize => new(EmojiS, EmojiS);
    }

    public class DgTextParameters : TextParameters
    {
        public int Lines           { get; private init; }
        public int EmojiS          { get; private init; }
        public Font Font           { get; private init; }
        public SolidBrush Color    { get; private init; }
        public RectangleF Layout   { get; private init; }
        public RichTextOptions RTO { get; private init; }

        public static DgTextParameters LargeText(int m, int w) => Construct(LargeFont, 1, 72, m, w, 1.00F);
        public static DgTextParameters UpperText(int m, int w) => Construct(UpperFont, 1, 54, m, w, 1.36F);
        public static DgTextParameters LowerText(int m, int w) => Construct(LowerFont, 2, 34, m, w, 1.39F);

        private static Font LargeFont => new(SystemFonts.Get(DEMOTIVATOR_UPPER_FONT), 64);
        private static Font UpperFont => new(SystemFonts.Get(DEMOTIVATOR_UPPER_FONT), 48);
        private static Font LowerFont => new(SystemFonts.Get(DEMOTIVATOR_LOWER_FONT), 24);

        private static DgTextParameters Construct
        (
            Font f, int l, int e, int margin, int width, float lineSpacing
        ) => new()
        {
            Font   = f,
            Lines  = l,
            EmojiS = e,
            Color  = new SolidBrush(SixLabors.ImageSharp.Color.White),
            Layout = new RectangleF(0, margin, width, 100),
            RTO = new RichTextOptions(f)
            {
                LineSpacing = lineSpacing,
                Origin = new Point(0, margin),
                WrappingLength = width,
                WordBreaking = WordBreaking.Standard,
                TextAlignment = TextAlignment.Center,
                KerningMode = KerningMode.Standard,
            }
        };
    }
}