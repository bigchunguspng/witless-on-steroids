using System;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.MediaTools;

namespace Witlesss.Services.Memes // ReSharper disable InconsistentNaming
{

    public class DynamicDemotivatorDrawer
    {
        //public static bool UseImpact, UseRoboto, UseBoldFont;
        public static bool CropEdges, UseGivenColor;
        public static Color GivenColor;
        
        private SolidBrush TextColor;

        private const int FM = 5;

        private int TextWidth => (full_w + img_w) / 2;

        private int img_w, img_h, txt_h, full_w, full_h, mg_top;
        private float ratio;

        private Point _pic;
        private RectangleF _frame;

        public  Point Location => _pic;

        private readonly Pen White = new SolidPen(Color.White, 2);
        private readonly SolidBrush WhiteBrush = new(Color.White);
        private readonly EmojiTool _emojer = new() { MemeType = MemeType.Dp };

        // /

        private readonly DrawingOptions _textOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 1 }
        };

        private readonly DrawingOptions _frameOptions = new()
        {
            GraphicsOptions = new GraphicsOptions { Antialias = false }
        };

        private readonly SolidPen _framePen = new(new PenOptions(Color.White, 1.5F)
        {
            JointStyle = JointStyle.Miter,
            EndCapStyle = EndCapStyle.Polygon
        });

        public  static readonly ExtraFonts ExtraFonts = new("dp");
        private static FontFamily FontFamily => ExtraFonts.GetFontFamily("tm");
        private static FontStyle  FontStyle  => ExtraFonts.GetFontStyle(FontFamily);
        private Font _font;

        private void ResizeFont(float size) => _font = new Font(FontFamily, Math.Max(MinFontSize, size), FontStyle);
        private void SetFontSizeToDefault() => ResizeFont(StartingFontSize);

        private bool text_is_short;
        public  void PassTextLength(string text) => text_is_short = text.Length < 8;

        private float StartingFontSize => img_w * (text_is_short ? 0.15f : 0.1f);
        private float MinFontSize => Math.Max(img_w * 0.03f, 12);

        // /

        public string BakeFrame(string text) => ImageSaver.SaveImageTemp(MakeFrame(DrawText(text)));

        public string DrawDemotivator(string path, string text)
        {
            PassTextLength(text);

            var (size, info) = GetImageSize(path);
            SetUp(size);

            var image = GetImage(path, size, info);
            var funny = DrawText(text);

            var frame = MakeFrame(funny);

            return ImageSaver.SaveImage(PasteImage(frame, image), PngJpg.Replace(path, "-Dg.jpg"));
        }
    
        private Image PasteImage(Image background, Image image)
        {
            background.Mutate(x => x.DrawImage(image, _pic, opacity: 1));

            var size = FitSize(background.Size, 1280);
            if (size != background.Size)
            {
                background.Mutate(x => x.Resize(size));
            }

            image.Dispose();

            return background;
        }

        /// <summary> Makes a FRAME and adds TEXT</summary>
        private Image MakeFrame(Image caption)
        {
            var safe_w = 1.5f * img_w;
            if (caption.Width > safe_w) // can happen to "long" pictures with long text
            {
                var k = safe_w / caption.Width;
                var size = new Size(safe_w.RoundInt(), (caption.Height * k).RoundInt());
                caption.Mutate(x => x.Resize(size));

                txt_h = (int)(txt_h * k);
                AdjustTotalSize();
                AdjustImageFrame();
            }

            if (CropEdges && ratio > 1)
            {
                var cap_w = caption.Width.ToEven();
                var offset = (full_w - cap_w) / 2;
                _pic  .X -= offset;
                _frame.X -= offset;
                full_w = cap_w;
            }
        
            var background = new Image<Rgb24>(full_w, full_h, Color.Black);
        
            if (caption.Width > full_w) // can happen to "tall" pictures with long text
            {
                var k = full_w / (float)caption.Width;
                var size = new Size(full_w, (caption.Height * k).RoundInt());
                caption.Mutate(x => x.Resize(size));
            }

            var point = new Point((full_w - caption.Width) / 2, mg_top + img_h + FM);
            background.Mutate(x => x.DrawImage(caption, point, opacity: 1));
        
            background.Mutate(x => x.Draw(_frameOptions, _framePen, _frame));

            return background;
        }

        /// <summary> Draws ONLY a black box with a text </summary>
        private Image DrawText(string text)
        {
            var emoji = EmojiRegex.Matches(text);
            var funny = emoji.Count > 0;
            var textM = funny ? EmojiTool.ReplaceEmoji(text, GetEmojiReplacement()) : text;

            AdjustProportions(textM, out var width);

            var height = txt_h; //funny ? txt_h * 2 : txt_h;
            width = width == 0 ? TextWidth : width;

            var area = new RectangleF(0, 0, width, height);

            var image = new Image<Rgba32>(width, height, Color.Black);

#if DEBUG
            graphics.Clear(Color.Indigo);
#else
            //graphics.Clear(Color.Black);
#endif

            var options = GetDefaultTextOptions(area.Width, area.Height);
            if (funny)
            {
                var h = (int)TextMeasuringHelpers.MeasureTextSize(textM, options, out var lines).Height;
                var parameters = new EmojiTool.Options(TextColor, EmojiSize);
                var textLayer = _emojer.DrawEmojiText(text, options, parameters);
                txt_h = txt_h - h + textLayer.Height;
                AdjustTotalSize();
                AdjustImageFrame();
                var point = new Point((area.Size.CeilingInt() - textLayer.Size) / 2);
                image.Mutate(x => x.DrawImage(textLayer, point, opacity: 1));
            }
            else image.Mutate(x => x.DrawText(_textOptions, options, text, TextColor, pen: null));

            return image;
        }

        private RichTextOptions GetDefaultTextOptions(float width, float height) => new(_font)
        {
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Origin = new PointF(width / 2F, height / 2F),
            WrappingLength = width,
            LineSpacing = ExtraFonts.GetLineSpacing() * 1.2F,
            WordBreaking = WordBreaking.Standard,
            KerningMode = KerningMode.Standard,
            FallbackFontFamilies = ExtraFonts.FallbackFamilies,
        };

        private int InitialMargin(int h) => (txt_h - h) / 2;
        private int Spacing   => (int)(_font.Size * 1.6);
        private int EmojiSize => (int)(_font.Size * 1.125D);

        private string GetEmojiReplacement() => "aa"; // UseRoboto ? "aa" : UseImpact ? "НН" : UseBoldFont ? "гм" : "мя";

        private void AdjustProportions(string text, out int width)
        {
            width = 0; 
            SizeF measure;

            var initial_w = TextWidth;
            var rows = Math.Max(text.Count(c => c == '\n'), 1) + 1;
            var height = txt_h * rows / 2;
            var area = new SizeF(initial_w, height * 2 * ratio);
            int lines;


            MeasureString();
            if (lines == 1) return; // max size + text fits + single line

            ResizeFont(_font.Size * 0.6f);
            MeasureString();
            if (lines == 1) return; // 0.6 size + text fits + single line


            while (_font.Size > MinFontSize && measure.Height > height)
            {
                ResizeFont(_font.Size * 0.8f);
                MeasureString();
            }

            if (_font.Size <= MinFontSize)
            {
                ResizeFont(MinFontSize);
                area.Height *= 64;
                MeasureString();
            }

            txt_h = (int)(measure.Height + _font.Size * 1.4f);
            AdjustTotalSize();

            width = TextWidth;


            area.Width = width;
            MeasureString();

            if (lines > 3)
            {
                txt_h = (int)(measure.Height + _font.Size * 1.4f * Math.Pow(lines, 0.28));
            }
            else
            {
                txt_h = (int)(txt_h * initial_w / (float)TextWidth);
                txt_h = (int)(txt_h + _font.Size * 1.4f);
            }

            AdjustTotalSize();
            AdjustImageFrame();

            void MeasureString()
            {
                var options = GetDefaultTextOptions(area.Width, area.Height);
                measure = TextMeasuringHelpers.MeasureTextSize(text, options, out lines);
            }
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

            SetColor();

            return image;
        }


        // OTHER STUFF I GUESS

        public static Size FitSize(Size size, int max = 720) => size.FitSize(max);

        public void SetColor()
        {
            TextColor  = UseGivenColor ? new SolidBrush(GivenColor) : WhiteBrush;
            //FrameColor = UseGivenColor ? new SolidPen(GivenColor, 2)     : White;
        }
    
        public void SetUp(Size size)
        {
            img_w = size.Width;
            img_h = size.Height;

            ratio = img_w / (float)img_h;

            SetFontSizeToDefault();

            mg_top = (int)Math.Max(img_h * 0.06f, 12);
            txt_h  = (int)(_font.Size * 2.4f); // 75 -> 180

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
            _frame = new RectangleF(_pic.X - FM - 0.5F, _pic.Y - FM - 0.5F, img_w + 2 * FM, img_h + 2 * FM);
        }
    }
}