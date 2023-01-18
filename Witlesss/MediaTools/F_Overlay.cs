using System.Drawing;
using System.IO;
using Drawer = Witlesss.DemotivatorDrawer;

namespace Witlesss.MediaTools
{
    // ffmpeg  -i "image.png" -i "video.mp4" -filter_complex "[1:v]scale=620:530[pic];[0:v][pic]overlay=50:50" output.mp4
    public class F_Overlay : F_Base
    {
        public F_Overlay(string lower, string upper, Size s, Point p, int quality) : base(SetOutName(upper, "-D"))
        {
            if (lower.Contains(PICTURES_FOLDER)) Output = SetOutName(lower, "-M");

            var d = quality > 0;

            AddInput(lower);
            AddInput(upper);
            AddOptions("-filter_complex", $"[1:v]scale={s.Width}:{s.Height}[pic];[0:v][pic]overlay={p.X}:{p.Y}");
            AddWhen(d, "-vcodec", "libx264", "-crf", quality.ToString(), "-b:a", "1k");

            Output = Path.ChangeExtension(Output, ".mp4");
        }
    }
}