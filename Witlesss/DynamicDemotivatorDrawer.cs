﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Witlesss.MediaTools;

using static System.Drawing.StringAlignment;

namespace Witlesss; // ReSharper disable InconsistentNaming

public class DynamicDemotivatorDrawer
{
    // color font[times/rg] weight[bold/regular]
    public static bool UseRegularFont, UseBoldFont;
    
    private const int FM = 5;

    private int img_w, img_h, txt_h, full_w, full_h;
    private int mg_top;
    private float ratio;
    private Rectangle _frame;
    private Point _pic;
    
    private SizeF _measure;

    private bool shortText;
    public  void CheckTextLength(string text) => shortText = text.Length < 8;

    private readonly EmojiTool _emojer = new() { MemeType = MemeType.Top };
    private static readonly Pen White = new(Color.White, 2);
    
    
    private static readonly PrivateFontCollection _fonts = new();
    private static FontFamily FontFamily => UseRegularFont ? _fonts.Families[0] : new FontFamily(DEMOTIVATOR_UPPER_FONT);
    private static Font _sans;
    private static readonly StringFormat _format = new() { Alignment = Center, Trimming = StringTrimming.Word, LineAlignment = Center };

    private float StartingFontSize => img_w * (shortText ? 0.15f : 0.1f);
    private float MinFontSize => Math.Max(img_w * 0.03f, 12);

    private void SetFontToDefault() => ResizeFont(StartingFontSize);

    private void ResizeFont(float size) => _sans = new(FontFamily, Math.Max(MinFontSize, size));


    static DynamicDemotivatorDrawer()
    {
        _fonts.AddFontFile(Config.FontRegular);
    }
    
    
    public string DrawDemotivator(string path, string text)
    {
        CheckTextLength(text);

        var image = GetImage(path);
        var funny = DrawText(text); // draws only text block

        var frame = CombineFrame(funny);

        return JpegCoder.SaveImage(PasteImage(frame, image), PngJpg.Replace(path, "-Dg.jpg"));
    }
    
    private Image CombineFrame(Image funny)
    {
        Image background = new Bitmap(full_w, full_h);
        using var graphics = Graphics.FromImage(background);

        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.Clear(Color.Black);

        graphics.CompositingMode = CompositingMode.SourceOver;
        graphics.DrawImage(funny, new Point((full_w - funny.Width) / 2, mg_top + img_h + FM));
        
        graphics.DrawRectangle(White, _frame);

        return background;
    }

    private Image PasteImage(Image background, Image image)
    {
        using var g = Graphics.FromImage(background);
        
        g.DrawImage(image, _pic);
        
        return background;
    }

    private int InitialMargin(int h) => (txt_h - h) / 2;
    private int Spacing   => (int)(_sans.Size * 1.6);
    private int EmojiSize => (int)(_sans.Size * 1.5);

    private SolidBrush TextColor => new(Color.White);

    private Image DrawText(string text)
    {
        var emoji = EmojiRegex.Matches(text);
        var funny = emoji.Count > 0;
        var textM = funny ? EmojiTool.ReplaceEmoji(text, UseRegularFont ? "aa" : "НН") : text; // todo find correct letters

        AdjustProportions(textM, out var width);
        //AdjustTextPosition(text);

        var height = funny ? txt_h * 2 : txt_h; // probably this fixes visibility of the last emoji-text line
        width = width == 0 ? TextWidth : width;

        var area = new RectangleF(0, 0, width, height);

        var image = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(image);
        
        graphics.Clear(Color.Indigo); // todo replace with black when ready
        
        graphics.CompositingMode    = CompositingMode.SourceOver;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.PixelOffsetMode    = PixelOffsetMode.HighQuality;
        graphics.TextRenderingHint  = TextRenderingHint.AntiAlias;

        if (funny)
        {
            var p = new TextParams(62, EmojiSize, _sans, TextColor, area, _format);
            var h = (int)graphics.MeasureString(textM, _sans, area.Size, _format, out _, out var lines).Height;
            var l = _emojer.DrawTextAndEmoji(graphics, text, emoji, p, InitialMargin(h), Spacing);
            var d = txt_h - h;
            txt_h = h * l / lines + d; // i guess ???
            AdjustTotalSize();
            AdjustImageFrame();
        }
        else graphics.DrawString(text, _sans, TextColor, area, _format);

        return image;
    }

    private void AdjustProportions(string text, out int width)
    {
        width = 0;

        using var g = Graphics.FromHwnd(IntPtr.Zero);
        var initial_w = TextWidth;
        var area = new SizeF(initial_w, txt_h * 2);
        int lines;
        
        if (!text.Contains('\n'))
        {
            MeasureString();
            if (lines == 1) return; // max size + text fits + single line
            
            ResizeFont(_sans.Size * 0.6f);
            MeasureString();
            if (lines == 1) return; // 0.6 size + text fits + single line
            
            while (_sans.Size > MinFontSize && _measure.Height > txt_h)
            {
                ResizeFont(_sans.Size * 0.5f);
                MeasureString();
            }
            
            if (_sans.Size <= MinFontSize)
            {
                ResizeFont(MinFontSize);
                area.Height *= 64;
                MeasureString();
            }
            
            txt_h = (int)(_measure.Height + _sans.Size * 1.4f);
            AdjustTotalSize();

            width = TextWidth;

            txt_h = (int)(txt_h * initial_w / (float)TextWidth);
            txt_h = (int)(txt_h + _sans.Size * 1.4f);
            AdjustTotalSize();
            AdjustImageFrame();
            
        }
        void MeasureString() => _measure = g.MeasureString(text, _sans, area, _format, out _, out lines);
    }

    private Image GetImage(string path)
    {
        var pic = Image.FromFile(path);
        var size = FitSize(pic.Size);
        var image = new Bitmap(pic, size.Width < 200 ? new Size(200, size.Height * 200 / size.Width) : size);

        SetUp(image.Size);
        //SetColor(image);

        return image;
    }

    public static Size FitSize(Size s, int max = 720)
    {
        return s.Width > max || s.Height > max ? Memes.NormalizeSize(s, max) : s;
    }
    
    public void SetUp(Size size)
    {
        img_w = size.Width;
        img_h = size.Height;

        ratio = img_w / (float)img_h;

        SetFontToDefault();

        mg_top = (int)(img_h * 0.06f);
        txt_h  = (int)(_sans.Size * 2.4f); // 75 -> 180

        AdjustTotalSize();
        AdjustImageFrame();
    }

    /// <summary> CALL THIS after changing <see cref="txt_h"/> </summary>
    private void AdjustTotalSize()
    {
        full_h = FF_Extensions.ToEven(mg_top + img_h + FM + txt_h);
        full_w = FF_Extensions.ToEven((int)(full_h * ratio));
    }
    private void AdjustImageFrame()
    {
        _pic = new Point((full_w - img_w) / 2, mg_top + 1);
        _frame = new Rectangle(_pic.X - FM, _pic.Y - FM, img_w + 2 * FM, img_h + 2 * FM);
    }

    private int TextWidth => (full_w + img_w) / 2;
}