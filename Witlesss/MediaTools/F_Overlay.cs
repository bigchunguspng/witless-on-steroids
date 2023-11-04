using System;
using System.Drawing;
using System.IO;
using System.Text;
using FFMpegCore;
using static Witlesss.MediaTools.F_SingleInput_Base;
using Drawer = Witlesss.DemotivatorDrawer;

namespace Witlesss.MediaTools
{
    // -i video -i image -filter_complex "[0:v]scale=620:530[vid];[1:v][vid]overlay=50:50"
    public class F_Overlay
    {
        private readonly string _a, _b;

        public F_Overlay(string a, string b)
        {
            _a = a; // video
            _b = b; // image
        }

        public string Meme(int loss, Size size)
        {
            return Overlay(SetOutName(_a, "-M",   ".mp4"), o => ArgsMeme(o, loss, size));
        }
        public string Demo(int loss, Drawer drawer)
        {
            return Overlay(SetOutName(_a, "-D",   ".mp4"), o => ArgsDemo(o, loss, drawer.Size, drawer.Pic));
        }
        public string When(int loss, Size s, Rectangle c, Point p, bool blur)
        {
            return Overlay(SetOutName(_a, "-Top", ".mp4"), o => ArgsWhen(o, loss, s, c, p, blur));
        }
        public string D300(int loss, Size s, Point p, Size size)
        {
            return Overlay(SetOutName(_a, "-Dp",  ".mp4"), o => { ArgsDemo(o, loss, s, p); o.Resize(size); });
        }

        private void ArgsMeme(FFMpegArgumentOptions o, int f, Size s)
        {
            BuildAndCompress(o, f, CoFi.Null.Scale("0:v", s, "vid").Overlay("vid", "1:v", Point.Empty));
        }
        private void ArgsDemo(FFMpegArgumentOptions o, int f, Size s, Point p)
        {
            BuildAndCompress(o, f, FixedFpsPic().Scale("0:v", s, "vid").Overlay("pic", "vid", p));
        }
        private void ArgsWhen(FFMpegArgumentOptions o, int f, Size s, Rectangle c, Point p, bool blur)
        {
            var fi = FixedFpsPic().Scale("0:v", s, "v0").Crop("v0", c, "vid").Overlay("pic", "vid", p, blur ? "ova" : null);
            BuildAndCompress(o, f, blur ? fi.Blur("ova", 1) : fi);
        }

        private CoFi FixedFpsPic() => CoFi.Null.Fps("1:v", new F_Resize(_a).GetFramerate(), "pic");

        private static void BuildAndCompress(FFMpegArgumentOptions o, int f, CoFi filter)
        {
            o.WithComplexFilter(filter);

            if (f >  0) o.WithCompression(f);
            if (f > 23) o.WithAudioBitrate(154 - 3 * f);
        }

        public string AddTrackMetadata(string artist, string title)
        {
            var file = $"{(artist is null ? "" : $"{artist} - ")}{title}";
            var name = $"{Path.GetDirectoryName(_a)}/{ValidFileName(file, '#')}.mp3";
            return Overlay(name, o => MetadataArgs(o, artist, title));
        }

        private static void MetadataArgs(FFMpegArgumentOptions o, string artist, string title)
        {
            var sb = new StringBuilder();
            sb.Append("-map 0:0 -map 1:0 -c copy -id3v2_version 3 ");
            sb.Append("-metadata:s:v title=\"Album cover\" ");
            sb.Append("-metadata:s:v comment=\"Cover (front)\" ");
            if (artist is not null) sb.Append("-metadata artist=\"").Append(artist).Append("\" ");
            sb.Append                        ("-metadata title=\"" ).Append(title ).Append("\" ");

            o.WithCustomArgument(sb.ToString());
        }

        private string Overlay(string output, Action<FFMpegArgumentOptions> action)
        {
            Run(FFMpegArguments.FromFileInput(_a).AddFileInput(_b).OutputToFile(output, addArguments: action));
            
            return output;
        }
    }
}