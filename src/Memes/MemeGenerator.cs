using ColorHelper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes
{
    public partial class MemeGenerator : MemeGeneratorBase, IMemeGenerator<TextPair>
    {
        // OPTIONS

        public static bool WrapText = true, RandomTextColor;
        public static int FontMultiplier = 100, ShadowOpacity = 100;
        public static CustomColorOption CustomColorBack = new("!"), CustomColorText = new("#");

        // SIZE

        private int _w, _h, _marginY, _marginX;
        private float _fontOffset;
        private Size _captionSize;

        // DATA

        private SolidBrush _textBrush = default!;

        private readonly SolidBrush _white = new(Color.White);

        private readonly DrawingOptions _textDrawingOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 16 }
        };


        // LOGIC

        public string GenerateMeme(MemeFileRequest request, TextPair text)
        {
            FetchImageSize(request);
            SetUp();

            using var image = GetImage(request);

            SetCaptionColor(image);
            using var caption = DrawCaption(text);
            using var meme = Combine(image, caption);

            meme.ApplyPressure(request.Press);

            return request.ExportAsSticker
                ? ImageSaver.SaveImageWebp(meme, request.TargetPath, request.Quality)
                : ImageSaver.SaveImage    (meme, request.TargetPath, request.Quality);
        }

        public Task<string> GenerateVideoMeme(MemeFileRequest request, TextPair text)
        {
            FetchVideoSize(request);
            SetUp();
            SetCaptionColor(CustomColorText.ByCoords ? request.GetVideoSnapshot() : null);

            using var caption = DrawCaption(text);
            return request.UseFFMpeg()
                .Meme(VideoMemeRequest.From(request, caption), _sourceSizeAdjusted)
                .OutAs(request.TargetPath);
        }

        private void SetUp()
        {
            _w = _sourceSizeAdjusted.Width;
            _h = _sourceSizeAdjusted.Height;

            _marginX = Math.Max(_w / 20, 10);
            _marginY = Math.Max(_h / 30, 10);

            SetUpFonts();
        }

        private Image<Rgba32> GetImage(MemeFileRequest request)
        {
            if (request is { IsSticker: true, ExportAsSticker: false })
            {
                using var image = GetImage(request.SourcePath);

                var color = CustomColorBack.GetColor(image) ?? Color.Black;
                var background = new Image<Rgba32>(_w, _h, color);

                background.Mutate(x => x.DrawImage(image));
                return background;
            }
            else
                return GetImage(request.SourcePath);
        }

        private Image<Rgba32> Combine(Image<Rgba32> image, Image<Rgba32> caption)
        {
            image.Mutate(x => x.DrawImage(caption));
            return image;
        }

        private Image<Rgba32> DrawCaption(TextPair text)
        {
            var canvas = new Image<Rgba32>(_w, _h);

            _captionSize = new Size(_w - 2 * _marginX, _h / 3 - _marginY);

            var tuple1 = AddText(canvas, text.A, top: true);
            var tuple2 = AddText(canvas, text.B, top: false);

            return ShadowOpacity > 0 ? DrawShadow(canvas, tuple1, tuple2) : canvas;
        }

        private (float height, float fontSize) AddText(Image<Rgba32> background, string text, bool top)
        {
            if (string.IsNullOrEmpty(text)) return (0, 0);

            text = text.Trim('\n');

            var emoji = EmojiRegex.Matches(text);
            var plain = emoji.Count == 0;
            
            var pngs = plain ? null : EmojiTool.GetEmojiPngs(emoji);
            text = MakeTextFitCard(plain ? text : EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs));

            var origin = GetTextOrigin(text, top, out var caseOffset);
            var options = GetDefaultTextOptions(origin, top);

            if (plain)
            {
                options.WrappingLength = -1;
                background.Mutate(x => x.DrawText(_textDrawingOptions, options, text, _textBrush, pen: null));
            }
            else
            {
                var pixelate = FontWizard.FontIsPixelated();
                var optionsE = new EmojiTool.Options(_textBrush, _w, GetEmojiSize(), _fontOffset, Pixelate: pixelate);
                var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs!.AsQueue());
                var point = GetFunnyOrigin(textLayer.Size, options, top, caseOffset);
                background.Mutate(ctx => ctx.DrawImage(textLayer, point));
            }

            LogDebug($"/meme >> font size: {FontSize:F2}");

            return (FontSize * GetLineSpacing() * text.GetLineCount(), FontSize);
        }

        private void SetCaptionColor(Image<Rgba32>? image)
        {
            var color = CustomColorText.GetColor(image);
            _textBrush = color is not null 
                ? new SolidBrush(color.Value) 
                : RandomTextColor ? RandomColor() : _white;
        }

        private SolidBrush RandomColor()
        {
            var h =       Random.Shared.Next(360);
            var s = (byte)Random.Shared.Next(50, 100);
            var l = (byte)Random.Shared.Next(50,  95);

            var rgb = ColorConverter.HslToRgb(new HSL(h, s, l));
            return new SolidBrush(rgb.ToRgb24());
        }
    }
}