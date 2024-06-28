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
    public static int CropPercent = 100, MinFontSize = 10, FontSizeMultiplier = 10;
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
    private bool _extraHigh;
    private SizeF _measure;

    public Point     Location => new(0, _cardHeight);
    public Rectangle Cropping => new(0, _cropOffset, _w, _h);

    // FONT

    public static float FontSize => MathF.Round(_font.Size, 2);

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

    public string GenerateMeme(MemeFileRequest request, string text)
    {
        var (size, info) = GetImageSize(request.SourcePath);
        SetUp(size);

        using var image = GetImage(request.SourcePath, size, info);
        SetColor(image);

        using var card = DrawText(text);
        using var meme = Combine(image, card, sticker: request.IsSticker);

        return ImageSaver.SaveImage(meme, request.TargetPath, request.Quality);
    }

    public Task<string> GenerateVideoMeme(MemeFileRequest request, string text)
    {
        var size = FFMpegXD.GetPictureSize(request.SourcePath).GrowSize();
        SetUp(size);
        SetColor(PickColor ? request.GetVideoSnapshot() : null);

        using var card = DrawText(text);
        using var frame = Combine(null, card);
        var frameAsFile = ImageSaver.SaveImageTemp(frame);

        return new F_Combine(request.SourcePath, frameAsFile)
            .When(request.GetCRF(), size, Cropping, Location, BlurImage)
            .OutputAs(request.TargetPath);
    }

    public void SetUp(Size size)
    {
        var crop = Math.Abs(CropPercent) / 100F;

        _w = size.Width;
        _h = (size.Height * crop).RoundInt().ToEven();

        _cropOffset = size.Height - _h;
        if (CropPercent < 0) _cropOffset = _cropOffset / 2;

        var wide = _w > _h;
        var cardHeight = wide
            ? _w / _h > 7
                ? _w / 7
                : _h / 2
            : _w / 2;

        SetCardHeight(cardHeight);

        _marginLeft = 0;
        _textWidth = _w; // todo make it 0.98 * _w

        SetFontToDefault();
    }

    private void SetCardHeight(int x)
    {
        _cardHeight = x.ToEven();
        _fullHeight = _h + _cardHeight;
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
        var funny = emoji.Count > 0;
        var textM = funny ? EmojiTool.ReplaceEmoji(text, "aa" /*: "НН"*/) : text;

        AdjustProportions(textM);
        AdjustTextPosition(text);

        Image<Rgba32> image;

        var options = GetDefaultTextOptions();
        if (funny)
        {
            var heightExpected = (int)TextMeasuring.MeasureTextSize(textM, options, out var linesExpected).Height;
            var parameters = new EmojiTool.Options(TextColor, EmojiSize);
            var textLayer = _emojer.DrawEmojiText(text, options, parameters, out var linesActual);

            SetCardHeight(heightExpected * linesActual / linesExpected + _cardHeight - heightExpected);

            var x = UseLeftAlignment ? _marginLeft : (_w - textLayer.Width) / 2F;
            var y = (_cardHeight - textLayer.Height) / 2F + _textOffset;
            var point = new Point(x.RoundInt(), y.RoundInt());
            image = GetBackground();
            image.Mutate(ctx => ctx.DrawImage(textLayer, point));
        }
        else
        {
            Console.WriteLine(FontSize);
            image = GetBackground();
            image.Mutate(x => x.DrawText(options, text, TextColor, pen: null));
        }

        return image;
        
        Image<Rgba32> GetBackground() => new(_w, _cardHeight, Background);
    }
    
    private int InitialMargin(int h) => UseLeftAlignment && !_extraHigh ? _cardHeight - h : (_cardHeight - h) / 2;
    private int Spacing   => (int)(_font.Size * 1.6);
    private int EmojiSize => (int)(_font.Size * ExtraFonts.GetLineSpacing() * 1.2F);

    private void AdjustProportions(string text) // todo change the algorithm
    {
        _extraHigh = false;

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
                SetCardHeightXD(_measure.Height + 15);
                
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

                if (_measure.Height > _cardHeight) SetCardHeightXD(_measure.Height + (_w - _measure.Width) / 2);
            }
        }
        
        if (ThinCard && _measure.Height < 0.95 * _cardHeight)
        {
            var extraHeight = UltraThinCard ? _font.Size * 0.1F : Math.Max(_font.Size, 8);
            SetCardHeight((int)(_measure.Height + extraHeight));
        }

        void MeasureText()
        {
            var options = GetDefaultTextOptions();
            options.WrappingLength = WrapText ? _textWidth : -1;
            _measure = TextMeasuring.MeasureTextSize(text, options, out _);
        }

        void SetCardHeightXD(float x)
        {
            SetCardHeight((int)x);
            _extraHigh = true;
        }
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
        LineSpacing = ExtraFonts.GetLineSpacing() * 1.2F,
        WordBreaking = WordBreaking.Standard,
        KerningMode = KerningMode.Standard,
        FallbackFontFamilies = ExtraFonts.FallbackFamilies,
    };

    private PointF GetTextOrigin()
    {
        var x = UseLeftAlignment ? _marginLeft : _w / 2F;
        var y = _cardHeight / 2F + _textOffset;

        return new PointF(x, y);
    }


    // IMAGE

    private (Size size, ImageInfo info) GetImageSize(string path)
    {
        var info = Image.Identify(path);
        return (info.Size.EnureIsWideEnough(), info);
    }

    private Image<Rgba32> GetImage(string path, Size size, ImageInfo info)
    {
        var image = Image.Load<Rgba32>(path);
        if (size != info.Size)
        {
            image.Mutate(x => x.Resize(size));
        }

        SetColor(image);

        return image;
    }


    // COLORS

    private void SetColor(Image<Rgba32>? image)
    {
        if      (CustomColorOption.IsActive) SetCustomColors();
        else if (PickColor && image != null) SetSpecialColors(image);
        else                                 SetDefaultColors();
    }

    public void SetSpecialColors(Image<Rgba32> image)
    {
        Background = PickColorFromImage(image);
        TextColor  = ChooseTextColor(Background);
    }
    public void SetCustomColors()
    {
        Background = CustomColorOption.Color;
        TextColor  = ChooseTextColor(Background);
    }
    public void SetDefaultColors()
    {
        Background = Color.White;
        TextColor  = _black;
    }

    private SolidBrush ChooseTextColor(Rgba32 b) => b.Rgb.BlackTextIsBetter() ? _black : _white;
}