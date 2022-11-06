using System.Collections.Generic;

namespace Witlesss.MediaTools
{
    // ffmpeg  -i "image.png" -i "video.mp4" -filter_complex "[1:v]scale=620:530[pic];[0:v][pic]overlay=50:50" output.mp4
    public class F_Overlay : F_Base
    {
        private readonly string _image, _video, _output;
        private readonly DemotivatorDrawer _d;

        public F_Overlay(string image, string video, out string output, DemotivatorDrawer drawer)
        {
            output = SetOutName(video.Replace(".webm", ".mp4"), "-D");
            
            _image = image;
            _video = video;
            _output = output;
            _d = drawer;
        }

        public override IList<string> CreateArguments() => new[]
        {
            "-i", _image, "-i", _video, "-filter_complex",
            $"[1:v]scale={_d.Size.Width}:{_d.Size.Height}[pic];[0:v][pic]overlay={_d.Pic.X}:{_d.Pic.Y}", _output
        };
    }
}