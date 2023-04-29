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

    static IFunnyApp()
    {
        _fonts.AddFontFile(Config.FontBold);
        _fonts.AddFontFile(Config.FontRegular);
    }

    private static readonly Regex Ext = new("(.png)|(.jpg)");
    private static readonly PrivateFontCollection _fonts = new();
    private static FontFamily FontFamily => _fonts.Families[UseRegularFont ? 1 : 0]; // "Segoe UI Black";
    private static Font _sans;
    private static StringFormat Format => UseLeftAlignment ? _formatL : _formatC;
    private static readonly StringFormat _formatL = new() { Alignment = Near,   Trimming = StringTrimming.Word, LineAlignment = Far };
    private static readonly StringFormat _formatC = new() { Alignment = Center, Trimming = StringTrimming.Word, LineAlignment = Center };

    private void ResizeFont(float size) => _sans = new(FontFamily, size);

    private void SetFontToDefault() => ResizeFont(StartingFontSize());
    private void MakeFontSmaller () => ResizeFont(_sans.Size * 0.8f);

    private int StartingFontSize () => Math.Max(Math.Max(36, _t / 5), MinFontSize);

    private int _w, _h, _t, _full, _crop_offset;
    private float _lm;

    public Rectangle Cropping => new(0, _crop_offset, _w, _h);
    public Point     Location => new(0, _t); // overlay

    public string MakeTopTextMeme(string path, string text)
    {
        var image = GetImage(path);
        var funny = DrawText(text);

        return JpegCoder.SaveImage(Combine(image, funny), Ext.Replace(path, "-C.jpg"));
    }

    private Image Combine(Image source, Image caption)
    {
        var meme = new Bitmap(_w, _full);
        using var g = Graphics.FromImage(meme);
        
        g.CompositingMode = CompositingMode.SourceCopy;
        g.DrawImage(source,  new Point(0, _t - _crop_offset));
        g.DrawImage(caption, new Point(0, 0));

        return meme;
    }

    public string BakeText(string text) => JpegCoder.SaveImageTemp(Combine(new Bitmap(_w, _h), DrawText(text)));
    private Image DrawText(string text)
    {
        if (UseRegularFont) text = MemeGenerator.RemoveEmoji(text); // todo drawing them instead

        AdjustProportions(text);

        var area = new RectangleF(_lm, 0, _w, _t);

        var image = new Bitmap(_w, _t);
        using var graphics = Graphics.FromImage(image);
        
        graphics.Clear(Background);
        
        graphics.CompositingMode    = CompositingMode.SourceOver;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.PixelOffsetMode    = PixelOffsetMode.HighQuality;
        graphics.TextRenderingHint  = TextRenderingHint.AntiAlias;
        graphics.DrawString(text, _sans, TextColor, area, Format);

        return image;
    }

    private void AdjustProportions(string text)
    {
        using var g = Graphics.FromHwnd(IntPtr.Zero);

        var area = new SizeF(WrapText ? _w : _w * 3, _h * 5);
        var sure = MeasureString();
        while (sure.Height > _t || sure.Width > _w) // fixes "text is too big"
        {
            MakeFontSmaller();
            sure = MeasureString();
            if (WrapText && _sans.Size < MinFontSize)
            {
                ResizeFont(MinFontSize);
                sure = MeasureString();
                SetCardHeight((int)sure.Height + 15);
                
                break;
            }
        }

        SizeF MeasureString() => g.MeasureString(text, _sans, area);
        
        if (text.Count(c => c == '\n') > 2) // fixes "text is too small"
        {
            var ms = g.MeasureString(text, _sans, new SizeF(_w, _t));
            if (ms.Width < _w * 0.9)
            {
                var k = 0.9f * _w / ms.Width;
                ResizeFont(_sans.Size * k);
                sure = sure * k;

                var m = (_w - sure.Width) / 2; // kinda top-bottom margin
                if (sure.Height > _t) SetCardHeight((int)(sure.Height + m));
            }
        }
        
        if (MinimizeHeight && sure.Height < 0.95 * _t) SetCardHeight((int)(sure.Height + Math.Min(_sans.Size, 8)));

        if (UseLeftAlignment) _lm = Math.Min(_sans.Size / 3, _w - sure.Width);
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
        var image = new Bitmap(Image.FromFile(path));
        if (image.Width < 200)
        {
            image = new Bitmap(Image.FromFile(path), new Size(200, image.Height * 200 / image.Width));
        }

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

    /*private Image DrawFrame(string text, Image image) // nooooooo u can't commit commented junkyard
    {
        using var graphics = Graphics.FromImage(image); // HAHA, I WILL USE IT LATER :gigachad:

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        
        TextParameters tp = new TextParameters{
            Font   = new Font("Comic Sans", 20),
            Lines  = 4,
            EmojiS = 30,
            Color  = new SolidBrush(Color.White),
            Layout = new RectangleF(0, margin, width, 100),
            Format = new StringFormat(StringFormatFlags.NoWrap) {Alignment = Center, Trimming = Word}
        };

        DrawText(new DrawableText{G = graphics});
        //AddText(text.A, _s, graphics, _upper, new Rectangle(_m, _m, _w - 2 * _m, _h / 3 - _m));

        return image;
    }*/
    
    /*private void DrawText(DrawableText x)
    {
        var emoji = Regex.Matches(x.S, REGEX_EMOJI);
        if (emoji.Count > 0)
        {
            DrawTextAndEmoji(x.G, x.S, emoji, x.P);
        }
        else
        {
            x.G.DrawString(x.S, x.P.Font, x.P.Color, x.P.Layout, x.P.Format);
        }
    }*/
}