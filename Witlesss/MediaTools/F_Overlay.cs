using System.IO;
using Drawer = Witlesss.DemotivatorDrawer;

namespace Witlesss.MediaTools
{
    // ffmpeg  -i "image.png" -i "video.mp4" -filter_complex "[1:v]scale=620:530[pic];[0:v][pic]overlay=50:50" output.mp4
    public class F_Overlay : F_Base
    {
        public F_Overlay(string image, string video, Drawer drawer, int quality) : base(SetOutName(video, "-D"))
        {
            var s = drawer.Size;
            var p = drawer.Pic;
            var d = quality > 0;

            AddInput(image);
            AddInput(video);
            AddOptions("-filter_complex", $"[1:v]scale={s.Width}:{s.Height}[pic];[0:v][pic]overlay={p.X}:{p.Y}");
            AddWhen(d, "-vcodec", "libx264", "-crf", quality.ToString(), "-b:a", "1k");

            Output = Path.ChangeExtension(Output, ".mp4");
        }
    }
}