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
}