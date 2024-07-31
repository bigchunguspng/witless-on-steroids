using System;
using System.Threading.Tasks;
using ColorHelper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme;
using Witlesss.MediaTools;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes
{
    public partial class MemeGenerator : MemeGeneratorBase, IMemeGenerator<TextPair>
    {
        // OPTIONS

        public static bool WrapText = true, ColorText;
        public static int FontMultiplier = 10, ShadowOpacity = 100;
        public static CustomColorOption CustomColorOption;

        // SIZE

        private int _w, _h, _marginY, _marginX;
        private float _fontOffset;
        private Size _captionSize;

        // DATA

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
            using var caption = DrawCaption(text);
            using var meme = Combine(image, caption);

            return request.ExportAsSticker
                ? ImageSaver.SaveImagePng(meme, request.TargetPath, request.Quality)
                : ImageSaver.SaveImage   (meme, request.TargetPath, request.Quality);
        }

        public Task<string> GenerateVideoMeme(MemeFileRequest request, TextPair text)
        {
            FetchVideoSize(request);
            SetUp();

            using var caption = DrawCaption(text);
            var captionAsFile = ImageSaver.SaveImageTemp(caption);
            return new F_Combine(request.SourcePath, captionAsFile)
                .Meme(request.GetCRF(), _sourceSizeAdjusted)
                .OutputAs(request.TargetPath);
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
                var color = CustomColorOption.GetColor() ?? Color.Black;
                var background = new Image<Rgba32>(_w, _h, color);

                using var image = GetImage(request.SourcePath);

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
            if (emoji.Count == 0)
            {
                text = MakeTextFitCard(text);

                Log($"/meme >> font size: {FontSize:F2}", ConsoleColor.DarkYellow);

                var origin = GetTextOrigin(text, top, out _);
                var options = GetDefaultTextOptions(origin, top);

                background.Mutate(x => x.DrawText(_textDrawingOptions, options, text, GetBrush(), pen: null));
            }
            else
            {
                var pngs = EmojiTool.GetEmojiPngs(emoji);

                text = MakeTextFitCard(EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs));

                Log($"/meme >> font size: {FontSize:F2}", ConsoleColor.DarkYellow);

                var origin = GetTextOrigin(text, top, out var caseOffset);
                var options = GetDefaultTextOptions(origin, top);

                var pixelate = ExtraFonts.FontIsPixelated();
                var optionsE = new EmojiTool.Options(GetBrush(), GetEmojiSize(), _fontOffset, pixelate);
                var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs.AsQueue(), out _);
                var space = 0.25F * options.Font.Size * options.LineSpacing;
                var marginY = top ? _marginY - space : _h - _marginY - textLayer.Height + space;
                var x = _w.Gap(textLayer.Width);
                var y = marginY - caseOffset;
                var point = new Point(x.RoundInt(), y.RoundInt());
                background.Mutate(ctx => ctx.DrawImage(textLayer, point));
            }

            return (FontSize * GetLineSpacing() * text.GetLineCount(), FontSize);
        }

        private int GetEmojiSize() => (int)(FontSize * GetLineSpacing());

        private SolidBrush GetBrush() => ColorText ? RandomColor() : _white;

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