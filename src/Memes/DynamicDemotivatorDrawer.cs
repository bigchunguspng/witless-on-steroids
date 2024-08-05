using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme.Core;
using Witlesss.MediaTools;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes // ReSharper disable InconsistentNaming
{
    public partial class DynamicDemotivatorDrawer : MemeGeneratorBase, IMemeGenerator<string>
    {
        // OPTIONS

        public static CustomColorOption CustomColorOption;
        public static bool Minimalist, WrapText;
        public static float MinFontSize = 10;

        // SIZE

        private const int FRAME_MARGIN = 5;

        private int _frameMargin, _frameWidth;

        private int imageW, imageH, fullW, fullH, marginTop;
        private double _ratio;

        private Point _imageOrigin;

        // DATA

        private Rgb24 FrameColor;
        private SolidBrush TextBrush = default!;

        private readonly SolidBrush WhiteBrush = new(Color.White);

        // LOGIC

        public string GenerateMeme(MemeFileRequest request, string text)
        {
            FetchImageSize(request);
            SetUp();

            text = ArrangeText(text, out var emojiPngs);

            SetUpFrameSize();

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

            SetUpFrameSize();

            using var frame = DrawFrame(text, emojiPngs);
            var frameAsFile = ImageSaver.SaveImageTemp(frame);

            var full_size = FFMpegXD.GetPictureSize(frameAsFile);//.FitSize(720);

            return new F_Combine(request.SourcePath, frameAsFile)
                .D300(request.GetCRF(), _sourceSizeAdjusted, _imageOrigin, full_size)
                .OutputAs(request.TargetPath);
        }

        private void SetUp()
        {
            imageW = _sourceSizeAdjusted.Width;
            imageH = _sourceSizeAdjusted.Height;

            _ratio = _sourceSizeAdjusted.AspectRatio();

            if (_ratio > 3) Minimalist = true;

            SetUpFonts();
            SetColor();
        }

        // CALCULATE

        private string ArrangeText(string text, out EmojiPngList? pngs)
        {
            var emoji = EmojiRegex.Matches(text);
            var plain = emoji.Count == 0;
            pngs = plain ? null : EmojiTool.GetEmojiPngs(emoji);
            text = plain ? text : EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs);
            return MakeTextFitCard(text);
        }

        private void SetUpFrameSize()
        {
            var space = Math.Max(imageH / 30F, 4);
            var lineHeight = FontSize * GetLineSpacing();
            var textHeight = _textHeight + 0.5F * lineHeight;
            var n = Minimalist ? 2 : 4;
            fullH = (imageH + textHeight + n * space).RoundInt().ToEven();
            fullW = Minimalist ? imageW : (fullH * _ratio).RoundInt().ToEven();

            marginTop = Minimalist ? 0 : (2 * space).RoundInt();

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

            _imageOrigin = Minimalist ? Point.Empty : new Point((fullW - imageW) / 2, marginTop);

            _frameMargin = imageW + imageH > 800 ? 5 : 3;
            _frameWidth  = imageW + imageH > 800 ? 3 : 2;
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

            background.DrawFrame(new Rectangle(_imageOrigin, _sourceSizeAdjusted), _frameWidth, _frameMargin, FrameColor);

            return background;
        }

        private void InsertImage(Image background, Image image)
        {
            background.Mutate(x => x.DrawImage(image, _imageOrigin));

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
            FrameColor = color?.Rgb ?? Color.White;
            TextBrush = color is null ? WhiteBrush : new SolidBrush(color.Value);
        }

        private void Debug_Text(Image image, RichTextOptions options)
        {
            var y = options.Origin.Y - _textHeight / 2F;
            image.Mutate(x => x.Fill(Color.Indigo, new RectangleF(0, y, fullW, _textHeight)));
        }
    }
}