using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Witlesss.MediaTools;
using static System.Drawing.StringAlignment;

namespace Witlesss; // ReSharper disable InconsistentNaming

public class IFunnyApp
{
    public static bool UseRegularFont, UseLeftAlignment, MinimizeHeight, WrapText = true, PickColor;
    public static int  CropPercent = 0, MinFontSize = 10;

    private Color Background;
    private SolidBrush TextColor;
    private static readonly SolidBrush _white = new(Color.White), _black = new(Color.Black);
    private static readonly SolidBrush _transparent = new(Color.Transparent);

    private readonly EmojiTool _emojer = new() { MemeType = MemeType.Top };
    private static readonly Regex CAPS = new(@"[A-ZА-Я0-9bdfhkltбф]");

    private int _w, _h, _t, _full, _crop_offset;
    private float _lm, _caps_fix;
    private SizeF _measure;

    public Point     Location => new(0, _t);
    public Rectangle Cropping => new(0, _crop_offset, _w, _h);

    static IFunnyApp()
    {
        _fonts.AddFontFile(Config.FontBold);
        _fonts.AddFontFile(Config.FontRegular);
    }

    private static readonly PrivateFontCollection _fonts = new();
    private static FontFamily FontFamily => _fonts.Families[UseRegularFont ? 1 : 0];
    private static Font _sans;
    private static StringFormat Format => UseLeftAlignment ? _formatL : _formatC;
    private static readonly StringFormat _formatL = new() { Alignment = Near,   Trimming = StringTrimming.Word, LineAlignment = Far };
    private static readonly StringFormat _formatC = new() { Alignment = Center, Trimming = StringTrimming.Word, LineAlignment = Center };

    private void ResizeFont(float size) => _sans = new(FontFamily, size);

    private int  StartingFontSize() => Math.Max(Math.Max(36, _t / 5), MinFontSize);

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
        var textM = funny ? EmojiTool.ReplaceEmoji(text, UseRegularFont ? "aa" : "НН") : text;

        AdjustProportions(textM);
        AdjustTextPosition(text);

        var top = funny ? _t * 2 : _t;

        var area = new RectangleF(_lm, _caps_fix, _w, top);

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
    
    private int InitialMargin(int h) => (_t - h) / 2;
    private int Spacing   => (int)(_sans.Size * 1.6);
    private int EmojiSize => (int)(_sans.Size * 1.5);

    private void AdjustProportions(string text)
    {
        using var g = Graphics.FromHwnd(IntPtr.Zero);

        var area = new SizeF(WrapText ? _w : _w * 3, _h * 5);

        MeasureString();
        while (_measure.Height > _t || _measure.Width > _w) // fixes "text is too big"
        {
            DecreaseFontSize();
            MeasureString();
            if (WrapText && _sans.Size < MinFontSize)
            {
                ResizeFont(MinFontSize);
                MeasureString();
                SetCardHeight((int)_measure.Height + 15);
                
                break;
            }
        }

        void MeasureString() => _measure = g.MeasureString(text, _sans, area);
        
        if (text.Count(c => c == '\n') > 2) // fixes "text is too small"
        {
            var ms = g.MeasureString(text, _sans, new SizeF(_w, _t));
            if (ms.Width < _w * 0.9)
            {
                var k = 0.9f * _w / ms.Width;
                ResizeFont(_sans.Size * k);
                _measure = _measure * k;

                if (_measure.Height > _t) SetCardHeight((int)(_measure.Height + (_w - _measure.Width) / 2));
            }
        }
        
        if (MinimizeHeight && _measure.Height < 0.95 * _t) SetCardHeight((int)(_measure.Height + Math.Min(_sans.Size, 8)));

        if (UseLeftAlignment) _lm = Math.Min(_sans.Size / 3, _w - _measure.Width);
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
        if (PickColor) SetSpecialColors(image);
        else           SetDefaultColors();
    }

    public void SetSpecialColors(Bitmap image)
    {
        Background = AverageColor(image, new Rectangle(_w / 2, _crop_offset, 5, 5));
        TextColor  = ChooseTextColor(Background);
    }

    public void SetDefaultColors()
    {
        Background = Color.White;
        TextColor  = _black;
    }

    private SolidBrush ChooseTextColor(Color b) => b.R * 0.299f + b.G * 0.587f + b.B * 0.114f > 186 ? _black : _white;

    private static Color AverageColor(Bitmap image, Rectangle where)
    {
        int r = 0, g = 0, b = 0;
        int w = where.Width, h = where.Height, s = w * h;
        int maxX = where.X + w, maxY = where.Y + h;

        for (var x = where.X; x < maxX; x++)
        for (var y = where.Y; y < maxY; y++)
        {
            var p = image.GetPixel(x, y);
            r += p.R;
            b += p.B;
            g += p.G;
        }

        return Color.FromArgb(r / s, g / s, b / s);
    }

    #endregion
}

public record TextParams(int Lines, int EmojiS, Font Font, SolidBrush Color, RectangleF Layout, StringFormat Format) : TextParameters;