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
        public F_ToMP4(string input, Size size, string extension) : base(input, extension)
        {
            AddSize(size);
        }
    }

    public class F_ToAnimation : F_ToMP4
    {
        public F_ToAnimation(string input, Size size) : base(input, size)
        {
            AddOptions("-an", "-vcodec", "libx264", "-crf", "30");
        }
    }
}