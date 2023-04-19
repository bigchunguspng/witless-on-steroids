using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using Witlesss.MediaTools;

namespace Witlesss; // Bahnschrift SemiBold Condensed // Segoe UI

public class TopTextAttacher //: MemeGenerator
{
    //static TopTextAttacher() { _fonts.AddFontFile(@"D:\Downloads\Telegram Desktop\futura-extra-black-condensed-bt.ttf"); }

    private static readonly Regex Ext = new("(.png)|(.jpg)");
    //private static PrivateFontCollection _fonts = new();
    private static string FontFamily => "Segoe UI Black"; //_fonts.Families.First();
    private static Font _sans;
    private static readonly SolidBrush TextColor = new(Color.Black);
    private static readonly StringFormat Format = new() { Alignment = StringAlignment.Center, Trimming = StringTrimming.Word, LineAlignment = StringAlignment.Center };

    private static void MakeFontSmaller () => _sans = new(FontFamily, _sans.Size * 0.8f, FontStyle.Bold);
    private static void SetFontToDefault() => _sans = new(FontFamily, 36, FontStyle.Bold);

    private int _w, _h, _t, _full;

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
        text = MemeGenerator.RemoveEmoji(text); // todo drawing them instead
        
        var textArea = new Bitmap(_w, _t);
        using var graphics = Graphics.FromImage(textArea);
        
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.Clear(Color.White);

        while (graphics.MeasureString(text, _sans, new SizeF(_w, 3 * _h)).Height > _t)
        {
            MakeFontSmaller();
        }
        
        var layout = new RectangleF(0, 0, _w, _t);
        
        graphics.CompositingMode = CompositingMode.SourceOver;
        
        graphics.DrawString(text, _sans, TextColor, layout, Format);

        return textArea;
    }
    
    public void SetUp(Size size)
    {
        SetFontToDefault();
        
        _w = size.Width;
        _h = size.Height;
        // _m = Math.Min(_h / 72, 10);
        _t = FF_Extensions.ToEven(_w > _h ? _h / 2 : _w / 2);
        _full = _h + _t;
    }
    private Image GetImage(string path)
    {
        //_resize = false;
        var image = new Bitmap(Image.FromFile(path));
        if (image.Width < 200)
        {
            //_resize = true;
            image = new Bitmap(Image.FromFile(path), new Size(200, image.Height * 200 / image.Width));
        }

        SetUp(image.Size);

        return image;
    }
    
    //public string MakeFrame(DgText text) => JpegCoder.SaveImageTemp(DrawFrame(text));
    
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