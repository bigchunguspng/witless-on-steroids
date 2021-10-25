﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using static System.Environment;
using static Witlesss.Also.Extension;
using static Witlesss.Also.Strings;

namespace Witlesss
{
    public class DemotivatorDrawer
    {
        private const string DotJpg = ".jpg";

        private readonly Random _r = new Random();
        private readonly Dictionary<Image, Point> _logos;
        private KeyValuePair<Image, Point> _logo;

        private readonly int _size, _imageMargin, _imageMarginB, _imageWidth;
        private readonly Font _fontA, _fontB;
        private readonly Rectangle _background, _frame;
        private readonly RectangleF _upperText, _lowerText;
        private readonly Pen _white;
        private readonly SolidBrush _fontColor;
        private readonly StringFormat _format;
        private readonly Point _imageTopLeft;

        public DemotivatorDrawer()
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

            _size = 720;
            _imageMargin = 50;
            _imageMarginB = 140;
            _imageWidth = _size - 2 * _imageMargin;

            int space = 5;
            int margin = _imageMargin - space;
            int marginB = _imageMarginB - space;

            _imageTopLeft = new Point(_imageMargin, _imageMargin);
            _background = new Rectangle(0, 0, _size, _size);
            _frame = new Rectangle(margin, margin, _size - 2 * margin, _size - margin - marginB);
            _upperText = new RectangleF(_imageMargin, _size - _imageMarginB + 18, _imageWidth, 100);
            _lowerText = new RectangleF(_imageMargin, _size - _imageMarginB + 84, _imageWidth, 100);
        }
        
        private void LoadLogos(string path)
        {
            Directory.CreateDirectory(path);
            string[] files = Directory.GetFiles(path);
            
            foreach (string file in files)
            {
                string[] coords = file.Split('\\', '.')[^2].Split(' ');
                if (int.TryParse(coords[0], out int x) && int.TryParse(coords[^1], out int y))
                    _logos.Add(Image.FromFile(file), new Point(x, y));
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

            using var image = ResizeImage(Image.FromFile(pathA), new Size(_imageWidth, _size - _imageMargin - _imageMarginB));
            using Image demotivator = new Bitmap(_size, _size);
            using var graphics = Graphics.FromImage(demotivator);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.FillRectangle(Brushes.Black, _background);
            graphics.DrawRectangle(_white, _frame);
            graphics.DrawImage(image, _imageTopLeft);
            
            graphics.DrawImage(_logo.Key, _logo.Value);

            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.DrawString(textA, _fontA, _fontColor, _upperText, _format);
            graphics.DrawString(textB, _fontB, _fontColor, _lowerText, _format);

            SaveImage(demotivator, ref pathB);

            return pathB;
        }

        public Image ResizeImage(Image image, Size size) => new Bitmap(image, size);

        private void SaveImage(Image image, ref string path)
        {
            path = UniquePath(path, DotJpg);
            image.Save(path, ImageFormat.Jpeg);
        }
    }
}