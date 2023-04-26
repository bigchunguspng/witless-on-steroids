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
    public static bool UseRegularFont = false, UseLeftAlignment = false, MinimizeHeight = false, WrapText = true;
    
    static IFunnyApp()
    {
        _fonts.AddFontFile(Config.FontBold);
        _fonts.AddFontFile(Config.FontRegular);
    }

    private static readonly Regex Ext = new("(.png)|(.jpg)");
    private static readonly PrivateFontCollection _fonts = new();
    private static FontFamily FontFamily => _fonts.Families[UseRegularFont ? 1 : 0]; // "Segoe UI Black";
    private static Font _sans;
    private static readonly SolidBrush TextColor = new(Color.Black);
    private static StringFormat Format => UseLeftAlignment ? _formatL : _formatC;
    private static readonly StringFormat _formatL = new() { Alignment = Near,   Trimming = StringTrimming.Word, LineAlignment = Far };
    private static readonly StringFormat _formatC = new() { Alignment = Center, Trimming = StringTrimming.Word, LineAlignment = Center };

    private void ResizeFont(float size) => _sans = new(FontFamily, size);

    private void SetFontToDefault() => ResizeFont(StartingFontSize());
    private void MakeFontSmaller () => ResizeFont(_sans.Size * 0.8f);

    private int StartingFontSize () => Math.Max(36, _t / 5);

    private int _w, _h, _t, _full;
    private float _lm;

    public Rectangle Cropping => new(0, _t, _w, _h);

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
        g.DrawImage(caption, new Point(0, 0));
        g.DrawImage(source,  new Point(0, _t));

        return meme;
    }

    public string BakeText(string text) => JpegCoder.SaveImageTemp(Combine(new Bitmap(_w,_h), DrawText(text)));
    private Image DrawText(string text)
    {
        if (UseRegularFont) text = MemeGenerator.RemoveEmoji(text); // todo drawing them instead

        AdjustProportions(text);

        var area = new RectangleF(_lm, 0, _w, _t);

        var image = new Bitmap(_w, _t);
        using var graphics = Graphics.FromImage(image);
        
        graphics.Clear(Color.White);
        
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
            if (WrapText && _sans.Size < 10)
            {
                ResizeFont(10);
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
        _h = size.Height;

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

        return image;
    }
    
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