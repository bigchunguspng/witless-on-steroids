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

    // ffmpeg -i "input.mp4" -s WxH -an -vcodec libx264 -crf 30 output.mp4
    public class F_ToAnimation : F_Resize
    {
        public F_ToAnimation(string input, Size size) : base(input, size)
        {
            AddOptions("-an", "-vcodec", "libx264", "-crf", "30");
        }
    }
    
    // ffmpeg -i "input.mp4" -an -vcodec libx264 -crf 30 output.mp4
    public class F_CompressAnimation : F_ToJPG
    {
        public F_CompressAnimation(string input) : base(input, ".mp4")
        {
            AddOptions("-an", "-vcodec", "libx264", "-crf", "30");
        }
    }
    
    //ffmpeg -i input.mp4 -filter:v "crop=272:272:56:56" output.mp4
    public class F_Crop : F_ToJPG
    {
        public F_Crop(string input, string extension = ".mp4") : base(input, extension)
        {
            AddOptions("-filter:v", "crop=272:272:56:56");
        }
    }

    //ffmpeg -i input.mp4 -filter:v "crop=D:D:X:Y" -s 384x384 output.mp4
    public class F_ToVideoNote : F_ToJPG
    {
        private static readonly Size VideoNoteSize = new(384, 384);
        
        public F_ToVideoNote(string input, Rectangle a, string extension = ".mp4") : base(input, extension)
        {
            AddOptions("-filter:v", $"crop={a.Width}:{a.Height}:{a.X}:{a.Y}");
            AddSize(VideoNoteSize);
        }
    }
}