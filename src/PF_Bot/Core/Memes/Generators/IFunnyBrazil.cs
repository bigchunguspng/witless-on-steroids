using ColorHelper;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Editing;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using PF_Tools.FFMpeg;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Core.Memes.Generators; // ReSharper disable InconsistentNaming

public struct MemeOptions_Top()
{
    public FontOption FontOption;

    /// Background color of caption card.
    public ColorOption CustomColor;

    public int MinFontSizeMultiplier =  10;
    public int    FontSizeMultiplier = 100; // todo float ?

    /// Percent of image height to crop.
    /// If positive, only top is cropped.
    /// If negative, both top and bottom are cropped.
    public sbyte CropPercent = 0;

    public bool WrapText = true;
    public bool TextLeftAlignment;
    /// Make caption card as tall as text + some margin.
    public bool      ThinCard;
    /// Make caption card just as tall as text. <br/>
    /// Matters only when <see cref="ThinCard"/> is true.
    public bool UltraThinCard;
    /// Pick card background color from image top side.
    public bool PickColor;
    /// Pick card background color from image top center. <br/>
    /// Matters only when <see cref="PickColor"/> is true.
    public bool PickColor_FromCenter;
    /// Fill sticker background with black (default is white).
    public bool BackInBlack;
}

public partial class IFunnyBrazil(MemeOptions_Top op) : MemeGeneratorBase, IMemeGenerator<string>
{
    // SIZE

    private int _w, _h; // <-- of the image
    private int _cardHeight, _fullHeight, _cropOffset;
    private float _marginLeft;

    private Point     Location => new(0, _cardHeight);
    private Rectangle Cropping => new(0, _cropOffset, _w, _h);


    // LOGIC

    public async Task GenerateMeme(MemeFileRequest request, string text)
    {
        await FetchImageSize(request);
        SetUp();

        using var image = await GetImage(request.SourcePath);

        SetColor(image);

        using var card = DrawText(text);
        using var meme = Combine(image, card, sticker: request.IsSticker);

        meme.ApplyPressure(request.Press);

        await ImageSaver.SaveImageJpeg(meme, request.TargetPath, request.Quality);
    }

    public async Task GenerateVideoMeme(MemeFileRequest request, string text)
    {
        await FetchVideoSize(request);
        SetUp();
        SetColor(op.PickColor || op.CustomColor.ByCoords ? await request.GetVideoSnapshot() : null);

        using var card = DrawText(text);
        using var frame = Combine(null, card);
        var frameAsFile = await ImageSaver.SaveImageTemp(frame);

        var probe = await request.ProbeSource();
        await new FFMpeg_Meme(probe, request, frameAsFile)
            .Top(_sourceSizeAdjusted, Cropping, Location)
            .FFMpeg_Run();
    }

    private void SetUp()
    {
        var crop = (100F - Math.Abs(op.CropPercent)) / 100F;

        _w =  _sourceSizeAdjusted.Width;
        _h = (_sourceSizeAdjusted.Height * crop).RoundInt().ToEven();

        _marginLeft = 0.025F * _w;
        _cropOffset = _sourceSizeAdjusted.Height - _h;
        if (op.CropPercent < 0) _cropOffset = _cropOffset / 2;

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

        if (sticker) meme.Mutate(x => x.Fill(op.BackInBlack ? Color.Black : Background));

        if (source is not null)
            meme.Mutate(x => x.DrawImage(source, new Point(0, _cardHeight - _cropOffset)));
        meme.Mutate(x => x.DrawImage(caption, new Point(0, 0)));

        return meme;
    }


    // DRAW TEXT

    private Image DrawText(string text)
    {
        var emoji = EmojiTool.FindEmoji(text);
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
            var optionsE = new EmojiTool.Options(TextBrush, _w, GetEmojiSize(), _fontOffset);
            var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs!.AsQueue());
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
        var fs = FontSize * op.FontOption.GetRelativeSize();
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