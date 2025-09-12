using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Editing;
using PF_Bot.Core.Meme.Options;
using PF_Bot.Core.Meme.Shared;
using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Core.Meme.Generators // ReSharper disable InconsistentNaming
{
    public partial class DynamicDemotivatorDrawer : MemeGeneratorBase, IMemeGenerator<string>
    {
        // OPTIONS

        public static CustomColorOption CustomColor = new("#");
        public static bool Minimalist, WrapText;
        public static float MinSizeMultiplier = 10, FontSizeMultiplier = 100;

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

        public async Task GenerateMeme(MemeFileRequest request, FilePath output, string text)
        {
            await FetchImageSize(request);
            SetUp();

            text = ArrangeText(text, out var emojiPngs);

            SetUpFrameSize(request);
            using var image = await GetImage(request.SourcePath);

            SetColor(image);
            using var frame = DrawFrame(text, emojiPngs);

            InsertImage(frame, image);

            frame.ApplyPressure(request.Press);

            await ImageSaver.SaveImageJpeg(frame, output, request.Quality);
        }

        public async Task GenerateVideoMeme(MemeFileRequest request, FilePath output, string text)
        {
            await FetchVideoSize(request);
            SetUp();

            text = ArrangeText(text, out var emojiPngs);

            SetUpFrameSize(request);
            SetColor(CustomColor.ByCoords ? await request.GetVideoSnapshot() : null);

            using var frame = DrawFrame(text, emojiPngs);
            var frameAsFile = await ImageSaver.SaveImageTemp(frame);

            var probe = await request.ProbeSource();
            await new FFMpeg_Meme(probe, request, output, frameAsFile)
                .Demotivator(_sourceSizeAdjusted, _imageOrigin)
                .FFMpeg_Run();
        }

        private void SetUp()
        {
            imageW = _sourceSizeAdjusted.Width;
            imageH = _sourceSizeAdjusted.Height;

            _ratio = _sourceSizeAdjusted.AspectRatio();

            if (_ratio > 3) Minimalist = true;

            SetUpFonts();
        }

        // CALCULATE

        private string ArrangeText(string text, out EmojiPngList? pngs)
        {
            var emoji = EmojiTool.FindEmoji(text);
            var plain = emoji.Count == 0;
            pngs = plain ? null : EmojiTool.GetEmojiPngs(emoji);
            text = plain ? text : EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs);
            return MakeTextFitCard(text);
        }

        private void SetUpFrameSize(MemeFileRequest request)
        {
            var space = Math.Max(imageH / 30F, 4);
            var lineHeight = FontSize * GetLineSpacing();
            var textHeight = _textHeight + 0.5F * lineHeight;
            var n = Minimalist ? 2 : 3;
            fullH = (imageH + textHeight + n * space).RoundInt().ToEven();
            fullW = Minimalist ? imageW : (fullH * _ratio).RoundInt().ToEven();

            marginTop = Minimalist ? 0 : (2 * space).RoundInt();

            var targetSize = request.IsVideo ? new Size(1280, 720) : new Size(1280, 800);
            var size = new Size(fullW, fullH).FitSize(targetSize);
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
                options.WrappingLength = -1;
                background.Mutate(x => x.DrawText(options, text, TextBrush, pen: null));
            }
            else
            {
                var optionsE = new EmojiTool.Options(TextBrush, fullW, GetEmojiSize(), _fontOffset);
                var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, emojiPngs.AsQueue());

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

        private static Size FitSize(Size size, int max = 720) => size.FitSize(max);

        private void SetColor(Image<Rgba32>? image)
        {
            var color = CustomColor.GetColor(image);
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