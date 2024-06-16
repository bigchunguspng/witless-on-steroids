using System;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Backrooms.Types;

namespace Witlesss.Services.Memes; // ReSharper disable InconsistentNaming

public class IFunnyApp
{
    public static bool PreferSegoe, UseLeftAlignment, ThinCard, UltraThinCard, WrapText = true;
    public static bool PickColor, ForceCenter, BackInBlack, BlurImage;
    public static int CropPercent = 100, MinFontSize = 10, FontSizeMultiplier = 10;
    public static CustomColorOption CustomColorOption;

    private Rgba32 Background;
    private SolidBrush TextColor = default!;
    private static readonly SolidBrush _white = new(Color.White), _black = new(Color.Black);
    private static readonly SolidBrush _transparent = new(Color.Transparent);

    private readonly EmojiTool _emojer = new() { MemeType = MemeType.Top };
    public  static readonly ExtraFonts ExtraFonts = new("top");
    private static readonly Regex CAPS = new(@"[A-ZА-Я0-9bdfhkltбф]");

    private int _w, _h; // <-- of the image
    private int _cardHeight, _fullHeight, _cropOffset, _textWidth;
    private float _marginLeft;
    private float _textOffset;
    private bool _extraHigh;
    private SizeF _measure;

    public static float FontSize => MathF.Round(_font.Size, 2);

    public Point     Location => new(0, _cardHeight);
    public Rectangle Cropping => new(0, _cropOffset, _w, _h);

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


    public string MakeCaptionMeme(string path, string text)
    {
        var (size, info) = GetImageSize(path);
        SetUp(size);

        var image = GetImage(path, size, info);

        var funny = DrawText(text);

        var meme = Combine(image, funny);

        return ImageSaver.SaveImage(meme, PngJpg.Replace(path, "-Top.jpg"));
    }

    private Image Combine(Image source, Image caption)
    {
        var meme = new Image<Rgba32>(_w, _fullHeight);

        if (Witlesss.Memes.Sticker) meme.Mutate(x => x.Fill(BackInBlack ? Color.Black : Background));

        meme.Mutate(x => x.DrawImage(source, new Point(0, _cardHeight - _cropOffset), opacity: 1));
        meme.Mutate(x => x.DrawImage(caption, new Point(0, 0), opacity: 1));

        return meme;
    }

    public string BakeText(string text) => ImageSaver.SaveImageTemp(Combine(new Image<Rgba32>(_w, _h), DrawText(text)));
    private Image DrawText(string text)
    {
        var emoji = EmojiRegex.Matches(text);
        var funny = emoji.Count > 0;
        var textM = funny ? EmojiTool.ReplaceEmoji(text, "aa" /*: "НН"*/) : text;

        AdjustProportions(textM);
        AdjustTextPosition(text);

        var image = funny ? null : new Image<Rgba32>(_w, _cardHeight, Background);

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
            image = new Image<Rgba32>(_w, _cardHeight, Background); // todo better branching
            image.Mutate(ctx => ctx.DrawImage(textLayer, point, opacity: 1));
        }
        else
        {
            Console.WriteLine(FontSize);
            image!.Mutate(x => x.DrawText(options, text, TextColor, pen: null));
        }

        return image!;
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


    // COLOR PICKING

    private void SetColor(Image<Rgba32> image)
    {
        if (CustomColorOption.IsActive) SetCustomColors();
        else if (PickColor) SetSpecialColors(image);
        else SetDefaultColors();
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

    private SolidBrush ChooseTextColor(Rgba32 b) => b.R * 0.4f + b.G * 0.5f + b.B * 0.1f > 100 ? _black : _white;

    private Rgba32 PickColorFromImage(Image<Rgba32> image)
    {
        var xd = ForceCenter ? 2 : 0;

        var colors = new Rgba32[7];
        colors[0] = AverageColorOnOffset(                  0);
        colors[1] = AverageColorOnOffset(image.Width * 1 / 8);
        colors[2] = AverageColorOnOffset(image.Width * 2 / 8);
        colors[3] = AverageColorOnOffset(image.Width * 4 / 8);
        colors[4] = AverageColorOnOffset(image.Width * 3 / 8);
        colors[5] = AverageColorOnOffset(image.Width * 7 / 8);
        colors[6] = AverageColorOnOffset(image.Width     - 5);

        var difference = new int[7 - xd * 2];
        for (var i = 0; i < colors.Length - xd * 2; i++)
        {
            difference[i] = colors[xd..^xd].Select(c => Difference(colors[i + xd], c)).OrderBy(x => x).Take(3).Sum();
        }

        var min = difference.Min();

        return min > 950 ? Average(colors[0], colors[^1]) : colors[difference.ToList().IndexOf(min) + xd];

        Rgba32 AverageColorOnOffset(int x)
        {
            var avg = AverageColor(image, new Rectangle(x, _cropOffset, 5, 5));
            return BackInBlack ? avg : PutOver(Color.White, avg);
        }
    }

    private static Rgba32 AverageColor(Image<Rgba32> image, Rectangle area)
    {
        int a = 0, r = 0, g = 0, b = 0;
        int w = area.Width, h = area.Height, s = w * h;
        int maxX = area.X + w, maxY = area.Y + h;

        for (var x = area.X; x < maxX; x++)
        for (var y = area.Y; y < maxY; y++)
        {
            var p = image[x, y];
            a += p.A;
            r += p.R;
            b += p.B;
            g += p.G;
        }

        return new Rgba32((r / s).ClampByte(), (g / s).ClampByte(), (b / s).ClampByte(), (a / s).ClampByte());
    }

    private static int Difference(Rgba32 a, Rgba32 b)
    {
        return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
    }

    private static Rgba32 Average(Rgba32 a, Rgba32 b)
    {
        return new Rgba32(Calc(a.R, b.R), Calc(a.G, b.G), Calc(a.B, b.B));

        byte Calc(byte x, byte y) => ((x + y) / 2).ClampByte();
    }

    private static Rgba32 PutOver(Rgba32 a, Rgba32 b)
    {
        return new Rgba32(Calc(a.R, b.R), Calc(a.G, b.G), Calc(a.B, b.B));

        byte Calc(byte x, byte y) => (x * (255 - b.A) / 255 + y * b.A / 255).ClampByte(); // lerp
    }
}

//public record TextParams(int Lines, int EmojiS, Font Font, SolidBrush Color, RectangleF Layout, StringFormat Format) : TextParameters;