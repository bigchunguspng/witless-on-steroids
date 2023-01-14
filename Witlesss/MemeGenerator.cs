using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static System.Drawing.StringAlignment;
using static System.Drawing.StringTrimming;

namespace Witlesss;

public class MemeGenerator
{
    private Pen _outline;
    private bool _resize;
    private readonly SolidBrush   _white = new(Color.FromArgb(255, 255, 255));
    private readonly FontFamily    _font = new("Impact");
    private readonly StringFormat _upper = new() { Alignment = Center, Trimming = Word };
    private readonly StringFormat _lower = new() { Alignment = Center, Trimming = Word, LineAlignment = Far};
    private readonly int _m = 10;

    public string MakeImpactMeme(string path, DgText text)
    {
        using var image = GetImage(path);
        using var graphics = Graphics.FromImage(image);

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        
        var w = image.Width;
        var h = image.Height;
        var s = Math.Max((int)Math.Min(w, 1.5 * h) / 12, 12);
        var mB = h / 3 * 2;

        AddText(text.A, s, graphics, _upper, new Rectangle(_m, _m, w - 2 * _m, h / 3 - _m));
        AddText(text.B, s, graphics, _lower, new Rectangle(_m, mB, w - 2 * _m, h / 3 - _m));

        return SaveImage(_resize ? CropFix(image) : image, path);
    }
    private Bitmap GetImage(string path)
    {
        _resize = false;
        var image = new Bitmap(Image.FromFile(path));
        if (image.Width < 200)
        {
            _resize = true;
            image = new Bitmap(Image.FromFile(path), new Size(200, image.Height * 200 / image.Width));
        }

        return image;
    }

    private void AddText(string text, int size, Graphics g, StringFormat f, Rectangle rect)
    {
        if (string.IsNullOrEmpty(text)) return;

        using var path = new GraphicsPath();
        path.AddString(text, _font, 0, size, rect, f);
        for (int i = size / 6; i > 0; i--)
        {
            _outline = new Pen(Color.FromArgb(128, 0, 0, 0), i);
            _outline.LineJoin = LineJoin.Round;
            g.DrawPath(_outline, path);
            _outline.Dispose();
        }
        g.FillPath(_white, path);
    }
    
    private string SaveImage(Image image, string path)
    {
        path = UniquePath(path);
        image.Save(path);
        image.Dispose();

        return path;
    }

    private Image CropFix(Image image)
    {
        var crop = new Rectangle(0, 0, image.Width - 1, image.Height - 1);
        var bitmap = new Bitmap(image);
        return bitmap.Clone(crop, bitmap.PixelFormat);
    }
}