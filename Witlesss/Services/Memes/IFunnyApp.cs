using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Witlesss.MediaTools;
using static System.Drawing.StringAlignment;

namespace Witlesss.Services.Memes; // ReSharper disable InconsistentNaming

public class IFunnyApp
{
    public static bool UseRoboto, UseSegoe, UseLeftAlignment, MinimizeHeight, WrapText = true;
    public static bool PickColor, ForceCenter, UseGivenColor, BackInBlack, BlurImage;
    public static int CropPercent = 100, MinFontSize = 10, DefFontSize = 36;
    public static Color GivenColor;

    private Color Background;
    private SolidBrush TextColor;
    private static readonly SolidBrush _white = new(Color.White), _black = new(Color.Black);
    private static readonly SolidBrush _transparent = new(Color.Transparent);

    private readonly EmojiTool _emojer = new() { MemeType = MemeType.Top };
    private static readonly Regex CAPS = new(@"[A-ZА-Я0-9bdfhkltбф]");

    private int _w, _h, _t, _full, _ww, _crop_offset;
    private float _lm, _caps_fix;
    private bool _extra_high;
    private SizeF _measure;

    public static float FontSize => MathF.Round(_sans.Size, 2);

    public Point     Location => new(0, _t);
    public Rectangle Cropping => new(0, _crop_offset, _w, _h);

    static IFunnyApp()
    {
        _fonts.AddFontFile(Config.FontBold);
        _fonts.AddFontFile(Config.FontRegular);
    }

    private static readonly PrivateFontCollection _fonts = new();
    private static FontFamily FontFamily => UseSegoe ? SegoeBlack : _fonts.Families[UseRoboto ? 1 : 0];
    private static FontFamily SegoeBlack = new("Segoe UI Black");
    private static Font _sans;
    private static StringFormat Format => UseLeftAlignment ? _formatL : _formatC;
    private static readonly StringFormat _formatL = new() { Alignment = Near,   Trimming = StringTrimming.Word, LineAlignment = Far };
    private static readonly StringFormat _formatC = new() { Alignment = Center, Trimming = StringTrimming.Word, LineAlignment = Center };

    private void ResizeFont(float size) => _sans = new(FontFamily, size);

    private int  StartingFontSize() => Math.Max(Math.Max(DefFontSize, _t / 5 * DefFontSize / 36), MinFontSize);

    private void SetFontToDefault() => ResizeFont(StartingFontSize());
    private void DecreaseFontSize() => ResizeFont(_sans.Size * 0.8f);


    public string MakeCaptionMeme(string path, string text)
    {
        var image = GetImage(path);
        var funny = DrawText(text);

        return JpegCoder.SaveImage(Combine(image, funny), PngJpg.Replace(path, "-Top.jpg"));
    }

    private Image Combine(Image source, Image caption)
    {
        var meme = new Bitmap(_w, _full);
        using var g = Graphics.FromImage(meme);

        if (Witlesss.Memes.Sticker) g.Clear(BackInBlack ? Color.Black : Background);
        
        g.CompositingMode = CompositingMode.SourceOver;
        g.DrawImage(source,  new Point(0, _t - _crop_offset));
        g.DrawImage(caption, new Point(0, 0));

        return meme;
    }

    public string BakeText(string text) => JpegCoder.SaveImageTemp(Combine(new Bitmap(_w, _h), DrawText(text)));
    private Image DrawText(string text)
    {
        var emoji = EmojiRegex.Matches(text);
        var funny = emoji.Count > 0;
        var textM = funny ? EmojiTool.ReplaceEmoji(text, UseRoboto ? "aa" : "НН") : text;

        AdjustProportions(textM);
        AdjustTextPosition(text);

        var top = funny ? _t * 2 : _t;

        var area = new RectangleF(_lm, _caps_fix, _ww, top);

        var image = new Bitmap(_w, top);
        using var graphics = Graphics.FromImage(image);
        
        graphics.Clear(Background);
        
        graphics.CompositingMode    = CompositingMode.SourceOver;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.PixelOffsetMode    = PixelOffsetMode.HighQuality;
        graphics.TextRenderingHint  = TextRenderingHint.AntiAlias;

        if (funny)
        {
            var p = new TextParams(62, EmojiSize, _sans, TextColor, area, Format);
            var h = (int)graphics.MeasureString(textM, _sans, area.Size, Format, out _, out var lines).Height;
            var l = _emojer.DrawTextAndEmoji(graphics, text, emoji, p, InitialMargin(h), Spacing);
            var d = _t - h;
            SetCardHeight(h * l / lines + d);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.FillRectangle(_transparent, 0, _t, _w, top - _t);
        }
        else graphics.DrawString(text, _sans, TextColor, area, Format);

        return image;
    }
    
    private int InitialMargin(int h) => UseLeftAlignment && !_extra_high ? _t - h : (_t - h) / 2;
    private int Spacing   => (int)(_sans.Size * 1.6);
    private int EmojiSize => (int)(_sans.Size * 1.5);

    private void AdjustProportions(string text)
    {
        _extra_high = false;

        using var g = Graphics.FromHwnd(IntPtr.Zero);

        if (UseLeftAlignment)
        {
            _lm = Math.Min(_sans.Size / 3, 5);
            _ww = _w - (int)(2 * _lm);
        }

        var area = new SizeF(WrapText ? _ww : _ww * 3, _h * 5);

        MeasureString();
        while (_measure.Height > _t || _measure.Width > _ww) // fixes "text is too big"
        {
            DecreaseFontSize();
            MeasureString();
            if (WrapText && _sans.Size < MinFontSize)
            {
                ResizeFont(MinFontSize);
                MeasureString();
                SetCardHeightXD(_measure.Height + 15);
                
                break;
            }
        }

        void MeasureString() => _measure = g.MeasureString(text, _sans, area);
        void SetCardHeightXD(float x)
        {
            SetCardHeight((int)x);
            _extra_high = true;
        }
        
        if (text.Count(c => c == '\n') > 2) // fixes "text is too small"
        {
            var ms = g.MeasureString(text, _sans, new SizeF(_ww, _t));
            if (ms.Width < _w * 0.9)
            {
                var k = 0.9f * _w / ms.Width;
                ResizeFont(_sans.Size * k);
                _measure = _measure * k;

                if (_measure.Height > _t) SetCardHeightXD(_measure.Height + (_w - _measure.Width) / 2);
            }
        }
        
        if (MinimizeHeight && _measure.Height < 0.95 * _t) SetCardHeight((int)(_measure.Height + Math.Min(_sans.Size, 8)));
    }

    private void AdjustTextPosition(string s)
    {
        var b = !UseLeftAlignment && CAPS.Matches(s).Count + EmojiRegex.Matches(s).Sum(m => m.Length) * 3 > s.Length / 5;

        _caps_fix = b ? _sans.Size * 0.0875f : 0;
    }

    public void SetUp(Size size)
    {
        _w = size.Width;
        _h = FF_Extensions.ToEven(size.Height * Math.Abs(CropPercent) / 100);

        _crop_offset = CropPercent < 0 ? (size.Height - _h) / 2 : size.Height - _h;

        SetCardHeight(_w > _h ? _w / _h > 7 ? _w / 7 : _h / 2 : _w / 2);

        _lm = 0;
        _ww = _w;

        SetFontToDefault();
    }
    private void SetCardHeight(int x)
    {
        _t = FF_Extensions.ToEven(x);
        _full = _h + _t;
    }
    private Image GetImage(string path)
    {
        var pic = Image.FromFile(path);
        var image = pic.Width < 200 ? new Bitmap(pic, new Size(200, pic.Height * 200 / pic.Width)) : new Bitmap(pic);

        SetUp(image.Size);
        SetColor(image);

        return image;
    }

    #region COLOR PICKING

    private void SetColor(Bitmap image)
    {
        if  (UseGivenColor) SetCustomColors();
        else if (PickColor) SetSpecialColors(image);
        else                SetDefaultColors();
    }

    public void SetSpecialColors(Bitmap image)
    {
        Background = PickColorFromImage(image);
        TextColor  = ChooseTextColor(Background);
    }
    public void SetCustomColors()
    {
        Background = GivenColor;
        TextColor  = ChooseTextColor(Background);
    }
    public void SetDefaultColors()
    {
        Background = Color.White;
        TextColor  = _black;
    }

    private SolidBrush ChooseTextColor(Color b) => b.R * 0.299f + b.G * 0.587f + b.B * 0.114f > 186 ? _black : _white;

    private Color PickColorFromImage(Bitmap image)
    {
        var xd = ForceCenter ? 2 : 0;

        var colors = new Color[7];
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

        Color AverageColorOnOffset(int x)
        {
            var avg = AverageColor(image, new Rectangle(x, _crop_offset, 5, 5));
            return BackInBlack ? avg : PutOver(Color.White, avg);
        }
    }

    private static Color AverageColor(Bitmap image, Rectangle area)
    {
        int a = 0, r = 0, g = 0, b = 0;
        int w = area.Width, h = area.Height, s = w * h;
        int maxX = area.X + w, maxY = area.Y + h;

        for (var x = area.X; x < maxX; x++)
        for (var y = area.Y; y < maxY; y++)
        {
            var p = image.GetPixel(x, y);
            a += p.A;
            r += p.R;
            b += p.B;
            g += p.G;
        }

        return Color.FromArgb(a / s, r / s, g / s, b / s);
    }

    private static int Difference(Color a, Color b)
    {
        return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
    }

    private static Color Average(Color a, Color b)
    {
        return Color.FromArgb(Calc(a.R, b.R), Calc(a.G, b.G), Calc(a.B, b.B));

        int Calc(byte x, byte y) => (x + y) / 2;
    }

    private static Color PutOver(Color a, Color b)
    {
        return Color.FromArgb(Calc(a.R, b.R), Calc(a.G, b.G), Calc(a.B, b.B));

        int Calc(byte x, byte y) => x * (255 - b.A) / 255 + y * b.A / 255; // lerp
    }

    #endregion
}

public record TextParams(int Lines, int EmojiS, Font Font, SolidBrush Color, RectangleF Layout, StringFormat Format) : TextParameters;