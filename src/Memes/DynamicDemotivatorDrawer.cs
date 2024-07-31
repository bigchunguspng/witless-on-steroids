using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme;
using Witlesss.MediaTools;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes // ReSharper disable InconsistentNaming
{
    public partial class DynamicDemotivatorDrawer : MemeGeneratorBase, IMemeGenerator<string>
    {
        // OPTIONS

        public static CustomColorOption CustomColorOption;
        public static bool CropEdges, WrapText;
        public static float MinFontSize = 10;

        // SIZE

        private const int FM = 5;

        private int TextWidth => (fullW + imageW) / 2;

        private int imageW, imageH, fullW, fullH, marginTop;
        private double _ratio;

        private Point _picOrigin;
        private RectangleF _frame;

        public Point Location => _picOrigin;
        public Size ImageSize => new(imageW, imageH);

        // DATA

        private SolidBrush TextBrush;
        private SolidPen FramePen;

        private readonly SolidBrush WhiteBrush = new(Color.White);

        private readonly DrawingOptions _textOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 1 }
        };

        private readonly DrawingOptions _frameOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = false }
        };

        private readonly SolidPen _framePen = new(GetPenOptions(Color.White));

        private SolidPen GetSolidPen(Rgba32 color) => new(GetPenOptions(color));

        private static PenOptions GetPenOptions(Rgba32 color) => new(color, 1.5F)
        {
            JointStyle = JointStyle.Miter,
            EndCapStyle = EndCapStyle.Polygon
        };

        // LOGIC

        public string GenerateMeme(MemeFileRequest request, string text)
        {
            FetchImageSize(request);
            SetUp();

            text = ArrangeText(text, out var emojiPngs);

            SetUpFrameSize(text);

            using var image = GetImage(request.SourcePath);
            using var frame = DrawFrame(text, emojiPngs);

            InsertImage(frame, image);

            return ImageSaver.SaveImage(frame, request.TargetPath, request.Quality);
        }

        public Task<string> GenerateVideoMeme(MemeFileRequest request, string text)
        {
            FetchVideoSize(request);
            SetUp();

            text = ArrangeText(text, out var emojiPngs);

            SetUpFrameSize(text);

            using var frame = DrawFrame(text, emojiPngs);
            var frameAsFile = ImageSaver.SaveImageTemp(frame);

            var full_size = FFMpegXD.GetPictureSize(frameAsFile);//.FitSize(720);

            return new F_Combine(request.SourcePath, frameAsFile)
                .D300(request.GetCRF(), ImageSize, Location, full_size)
                .OutputAs(request.TargetPath);
        }

        private void SetUp()
        {
            imageW = _sourceSizeAdjusted.Width;
            imageH = _sourceSizeAdjusted.Height;

            _ratio = _sourceSizeAdjusted.AspectRatio();

            SetUpFonts();
            SetColor();
        }

        // CALCULATE

        private string ArrangeText(string text, out EmojiPngList? pngs)
        {
            var emoji = EmojiRegex.Matches(text);
            if (emoji.Count == 0)
            {
                pngs = null;
                return MakeTextFitCard(text);
            }
            else
            {
                pngs = EmojiTool.GetEmojiPngs(emoji);
                return MakeTextFitCard(EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs));
            }
        }

        private void SetUpFrameSize(string text)
        {
            var space = imageH / 30F;
            var lines = text.GetLineCount();
            Log(lines.ToString());
            var lineHeight = FontSize * ExtraFonts.GetRelativeSize() * GetLineSpacing();
            var textHeight = _textHeight + 0.5F * lineHeight;
            Log(_textHeight.ToString(), ConsoleColor.DarkGreen);
            fullH = (imageH + textHeight + 4 * space).RoundInt().ToEven();
            fullW = (fullH * _ratio).RoundInt().ToEven();

            marginTop = (2 * space).RoundInt();

            var size = new Size(fullW, fullH).FitSize(new Size(1280, 720));
            if (size.Width != fullW)
            {
                var k = size.Width / (float) fullW;
                fullW = (fullW * k).RoundInt().ToEven();
                fullH = (fullH * k).RoundInt().ToEven();
                imageW = (imageW * k).RoundInt();
                imageH = (imageH * k).RoundInt();
                _sourceSizeAdjusted = new Size(imageW, imageH);
                _textHeight *= k;
                marginTop = (marginTop * k).RoundInt();

                ResizeFont(FontSize * k);
            }

            var m = 5; // frame margin todo make it size dependent

            _picOrigin = new Point((fullW - imageW) / 2, marginTop + 1); //todo +1 ?
            _frame = new RectangleF(_picOrigin.X - m - 0.5F, _picOrigin.Y - m - 0.5F, imageW + 2 * m, imageH + 2 * m);
        }

        // DRAW

        private Image DrawFrame(string text, EmojiPngList? emojiPngs)
        {
            var background = new Image<Rgb24>(fullW, fullH, Color.Black);

            AdjustTextOffset(text);

            var options = GetDefaultTextOptions();

            if (emojiPngs is null)
            {
#if DEBUG
                Debug_Text(background, options);
#endif
                background.Mutate(x => x.DrawText(options, text, TextBrush, pen: null));
            }
            else
            {
                var optionsE = new EmojiTool.Options(TextBrush, GetEmojiSize(), _fontOffset);
                var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, emojiPngs.AsQueue(), out _);

                background.Mutate(ctx => ctx.DrawImage(textLayer, GetOriginFunny(textLayer.Size)));
            }

            background.Mutate(x => x.Draw(_frameOptions, FramePen, _frame));

            return background;
        }

        private Point GetOriginFunny(Size size)
        {
            var x = fullW.Gap(size.Width);
            var y = (fullH - imageH - marginTop - 5).Gap(size.Height) - _caseOffset;
            return new Point(x.RoundInt(), y.RoundInt());
        }

        private void InsertImage(Image background, Image image)
        {
            background.Mutate(x => x.DrawImage(image, _picOrigin));

            var size = FitSize(background.Size, 1280);
            if (size != background.Size)
            {
                background.Mutate(x => x.Resize(size));
            }
        }


        // OTHER STUFF I GUESS

        public static Size FitSize(Size size, int max = 720) => size.FitSize(max);

        public void SetColor()
        {
            var color = CustomColorOption.GetColor();
            TextBrush = color is null ? WhiteBrush : new SolidBrush(color.Value);
            FramePen  = color is null ? _framePen  :  GetSolidPen  (color.Value);
        }


        /*/// <summary> CALL THIS after changing <see cref="textH"/> </summary>
        private void AdjustTotalSize()
        {
            fullH = FF_Extensions.ToEven(marginTop + imageH + FM + textH);
            fullW = FF_Extensions.ToEven((int)(fullH * _ratio));
        }
        private void AdjustImageFrame()
        {
            _picOrigin = new Point((fullW - imageW) / 2, marginTop + 1);
            _frame = new RectangleF(_picOrigin.X - FM - 0.5F, _picOrigin.Y - FM - 0.5F, imageW + 2 * FM, imageH + 2 * FM);
        }*/
        private void Debug_Text(Image image, RichTextOptions options)
        {
            var y = options.Origin.Y - _textHeight / 2F;
            image.Mutate(x => x.Fill(Color.Indigo, new RectangleF(0, y, fullW, _textHeight)));
        }
    }
}