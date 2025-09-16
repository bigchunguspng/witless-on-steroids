using ColorHelper;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Editing;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Core.Memes.Generators
{
    public struct MemeOptions_Meme()
    {
        public FontOption FontOption;

        public ColorOption CustomColorBack;
        public ColorOption CustomColorText;

        public int FontMultiplier = 100;
        public int TextOffset     =  -1;

        public byte ShadowOpacity = 100;

        public bool WrapText = true;
        public bool RandomTextColor;
        public bool RandomOffset;
        public bool           NoMargin;
        public bool AbsolutelyNoMargin;

        public bool CustomOffsetMode => RandomOffset || TextOffset >= 0;
    }

    public partial class MemeGenerator(MemeOptions_Meme op) : MemeGeneratorBase, IMemeGenerator<TextPair>
    {
        // SIZE

        private int _w, _h, _marginY, _marginX;
        private float _fontOffset;
        private Size _captionSize;

        // DATA

        private SolidBrush _textBrush = null!;

        private readonly SolidBrush _white = new(Color.White);

        private readonly DrawingOptions _textDrawingOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 16 }
        };


        // LOGIC

        public async Task GenerateMeme(MemeFileRequest request, FilePath output, TextPair text)
        {
            await FetchImageSize(request);
            SetUp();

            using var image = await GetImage(request);

            SetCaptionColor(image);
            using var caption = DrawCaption(text);
            using var meme = Combine(image, caption);

            meme.ApplyPressure(request.Press);

            var saveImageTask = request.ExportAsSticker
                ? ImageSaver.SaveImageWebp(meme, output, request.Quality)
                : ImageSaver.SaveImageJpeg(meme, output, request.Quality);

            await saveImageTask;
        }

        public async Task GenerateVideoMeme(MemeFileRequest request, FilePath output, TextPair text)
        {
            await FetchVideoSize(request);
            SetUp();
            SetCaptionColor(op.CustomColorText.ByCoords ? await request.GetVideoSnapshot() : null);

            using var caption = DrawCaption(text);
            var captionAsFile = await ImageSaver.SaveImageTemp(caption);
            var probe = await request.ProbeSource();
            await new FFMpeg_Meme(probe, request, output, captionAsFile)
                .Meme(_sourceSizeAdjusted)
                .FFMpeg_Run();
        }

        private void SetUp()
        {
            _w = _sourceSizeAdjusted.Width;
            _h = _sourceSizeAdjusted.Height;

            _marginX = op.NoMargin ? 0 : op.AbsolutelyNoMargin ? 0 - _w / 20 : Math.Max(_w / 20, 10);
            _marginY = op.NoMargin ? 0 : op.AbsolutelyNoMargin ? 0 - _h / 30 : Math.Max(_h / 30, 10);

            if (op.CustomOffsetMode)
            {
                var offset = op.RandomOffset ? Fortune.RandomInt(15, 85) : op.TextOffset;
                _marginY = _h * offset / 100;
            }

            SetUpFonts();
        }

        private async Task<Image<Rgba32>> GetImage(MemeFileRequest request)
        {
            if (request is { IsSticker: true, ExportAsSticker: false })
            {
                using var image = await GetImage(request.SourcePath);

                var color = op.CustomColorBack.GetColor(image) ?? Color.Black;
                var background = new Image<Rgba32>(_w, _h, color);

                background.Mutate(x => x.DrawImage(image));
                return background;
            }
            else
                return await GetImage(request.SourcePath);
        }

        private Image<Rgba32> Combine(Image<Rgba32> image, Image<Rgba32> caption)
        {
            image.Mutate(x => x.DrawImage(caption));
            return image;
        }

        private Image<Rgba32> DrawCaption(TextPair text)
        {
            var canvas = new Image<Rgba32>(_w, _h);

            var height = op.CustomOffsetMode
                ? _h / 3
                : _h / 3 - _marginY;

            _captionSize = new Size(_w - 2 * _marginX, height);

            var tuple1 = AddText(canvas, text.A, top: true);
            var tuple2 = AddText(canvas, text.B, top: false);

            return op.ShadowOpacity > 0 ? DrawShadow(canvas, tuple1, tuple2) : canvas;
        }

        private (float height, float fontSize) AddText(Image<Rgba32> background, string text, bool top)
        {
            if (text.IsNull_OrEmpty()) return (0, 0);

            text = text.Trim('\n');

            var emoji = EmojiTool.FindEmoji(text);
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
                var pixelate = op.FontOption.FontIsPixelated();
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
            var color = op.CustomColorText.GetColor(image);
            _textBrush = color is not null 
                ? new SolidBrush(color.Value) 
                : op.RandomTextColor ? RandomColor() : _white;
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