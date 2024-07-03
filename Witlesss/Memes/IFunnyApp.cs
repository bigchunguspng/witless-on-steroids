using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Meme;
using Witlesss.MediaTools;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes; // ReSharper disable InconsistentNaming

public partial class IFunnyApp : IMemeGenerator<string>
{
    // OPTIONS

    public static bool PreferSegoe, UseLeftAlignment, ThinCard, UltraThinCard, WrapText = true;
    public static bool PickColor, ForceCenter, BackInBlack, BlurImage;
    public static int CropPercent = 0; // 0 - 100
    public static int MinFontSize = 10, FontSizeMultiplier = 10;
    public static CustomColorOption CustomColorOption;

    // DATA

    private Rgba32 Background;
    private SolidBrush TextColor = default!;
    private static readonly SolidBrush _white = new(Color.White), _black = new(Color.Black);

    // ...

    private readonly EmojiTool _emojer = new() { MemeType = MemeType.Top };
    public  static readonly ExtraFonts ExtraFonts = new("top");
    private static readonly Regex CAPS = new(@"[A-ZА-Я0-9bdfhkltбф]");

    // SIZE

    private int _w, _h; // <-- of the image
    private int _cardHeight, _fullHeight, _cropOffset, _textWidth;
    private float _marginLeft;
    private float _textOffset;
    //private bool _extraHigh; // <-- watahell?
    private SizeF _measure;

    private Point     Location => new(0, _cardHeight);
    private Rectangle Cropping => new(0, _cropOffset, _w, _h);

    // FONT

    public static float FontSizeRounded => MathF.Round(_font.Size, 2);

    private static Font _font = default!;
    private static FontFamily FontFamily => ExtraFonts.GetFontFamily(PreferSegoe ? "sg" : "ft");
    private static FontStyle  FontStyle  => ExtraFonts.GetFontStyle(FontFamily);


    private void ResizeFont(float size) => _font = FontFamily.CreateFont(size, FontStyle);

    private float StartingFontSize()
    {
        var multiplier = FontSizeMultiplier / 10F;
        return Math.Max(Math.Max(48, _cardHeight / 3.75F) * multiplier, MinFontSize) * ExtraFonts.GetSizeMultiplier();
    }

    private void SetFontToDefault() => ResizeFont(StartingFontSize());
    private void DecreaseFontSize() => ResizeFont(_font.Size * 0.8f);


    // LOGIC

    private Size _sourceSizeOG, _sourceSizeAdjusted;

    public string GenerateMeme(MemeFileRequest request, string text)
    {
        _sourceSizeOG = GetImageSize(request.SourcePath);
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

    private Size GetImageSize(string path) => Image.Identify(path).Size;

    private Size AdjustImageSize() => _sourceSizeOG.EnureIsWideEnough().FitSize(new Size(1280, 1080));

    private void SetUp()
    {
        var crop = Math.Abs(CropPercent) / 100F;

        _w = _sourceSizeAdjusted.Width;
        _h = (_sourceSizeAdjusted.Height * (1 - crop)).RoundInt().ToEven();

        _cropOffset = _sourceSizeAdjusted.Height - _h;
        if (CropPercent < 0) _cropOffset = _cropOffset / 2;

        var ratio = _sourceSizeAdjusted.AspectRatio();
        var cardHeight = ratio > 1D
            ? ratio > 3.5D
                ? _w / 7
                : _h / 2
            : _w / 2;

        SetCardHeight(cardHeight);

        _marginLeft = 0.025F * _w;
        _textWidth = _w; // todo make it 0.98 * _w

        SetFontToDefault();
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
        // text with random emojis
        // all emojis replaced with 👌 for proper measurement
        // text is broken with several \n's to be drawn like that
        // all emojis replaced back | text is built again with og emojis:
        
        // text🔥 -> emoji matches -> text👌 -> chunks -> text\n👌 -> emoji matches 2 -> text\n🔥

        var emoji = EmojiRegex.Matches(text);
        var funny = emoji.Count > 0;
        var textM = funny ? EmojiTool.ReplaceEmoji(text, "👌") : text;

        var textA = AdjustProportions(textM);
        AdjustTextPosition(textA);

        Image<Rgba32> image;

        var options = GetDefaultTextOptions();
        if (funny)
        {
            var heightExpected = (int)TextMeasuring.MeasureTextSize(textM, options, out var linesExpected).Height;
            var parameters = new EmojiTool.Options(TextColor, EmojiSize);
            var textLayer = _emojer.DrawEmojiText(textA, options, parameters, out var linesActual);

            SetCardHeight(heightExpected * linesActual / linesExpected + _cardHeight - heightExpected);

            var x = UseLeftAlignment ? _marginLeft : _w.Gap(textLayer.Width);
            var y = _cardHeight.Gap(textLayer.Height) + _textOffset;
            var point = new Point(x.RoundInt(), y.RoundInt());

            image = CreateBackgroundCard();
            image.Mutate(ctx => ctx.DrawImage(textLayer, point));
        }
        else
        {
            Console.WriteLine(FontSizeRounded);

            image = CreateBackgroundCard();
            image.Mutate(x => x.DrawText(options, textA, TextColor, pen: null));
        }

        return image;
        
        Image<Rgba32> CreateBackgroundCard() => new(_w, _cardHeight, Background);
    }
    
    private int InitialMargin(int h) => /*UseLeftAlignment && !_extraHigh ? _cardHeight - h : */(_cardHeight - h) / 2;
    private int EmojiSize => (int)(_font.Size * GetLineSpacing());

    private string AdjustProportions(string text) // GetTextToDraw / MakeTextFitCard
    {
        // return: text split by \n in right places (if needed)
        // side effects: font size and card height changed

        var textChunks = TextMeasuring.MeasureTextSuperCool(text, GetDefaultTextOptions(), EmojiSize);
        
        var lineHeight = _font.Size * GetLineSpacing();
        var textWidthLimit = 0.9F * _w;

        if (text.Contains('\n') || !WrapText)
        {
            var k = 1F;

            var maxLineWidth = textChunks.GetMaxLineWidth();
            if (maxLineWidth > textWidthLimit)
            {
                k = textWidthLimit / maxLineWidth;
            }

            var textHeight = lineHeight * text.GetLineCount();
            if (textHeight * k > _cardHeight || ThinCard)
            {
                SetCardHeightLol(textHeight, k);
            }

            ResizeFont(_font.Size * k);

            return text;
        }
        else
        {
            var textWidth = textChunks.Sum(x => x.Width);
            if (textWidth < textWidthLimit)
            {
                if (ThinCard) SetCardHeightLol(lineHeight, 1F);
                return text;
            }

            var k = 1F;

            var maxWordWidth = textChunks.Where(x => x is { Type: CharType.Text, Length: <= 25 }).Max(x => x.Width);
            if (maxWordWidth > textWidthLimit)
            {
                k = textWidthLimit / maxWordWidth;
            }

            var fontRatio = MinFontSize / _font.Size;
            var lineH = fontRatio * lineHeight;
            var lineC = fontRatio * textWidth / textWidthLimit;
            var minRatio = textWidthLimit / (lineH * lineC);

            var lineCount = 2;
            while (true)
            {
                var textRatio = (textWidth / lineCount) / (lineHeight * lineCount);
                var targetRatio = Math.Min(minRatio, textWidthLimit / (_cardHeight * Math.Min(lineCount, 4) / 6F));
                if (textRatio < targetRatio) break;

                lineCount++;
            }

            TextMeasuring.DistributeText(textChunks, lineCount); // lineCount: 2+
            var distributedText = textChunks.FillWith(text);

            var maxLineWidth = textChunks.GetMaxLineWidth();
            if (maxLineWidth * k > textWidthLimit)
            {
                k = textWidthLimit / maxLineWidth;
            }

            var textHeight = lineHeight * lineCount;
            if (textHeight * k > _cardHeight || ThinCard)
            {
                SetCardHeightLol(textHeight, k);
            }

            ResizeFont(_font.Size * k);

            return distributedText;
        }

        void SetCardHeightLol(float textHeight, float k)
        {
            var k2 = UltraThinCard ? 0.1F : 1F;
            var min = UltraThinCard ? 8 : 16;
            var extra = Math.Max(_font.Size * k * k2, min);
            SetCardHeight((textHeight * k + extra).CeilingInt());
        }
    }

    private void AdjustProportionsOld(string text) // todo change the algorithm
    {
        // increase card height only if text font is too small

        if (UseLeftAlignment)
        {
            _marginLeft = Math.Max(_w / 40F, 5);
            _textWidth = _w - (int)(2 * _marginLeft);
        }

        MeasureText();
        while (_measure.Height > _cardHeight || _measure.Width > _textWidth) // fixes "text is too big"
        {
            DecreaseFontSize();
            MeasureText();
            if (WrapText && _font.Size < MinFontSize)
            {
                ResizeFont(MinFontSize);
                MeasureText();
                SetCardExtraHeight(_measure.Height + 15);
                
                break;
            }
        }
        
        if (text.Count(c => c == '\n') > 2) // fixes "text is too small"
        {
            var ms = TextMeasuring.MeasureTextSize(text, GetDefaultTextOptions(), out _);
            if (ms.Width < _w * 0.9)
            {
                var k = 0.9f * _w / ms.Width;
                ResizeFont(_font.Size * k);
                _measure = _measure * k;

                if (_measure.Height > _cardHeight) SetCardExtraHeight(_measure.Height + (_w - _measure.Width) / 2);
            }
        }
        
        if (ThinCard && _measure.Height < 0.95 * _cardHeight)
        {
            var extraHeight = UltraThinCard ? _font.Size * 0.1F : Math.Max(_font.Size, 8);
            SetCardExtraHeight(_measure.Height + extraHeight);
        }

        void MeasureText()
        {
            var options = GetDefaultTextOptions();
            options.WrappingLength = WrapText ? _textWidth : -1;
            _measure = TextMeasuring.MeasureTextSize(text, options, out _);
        }

        void SetCardExtraHeight(float x) => SetCardHeight((int)x);
    }

    private void AdjustTextPosition(string s)
    {
        var offset = _font.Size * ExtraFonts.GetVerticalOffset();
        var caps = TextIsUppercaseEnough(s) ? _font.Size * 0.103F : 0; // todo different for every font

        _textOffset = offset;// offset + caps;
    }

    private bool TextIsUppercaseEnough(string s)
    {
        var caps  = CAPS.Matches(s).Count;
        var emoji = EmojiRegex.Matches(s).Sum(m => m.Length);
        return caps + 3 * emoji > s.Length / 5;
    }

    private RichTextOptions GetDefaultTextOptions() => new(_font)
    {
        TextAlignment = UseLeftAlignment
            ? TextAlignment.Start
            : TextAlignment.Center,
        HorizontalAlignment = UseLeftAlignment
            ? HorizontalAlignment.Left
            : HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Origin = GetTextOrigin(),
        WrappingLength = _textWidth,
        LineSpacing = GetLineSpacing(),
        WordBreaking = WordBreaking.Standard,
        KerningMode = KerningMode.Standard,
        FallbackFontFamilies = ExtraFonts.FallbackFamilies,
    };

    private float GetLineSpacing() => ExtraFonts.GetLineSpacing() * 1.2F;
    private PointF GetTextOrigin()
    {
        var x = UseLeftAlignment ? _marginLeft : _w / 2F;
        var y = _cardHeight / 2F + _textOffset;

        return new PointF(x, y);
    }


    // COLORS

    private void SetColor(Image<Rgba32>? image)
    {
        if      (CustomColorOption.IsActive) SetCustomColors();
        else if (PickColor && image != null) SetSpecialColors(image);
        else                                 SetDefaultColors();
    }

    private void SetSpecialColors(Image<Rgba32> image)
    {
        Background = PickColorFromImage(image);
        TextColor  = ChooseTextColor(Background);
    }
    private void SetCustomColors()
    {
        Background = CustomColorOption.Color;
        TextColor  = ChooseTextColor(Background);
    }
    private void SetDefaultColors()
    {
        Background = Color.White;
        TextColor  = _black;
    }

    private SolidBrush ChooseTextColor(Rgba32 b) => b.Rgb.BlackTextIsBetter() ? _black : _white;
}