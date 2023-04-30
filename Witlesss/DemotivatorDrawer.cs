using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Drawing.Drawing2D.CompositingMode;
using static System.Drawing.StringAlignment;
using static System.Drawing.StringTrimming;

namespace Witlesss
{
    public class DemotivatorDrawer
    {
        private readonly int _w, _h;
        private readonly Rectangle _frame;
        private readonly DrawableText _textA = new(), _textB = new();

        private readonly EmojiTool _emojer = new() { MemeType = MemeType.Dg };
        
        private static readonly Pen White;
        private static readonly Dictionary<Image, Point> Logos = new();
        private static readonly Regex Ext = new("(.png)|(.jpg)");
        private static readonly Random R = new();

        static DemotivatorDrawer()
        {
            White = new Pen(Color.White, 2);

            LoadLogos(WATERMARKS_FOLDER);
        }

        public DemotivatorDrawer(int width = 720, int height = 720)
        {
            _w = width;
            _h = height;

            var imageMarginT = 50;
            int imageMarginS = width == 1280 ? 144 : 50;
            var imageMarginB = 140;

            var imageW = _w - imageMarginS * 2;
            var imageH = _h - imageMarginT - imageMarginB;
            
            Size = new Size(imageW, imageH);

            var space = 5;
            int marginT = imageMarginT - space;
            int marginS = imageMarginS - space;
            int marginB = imageMarginB - space;

            Pic = new Point(imageMarginS, imageMarginT);
            _frame = new Rectangle(marginS, marginT, _w - 2 * marginS, _h - marginT - marginB);

            if (width == 1280)
            {
                _textA.P = DgTextParameters.LargeText(_h - imageMarginB + 28, _w);
                _textB.P = DgTextParameters.LowerText(_h, 0);
            }
            else
            {
                _textA.P = DgTextParameters.UpperText(_h - imageMarginB + 18, _w);
                _textB.P = DgTextParameters.LowerText(_h - imageMarginB + 84, _w);
            }
        }

        public Size Size { get; }
        public Point Pic { get; }

        public string DrawDemotivator(string path, DgText text) => PasteImage(DrawFrame(text), path);

        public string MakeFrame(DgText text) => JpegCoder.SaveImageTemp(DrawFrame(text));
        private Image DrawFrame(DgText text)
        {
            Image background = new Bitmap(_w, _h);
            using var graphics = Graphics.FromImage(background);

            graphics.CompositingMode = SourceCopy;
            graphics.Clear(Color.Black);
            graphics.DrawRectangle(White, _frame);
            if (_w == 720)
            {
                var logo = PickRandomLogo();
                graphics.DrawImage(logo.Key, logo.Value);
            }

            graphics.CompositingMode = SourceOver;

            _textA.Pass(graphics, text.A);
            _textB.Pass(graphics, text.B);

            DrawText(_textA);
            DrawText(_textB);
            
            return background;
        }

        private string PasteImage(Image background, string picture)
        {
            using var graphics = Graphics.FromImage(background);
            using var image = new Bitmap(Image.FromFile(picture), Size);
            
            graphics.CompositingMode = SourceCopy;
            graphics.DrawImage(image, Pic);

            return JpegCoder.SaveImage(background, Ext.Replace(picture, "-D.jpg"));
        }

        private static KeyValuePair<Image, Point> PickRandomLogo() => Logos.ElementAt(R.Next(Logos.Count));

        private void DrawText(DrawableText x)
        {
            var emoji = Regex.Matches(x.S, REGEX_EMOJI);
            if (emoji.Count > 0)
            {
                _emojer.DrawTextAndEmoji(x.G, x.S, emoji, x.P);
            }
            else
            {
                x.G.DrawString(x.S, x.P.Font, x.P.Color, x.P.Layout, x.P.Format);
            }
        }

        private static void LoadLogos(string path)
        {
            var files = GetFilesInfo(path);
            foreach (var file in files)
            {
                var coords = file.Name.Replace(file.Extension, "").Split(' ');
                if (int.TryParse(coords[0], out int x) && int.TryParse(coords[^1], out int y))
                    Logos.Add(Image.FromFile(file.FullName), new Point(x, y));
            }
        }
    }

    public class DrawableText
    {
        public Graphics G;
        public string S;
        public TextParameters P;

        public void Pass(Graphics g, string s) { G = g; S = s; }
    }

    public interface TextParameters
    {
        int Lines           { get; }
        int EmojiS          { get; }
        Font Font           { get; }
        SolidBrush Color    { get; }
        RectangleF Layout   { get; }
        StringFormat Format { get; }
        
        public Size EmojiSize => new(EmojiS, EmojiS);
    }
    public class DgTextParameters : TextParameters
    {
        public int Lines           { get; init; }
        public int EmojiS          { get; init; }
        public Font Font           { get; init; }
        public SolidBrush Color    { get; init; }
        public RectangleF Layout   { get; init; }
        public StringFormat Format { get; init; }

        public static DgTextParameters LargeText(int m, int w) => Construct(LargeFont, 1, 72, m, w);
        public static DgTextParameters UpperText(int m, int w) => Construct(UpperFont, 1, 54, m, w);
        public static DgTextParameters LowerText(int m, int w) => Construct(LowerFont, 2, 34, m, w);

        private static Font LargeFont => new(DEMOTIVATOR_UPPER_FONT, 48);
        private static Font UpperFont => new(DEMOTIVATOR_UPPER_FONT, 36);
        private static Font LowerFont => new(DEMOTIVATOR_LOWER_FONT, 18);

        private static DgTextParameters Construct(Font f, int l, int e, int margin, int width) => new()
        {
            Font   = f,
            Lines  = l,
            EmojiS = e,
            Color  = new SolidBrush(System.Drawing.Color.White),
            Layout = new RectangleF(0, margin, width, 100),
            Format = new StringFormat(StringFormatFlags.NoWrap) { Alignment = Center, Trimming = Word }
        };
    }
}