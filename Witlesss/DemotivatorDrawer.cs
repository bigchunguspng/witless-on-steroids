using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using static System.Environment;
using static Witlesss.Extension;
using static Witlesss.Strings;

namespace Witlesss
{
    public class DemotivatorDrawer
    {
        private const string DotJpg = ".jpg";

        private readonly Random _r = new Random();
        private readonly Dictionary<Image, Point> _logos;
        private KeyValuePair<Image, Point> _logo;

        private readonly int _w, _h, _imageMargin, _imageMarginB, _imageWidth;
        private readonly Font _fontA, _fontB;
        private readonly Rectangle _background, _frame;
        private readonly RectangleF _upperText, _lowerText;
        private readonly Pen _white;
        private readonly SolidBrush _fontColor;
        private readonly StringFormat _format;
        private readonly Point _imageTopLeft;

        public DemotivatorDrawer(int width = 720, int height = 720)
        {
            _logos = new Dictionary<Image, Point>();
            LoadLogos($@"{CurrentDirectory}\{WATERMARKS_FOLDER}");
            
            string fontA = DEMOTIVATOR_UPPER_FONT;
            string fontB = DEMOTIVATOR_LOWER_FONT;

            _fontA = new Font(fontA, 36);
            _fontB = new Font(fontB, 18);
            _fontColor = new SolidBrush(Color.White);

            _white = new Pen(Color.White, 2);
            _format = new StringFormat(StringFormatFlags.NoWrap)
                {Alignment = StringAlignment.Center, Trimming = StringTrimming.Word};

            _w = width;
            _h = height;
            _imageMargin = 50;
            _imageMarginB = 140;
            _imageWidth = _w - 2 * _imageMargin;

            var space = 5;
            int margin = _imageMargin - space;
            int marginB = _imageMarginB - space;

            _imageTopLeft = new Point(_imageMargin, _imageMargin);
            _background = new Rectangle(0, 0, _w, _h);
            _frame = new Rectangle(margin, margin, _w - 2 * margin, _h - margin - marginB);
            _upperText = new RectangleF(0, _h - _imageMarginB + 18, _w, 100);
            _lowerText = new RectangleF(0, _h - _imageMarginB + 84, _w, 100);
        }
        
        private void LoadLogos(string path)
        {
            var files = GetFiles(path);
            foreach (var file in files)
            {
                var coords = file.Name.Replace(file.Extension, "").Split(' ');
                if (int.TryParse(coords[0], out int x) && int.TryParse(coords[^1], out int y))
                    _logos.Add(Image.FromFile(file.FullName), new Point(x, y));
            }
        }

        public void SetRandomLogo() => _logo = _logos.ElementAt(_r.Next(_logos.Count));

        public string DrawDemotivator(string path, string textA, string textB)
        {
            //1280 x 1024 нейродемотиваторы (5:4)
            // 357 x 372 ржакабот в беседах
            // 714 x 745 ржакабот в личке
            // 430 x 430 - telegram max res to be displayed on desktop
            // 720 x 720

            string pathA = path;
            string pathB = path.Replace(DotJpg, "-D" + DotJpg);

            using var image = ResizeImage(Image.FromFile(pathA), new Size(_imageWidth, _h - _imageMargin - _imageMarginB));
            using Image demotivator = new Bitmap(_w, _h);
            using var graphics = Graphics.FromImage(demotivator);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.FillRectangle(Brushes.Black, _background);
            graphics.DrawRectangle(_white, _frame);
            if (_w == 720) graphics.DrawImage(_logo.Key, _logo.Value);
            graphics.DrawImage(image, _imageTopLeft);

            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.DrawString(textA, _fontA, _fontColor, _upperText, _format);
            graphics.DrawString(textB, _fontB, _fontColor, _lowerText, _format);

            SaveImage(demotivator, ref pathB);

            return pathB;
        }

        private Image ResizeImage(Image image, Size size) => new Bitmap(image, size);

        private void SaveImage(Image image, ref string path)
        {
            path = UniquePath(path, DotJpg);
            image.Save(path, ImageFormat.Jpeg);
        }
    }
}