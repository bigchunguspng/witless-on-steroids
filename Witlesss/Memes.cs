using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;

namespace Witlesss
{
    public class Memes
    {
        private const string DotJpg = ".jpg";
        
        private readonly Random _r = new Random();
        private readonly Dictionary<Image, Point> _logos;
        private KeyValuePair<Image,Point> _logo;

        public Memes()
        {
            _logos = new Dictionary<Image, Point>();
            _logos.Add(Image.FromFile(@"D:\Videos\Монтаж\Проекти\DoritRYTP\BullShit\Vi\z prikolchiki\Unregistered-Hypercam-2.png"), new Point(51, 37));
            _logos.Add(Image.FromFile(@"D:\Videos\Монтаж\Проекти\DoritRYTP\BullShit\Vi\z prikolchiki\PS-PS4.png"), new Point(299, 8)); //44
            _logos.Add(Image.FromFile(@"D:\Videos\Монтаж\Проекти\DoritRYTP\BullShit\Vi\z prikolchiki\PS4.png"), new Point(59, 30));
        }
        
        public string MakeDemotivator(string path, string textA, string textB)
        {
            //1280 x 1024 нейродемотиваторы (5:4)
            // 357 x 372 ржакабот в беседах
            // 714 x 745 ржакабот в личке
            // 430 x 430 - telegram max res to be displayed on desktop
            // 720 x 720

            string pathA = path;
            string pathB = path.Replace(DotJpg, "-D" + DotJpg);
            string fontA = "Times New Roman", fontB = "Roboto Slab Medium";

            int size = 720;
            int imageMargin = 50;
            int imageMarginB = 140;
            int imageWidth = size - 2 * imageMargin;
            int space = 5;
            int margin = imageMargin - space;
            int marginB = imageMarginB - space;

            Image image = ResizeImage(Image.FromFile(pathA), new Size(imageWidth, size - imageMargin - imageMarginB));
            Image demotivator = new Bitmap(size, size);

            Rectangle background = new Rectangle(0, 0, size, size);
            Rectangle frame = new Rectangle(margin, margin, size - 2 * margin, size - margin - marginB);
            RectangleF upperText = new RectangleF(imageMargin, size - imageMarginB + 18, imageWidth, 100);
            RectangleF lowerText = new RectangleF(imageMargin, size - imageMarginB + 84, imageWidth, 100);

            Pen white = new Pen(Color.White, 2);
            StringFormat format = new StringFormat(StringFormatFlags.NoWrap) {Alignment = StringAlignment.Center, Trimming = StringTrimming.Word};

            Graphics graphics = Graphics.FromImage(demotivator);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.FillRectangle(Brushes.Black, background);
            graphics.DrawRectangle(white, frame);
            graphics.DrawImage(image, new Point(imageMargin, imageMargin));
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.DrawString(textA, new Font(fontA, 36), new SolidBrush(Color.White), upperText, format);
            graphics.DrawString(textB, new Font(fontB, 19), new SolidBrush(Color.White), lowerText, format);
            
            SetRandomLogo();
            graphics.DrawImage(_logo.Key, _logo.Value);
            
            SaveImage(demotivator, ref pathB);
            return pathB;
        }
        
        private Image ResizeImage(Image image, Size size) => new Bitmap(image, size);
        private void SetRandomLogo()
        {
            int n = _r.Next(_logos.Count);
            int i = 0;
            foreach (var pair in _logos)
            {
                if (i == n)
                {
                    _logo = pair;
                    return;
                }
                i++;
            }
        }
        private void SaveImage(Image image, ref string path)
        {
            while (File.Exists(path))
            {
                path = path.Replace(DotJpg, "");
                if (path.EndsWith("D"))
                    path = path + "_0";
                else
                {
                    int index = path.LastIndexOf("_", StringComparison.Ordinal) + 1;
                    int number = int.Parse(path.Substring(index)) + 1;
                    
                    path = path.Remove(index) + number;
                }

                path = path + DotJpg;
            }
            image.Save(path, ImageFormat.Jpeg);
        }
    }
}