using ColorHelper;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes; // ReSharper disable InconsistentNaming

public partial class IFunnyApp : MemeGeneratorBase, IMemeGenerator<string>
{
    // OPTIONS

    public static bool PreferSegoe, UseLeftAlignment, ThinCard, UltraThinCard, WrapText = true;
    public static bool PickColor, ForceCenter, BackInBlack, BlurImage;
    public static int CropPercent = 0;
    public static int MinSizeMultiplier = 10, FontSizeMultiplier = 100;
    public static CustomColorOption CustomColor = new("#");

    // SIZE

    private int _w, _h; // <-- of the image
    private int _cardHeight, _fullHeight, _cropOffset;
    private float _marginLeft;

    private Point     Location => new(0, _cardHeight);
    private Rectangle Cropping => new(0, _cropOffset, _w, _h);


    // LOGIC

    public string GenerateMeme(MemeFileRequest request, string text)
    {
        FetchImageSize(request);
        SetUp();

        using var image = GetImage(request.SourcePath);

        SetColor(image);

        using var card = DrawText(text);
        using var meme = Combine(image, card, sticker: request.IsSticker);

        return ImageSaver.SaveImage(meme, request.TargetPath, request.Quality);
    }

    public Task<string> GenerateVideoMeme(MemeFileRequest request, string text)
    {
        FetchVideoSize(request);
        SetUp();
        SetColor(PickColor || CustomColor.ByCoords ? request.GetVideoSnapshot() : null);

        using var card = DrawText(text);
        using var frame = Combine(null, card);

        return request.UseFFMpeg()
            .When(VideoMemeRequest.From(request, frame), _sourceSizeAdjusted, Cropping, Location, BlurImage)
            .OutAs(request.TargetPath);
    }

    private void SetUp()
    {
        var crop = (100F - Math.Abs(CropPercent)) / 100F;

        _w =  _sourceSizeAdjusted.Width;
        _h = (_sourceSizeAdjusted.Height * crop).RoundInt().ToEven();

        _marginLeft = 0.025F * _w;
        _cropOffset = _sourceSizeAdjusted.Height - _h;
        if (CropPercent < 0) _cropOffset = _cropOffset / 2;

        var ratio = _sourceSizeAdjusted.AspectRatio();
        var cardHeight = ratio > 1D
            ? ratio > 3.5D
                ? _w / 7
                : _h / 2
            : _w / 2;

        SetCardHeight(cardHeight);

        SetUpFonts();
    }

    private void SetCardHeight(int x)
    {
        _cardHeight = x.ToEven();
        _fullHeight = _h + _cardHeight;
    }

    private Image Combine(Image? source, Image caption, bool sticker = false)
    {
        var meme = new Image<Rgba32>(_w, _fullHeight);

        if (sticker) meme.Mutate(x => x.Fill(BackInBlack ? Color.Black : Background));

        if (source is not null)
            meme.Mutate(x => x.DrawImage(source, new Point(0, _cardHeight - _cropOffset)));
        meme.Mutate(x => x.DrawImage(caption, new Point(0, 0)));

        return meme;
    }


    // DRAW TEXT

    private Image DrawText(string text)
    {
        var emoji = EmojiRegex.Matches(text);
        var plain = emoji.Count == 0;

        var pngs = plain ? null : EmojiTool.GetEmojiPngs(emoji);
        text = MakeTextFitCard(plain ? text : EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs));

        AdjustTextOffset(text);

        var image = new Image<Rgba32>(_w, _cardHeight, Background);
        var options = GetDefaultTextOptions();

        //Debug_Line(image, options);
        //Debug_Chars(image, options, text);

        if (plain)
        {
            options.WrappingLength = -1;
            image.Mutate(x => x.DrawText(options, text, TextBrush, pen: null));
        }
        else
        {
            var optionsE = new EmojiTool.Options(TextBrush, GetEmojiSize(), _fontOffset);
            var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs!.AsQueue(), out _);
            image.Mutate(ctx => ctx.DrawImage(textLayer, GetOriginFunny(textLayer.Size)));
        }

        return image;
    }


    // DEBUG

    private void Debug_Line(Image<Rgba32> image, RichTextOptions options)
    {
        // font size showed in pixels (different for different fonts)
        var rect1 = new RectangleF(0, options.Origin.Y - _textOffset - FontSize / 2F, _w, FontSize);
        image.Mutate(ctx => ctx.Fill(new SolidBrush(Color.OrangeRed), rect1));

        // actual font size (same for same pic*text)
        var w = _w * 0.75F;
        var x = _w.Gap(w.RoundInt());
        var fs = FontSize * ExtraFonts.GetRelativeSize();
        var rect2 = new RectangleF(x, options.Origin.Y - _textOffset - fs / 2F, w, fs);
        image.Mutate(ctx => ctx.Fill(new SolidBrush(Color.Orange), rect2));
    }

    private void Debug_Chars(Image<Rgba32> image, RichTextOptions options, string text)
    {
        TextMeasurer.TryMeasureCharacterBounds(text, options, out var bounds);
        foreach (var bound in bounds)
        {
            var color = ColorGenerator.GetLightRandomColor<RGB>().ToRgb24();
            var b = bound.Bounds;
            var rect = new RectangleF(b.X, b.Y, b.Width, b.Height);
            image.Mutate(x => x.Fill(new SolidBrush(color), rect));
        }
    }
}