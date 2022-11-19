using System.Drawing;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webm" -s WxH output.mp4
    public class F_ToMP4 : F_ToJPG
    {
        public F_ToMP4(string input, Size size) : base(input, ".mp4")
        {
            AddSize(size);
        }
    }

    public class F_ToAnimation : F_ToMP4
    {
        public F_ToAnimation(string input, Size size) : base(input, NormalizeSize(size))
        {
            AddOptions("-an", "-vcodec", "libx264", "-crf", "30");
        }

        private static Size NormalizeSize(Size s)
        {
            if (s.Width > 1280 || s.Height > 1280)
            {
                if (s.Width > s.Height)
                {
                    return new Size(1280, (int)(s.Height / (s.Width / 1280D)));
                }
                else
                {
                    return new Size((int)(s.Width / (s.Height / 1280D)), 1280);
                }
            }
            else return s;
        } 
    }
}