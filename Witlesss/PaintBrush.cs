using System;
using System.Drawing;

namespace Witlesss
{
    public interface PaintBrush { SolidBrush Brush { get; } }
    
    public class WhiteBrush : PaintBrush
    {
        public SolidBrush Brush { get; } = new(Color.FromArgb(255, 255, 255));
    }
    
    public class RandomBrush : PaintBrush
    {
        private readonly MemeGenerator _imgflip;

        public RandomBrush(MemeGenerator mg) => _imgflip = mg;

        public SolidBrush Brush => RandomColor();

        private SolidBrush RandomColor()
        {
            var h = Extension.Random.Next(360);
            var s = Extension.Random.NextDouble();
            var v = Extension.Random.NextDouble();

            var o = Math.Min(_imgflip.OutlineWidth, 6);
            var x = Math.Min(Math.Abs(240 - h), 60);

            s = s * (0.75 + x / 240D);  // <-- removes dark blue
            s = s * (0.25 + 0.125 * o); // <-- makes small text brighter

            v = 1 - 0.3 * v * Math.Sqrt(s);

            return new SolidBrush(ColorFromHSV(h, s, v));
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            var sextants = hue / 60;
            var triangle = Math.Floor(sextants);
            int dye = Convert.ToInt32(triangle) % 6;
            var fraction = sextants - triangle;

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - saturation * fraction));
            int t = Convert.ToInt32(value * (1 - saturation * (1 - fraction)));

            return dye switch
            {
                0 => Color.FromArgb(v, t, p),
                1 => Color.FromArgb(q, v, p),
                2 => Color.FromArgb(p, v, t),
                3 => Color.FromArgb(p, q, v),
                4 => Color.FromArgb(t, p, v),
                _ => Color.FromArgb(v, p, q)
            };
        }
    }
}