using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ColorHelper;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme;
using Witlesss.MediaTools;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes; // ReSharper disable InconsistentNaming

public partial class IFunnyApp : IMemeGenerator<string>
{
    private static readonly EmojiTool _emojer = new() { MemeType = MemeType.Top };

    // OPTIONS

    public static bool PreferSegoe, UseLeftAlignment, ThinCard, UltraThinCard, WrapText = true;
    public static bool PickColor, ForceCenter, BackInBlack, BlurImage;
    public static int CropPercent = 0; // 0 - 100
    public static int MinFontSize = 10, FontSizeMultiplier = 10;
    public static CustomColorOption CustomColorOption;

    // SIZE

    private Size _sourceSizeOG, _sourceSizeAdjusted;

    private int _w, _h; // <-- of the image
    private int _cardHeight, _fullHeight, _cropOffset;
    private float _marginLeft;
    private float _fontOffset, _caseOffset, _textOffset;

    private Point     Location => new(0, _cardHeight);
    private Rectangle Cropping => new(0, _cropOffset, _w, _h);


    // LOGIC

    public string GenerateMeme(MemeFileRequest request, string text)
    {
        _sourceSizeOG = Image.Identify(request.SourcePath).Size;
        _sourceSizeAdjusted = AdjustImageSize();

        SetUp();

        using var image = GetImage(request.SourcePath);
        SetColor(image);

        using var card = DrawText(text);
        using var meme = Combine(image, card, sticker: request.IsSticker);

        return ImageSaver.SaveImage(meme, request.TargetPath, request.Quality);
    }

    public Task<string> GenerateVideoMeme(MemeFileRequest request, string text)
    {
        _sourceSizeOG = FFMpegXD.GetPictureSize(request.SourcePath);
        _sourceSizeAdjusted = AdjustImageSize().ValidMp4Size();

        SetUp();
        SetColor(PickColor ? request.GetVideoSnapshot() : null);

        using var card = DrawText(text);
        using var frame = Combine(null, card);
        var frameAsFile = ImageSaver.SaveImageTemp(frame);

        return new F_Combine(request.SourcePath, frameAsFile)
            .When(request.GetCRF(), _sourceSizeAdjusted, Cropping, Location, BlurImage)
            .OutputAs(request.TargetPath);
    }

    private Size AdjustImageSize() => _sourceSizeOG.EnureIsWideEnough().FitSize(new Size(1280, 720));

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

    private Image<Rgba32> GetImage(string path)
    {
        var image = Image.Load<Rgba32>(path);
        var resize = _sourceSizeOG != _sourceSizeAdjusted;
        if (resize)
            image.Mutate(x => x.Resize(_sourceSizeAdjusted));

        return image;
    }

    private Image Combine(Image? source, Image caption, bool sticker = false)
    {
        var meme = new Image<Rgba32>(_w, _fullHeight);

        // todo if sticker and not send as sticker
        if (sticker) meme.Mutate(x => x.Fill(BackInBlack ? Color.Black : Background));

        if (source is not null)
            meme.Mutate(x => x.DrawImage(source, new Point(0, _cardHeight - _cropOffset)));
        meme.Mutate(x => x.DrawImage(caption, new Point(0, 0)));

        return meme;
    }

    private Image DrawText(string text)
    {
        var emoji = EmojiRegex.Matches(text);
        return emoji.Count == 0 
            ? DrawTextSimple(text) 
            : DrawTextFunny(text, emoji);
    }

    private Image DrawTextSimple(string text)
    {
        text = MakeTextFitCard(text);
        AdjustTextPosition(text);

        var options = GetDefaultTextOptions();
        var image = CreateBackgroundCard();

        //Debug_Line(image, options);
        //Debug_Chars(image, options, text);

        image.Mutate(x => x.DrawText(options, text, TextBrush, pen: null));
        return image;
    }

    private Image DrawTextFunny(string text, MatchCollection emoji)
    {
        var pngs = EmojiTool.GetEmojiPngs(emoji);

        text = MakeTextFitCard(EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs));
        AdjustTextPosition(text);

        var options = GetDefaultTextOptions();

        var parameters = new EmojiTool.Options(TextBrush, GetEmojiSize(), _fontOffset);
        var textLayer = _emojer.DrawEmojiText(text, options, parameters, pngs.AsQueue(), out _);

        var image = CreateBackgroundCard();

        //Debug_Line(image, options);
        //Debug_Chars(image, options, text);

        image.Mutate(ctx => ctx.DrawImage(textLayer, GetOriginFunny(textLayer.Size)));
        return image;
    }

    private Image<Rgba32> CreateBackgroundCard() => new(_w, _cardHeight, Background);

    private int GetEmojiSize() => (int)(FontSize * GetLineSpacing());

    private Point GetOriginFunny(Size textLayer)
    {
        var x = UseLeftAlignment ? _marginLeft : _w.Gap(textLayer.Width);
        var y = _cardHeight.Gap(textLayer.Height) + _caseOffset;
        return new Point(x.RoundInt(), y.RoundInt());
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