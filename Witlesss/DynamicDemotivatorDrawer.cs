using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using Witlesss.MediaTools;

namespace Witlesss // ReSharper disable InconsistentNaming
{

    public class DynamicDemotivatorDrawer
    {
        // color font[times/rg] weight[bold/regular]
        public static bool UseImpact, UseRoboto = true, UseBoldFont = true;
        public static bool CropEdges;

        private const int FM = 5;

        private int TextWidth => (full_w + img_w) / 2;

        private int img_w, img_h, txt_h, full_w, full_h, mg_top;
        private float ratio;

        private Point _pic;
        private Rectangle _frame;

        private readonly Pen White = new(Color.White, 2);
        private readonly EmojiTool _emojer = new() { MemeType = MemeType.Dp };

        // /

        private readonly StringFormat _format = new()
        {
            Alignment     = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming      = StringTrimming.Word
        };
        private readonly FontFamily[] _fonts = new[]
        {
            new FontFamily(DEMOTIVATOR_UPPER_FONT), new FontFamily("Roboto"), new FontFamily("Impact") 
        };
        private FontFamily FontFamily => _fonts[UseImpact ? 2 : UseRoboto ? 1 : 0];
        private Font _sans;

        private void ResizeFont(float size) => _sans = new(FontFamily, Math.Max(MinFontSize, size), FontStyle);
        private void SetFontSizeToDefault() => ResizeFont(StartingFontSize);

        private bool text_is_short;
        public  void PassTextLength(string text) => text_is_short = text.Length < 8;

        private float StartingFontSize => img_w * (text_is_short ? 0.15f : 0.1f);
        private float MinFontSize => Math.Max(img_w * 0.03f, 12);
    
        private FontStyle FontStyle => UseBoldFont ? FontStyle.Bold : FontStyle.Regular;

        // /

        public string BakeFrame(string text) => JpegCoder.SaveImageTemp(MakeFrame(DrawText(text)));

        public string DrawDemotivator(string path, string text)
        {
            PassTextLength(text);

            var image = GetImage(path);
            var funny = DrawText(text);

            var frame = MakeFrame(funny);

            return JpegCoder.SaveImage(PasteImage(frame, image), PngJpg.Replace(path, "-Dg.jpg"));
        }
    
        private Image PasteImage(Image background, Image image)
        {
            using var g = Graphics.FromImage(background);

            g.DrawImage(image, _pic);

            var size = FitSize(background.Size, 1280);

            return size == background.Size ? background : new Bitmap(background, size);
        }

        /// <summary> Makes a FRAME and adds TEXT</summary>
        private Image MakeFrame(Image caption)
        {
            var safe_w = 1.5f * img_w;
            if (caption.Width > safe_w) // can happen to "long" pictures with long text
            {
                var k = safe_w / caption.Width;
                caption = new Bitmap(caption, new Size((int)safe_w, (int)(caption.Height * k)));

                txt_h = caption.Height;
                AdjustTotalSize();
                AdjustImageFrame();
            }

            if (CropEdges && ratio > 1)
            {
                var offset = (full_w - caption.Width) / 2;
                _pic  .X -= offset;
                _frame.X -= offset;
                full_w = caption.Width;
            }
        
            Image background = new Bitmap(full_w, full_h);
            using var g = Graphics.FromImage(background);

            g.CompositingMode = CompositingMode.SourceCopy;
            g.Clear(Color.Black);

            g.CompositingMode = CompositingMode.SourceOver;
        
            if (caption.Width > full_w) // can happen to "tall" pictures with long text
            {
                var k = full_w / (float)caption.Width;
                caption = new Bitmap(caption, new Size(full_w, (int)(caption.Height * k)));
            }
        
            g.DrawImage(caption, new Point((full_w - caption.Width) / 2, mg_top + img_h + FM));
        
            g.DrawRectangle(White, _frame);

            return background;
        }

        /// <summary> Draws ONLY a black box with a text </summary>
        private Image DrawText(string text)
        {
            var emoji = EmojiRegex.Matches(text);
            var funny = emoji.Count > 0;
            var textM = funny ? EmojiTool.ReplaceEmoji(text, UseRoboto ? "aa" : "НН") : text; // todo find correct letters

            AdjustProportions(textM, out var width);

            var height = funny ? txt_h * 2 : txt_h;
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
                txt_h = txt_h - h + h * l / lines;
                AdjustTotalSize();
                AdjustImageFrame();
            }
            else graphics.DrawString(text, _sans, TextColor, area, _format);

            return image;
        }

        private int InitialMargin(int h) => (txt_h - h) / 2;
        private int Spacing   => (int)(_sans.Size * 1.6);
        private int EmojiSize => (int)(_sans.Size * 1.5);

        private SolidBrush TextColor => new(Color.White);

        private void AdjustProportions(string text, out int width)
        {
            width = 0; 
            SizeF measure;

            using var g = Graphics.FromHwnd(IntPtr.Zero);
            var initial_w = TextWidth;
            var rows = Math.Max(text.Count(c => c == '\n'), 1) + 1;
            var height = txt_h * rows / 2;
            var area = new SizeF(initial_w, height * 2 * ratio);
            int lines;


            MeasureString();
            if (lines == 1) return; // max size + text fits + single line

            ResizeFont(_sans.Size * 0.6f);
            MeasureString();
            if (lines == 1) return; // 0.6 size + text fits + single line


            while (_sans.Size > MinFontSize && measure.Height > height)
            {
                ResizeFont(_sans.Size * 0.8f);
                MeasureString();
            }

            if (_sans.Size <= MinFontSize)
            {
                ResizeFont(MinFontSize);
                area.Height *= 64;
                MeasureString();
            }

            txt_h = (int)(measure.Height + _sans.Size * 1.4f);
            AdjustTotalSize();

            width = TextWidth;


            area.Width = width;
            MeasureString();

            if (lines > 3)
            {
                txt_h = (int)(measure.Height + _sans.Size * 1.4f * Math.Pow(lines, 0.28));
            }
            else
            {
                txt_h = (int)(txt_h * initial_w / (float)TextWidth);
                txt_h = (int)(txt_h + _sans.Size * 1.4f);
            }

            AdjustTotalSize();
            AdjustImageFrame();

            void MeasureString() => measure = g.MeasureString(text, _sans, area, _format, out _, out lines);
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

            SetFontSizeToDefault();

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
    }
}