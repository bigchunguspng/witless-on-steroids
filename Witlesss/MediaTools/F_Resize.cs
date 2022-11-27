using System.Drawing;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webm" -s WxH output.mp4
    public class F_Resize : F_ToJPG
    {
        public F_Resize(string input, Size size, string extension = ".mp4") : base(input, extension)
        {
            AddSize(size);
        }
    }

    public class F_ToAnimation : F_Resize
    {
        public F_ToAnimation(string input, Size size) : base(input, size)
        {
            AddOptions("-an", "-vcodec", "libx264", "-crf", "30");
        }
    }
}