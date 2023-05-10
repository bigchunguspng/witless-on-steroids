using System;
using System.Drawing;
using System.IO;
using System.Text;
using FFMpegCore;
using static Witlesss.MediaTools.F_SingleInput_Base;
using Drawer = Witlesss.DemotivatorDrawer;

namespace Witlesss.MediaTools // ReSharper disable RedundantAssignment
{
    // -i video -i image -filter_complex "[0:v]scale=690:420[vid];[vid][1:v]overlay=0:0"   meme.mp4
    // -i image -i video -filter_complex "[1:v]scale=620:530[vid];[0:v][vid]overlay=50:50" demo.mp4
    public class F_Overlay
    {
        private readonly string _a, _b;

        public F_Overlay(string a, string b)
        {
            _a = a;
            _b = b;
        }

        public string Meme(int loss, Size size)
        {
            return Overlay(SetOutName(_a, "-M",   ".mp4"), o => ArgsMeme(o, loss, size));
        }
        public string Demo(int loss, Drawer drawer)
        {
            return Overlay(SetOutName(_b, "-D",   ".mp4"), o => ArgsDemo(o, loss, drawer.Size, drawer.Pic));
        }
        public string When(int loss, Size s, Rectangle c, Point p)
        {
            return Overlay(SetOutName(_b, "-Top", ".mp4"), o => ArgsWhen(o, loss, s, c, p));
        }

        private static void ArgsMeme(FFMpegArgumentOptions o, int f, Size s)
        {
            o = BuildAndCompress(o, f, CoFi.Null.Scale("0:v", s, "vid").Overlay("vid", "1:v", Point.Empty));
        }
        private static void ArgsDemo(FFMpegArgumentOptions o, int f, Size s, Point p)
        {
            o = BuildAndCompress(o, f, CoFi.Null.Scale("1:v", s, "vid").Overlay("0:v", "vid", p));
        }
        private static void ArgsWhen(FFMpegArgumentOptions o, int f, Size s, Rectangle c, Point p)
        {
            o = BuildAndCompress(o, f, CoFi.Null.Scale("1:v", s, "v1").Crop("v1", c, "vid").Overlay("0:v", "vid", p));
        }

        private static FFMpegArgumentOptions BuildAndCompress(FFMpegArgumentOptions o, int f, CoFi filter)
        {
            o = o.WithComplexFilter(filter);

            if (f >  0) o = o.WithVideoCodec("libx264").WithConstantRateFactor(f);
            if (f > 23) o = o.WithAudioBitrate(154 - 3 * f);

            return o;
        }

        // -i track -i image -map 0:0 -map 1:0 -c copy
        // -id3v2_version 3 -metadata:s:v title="Album cover" -metadata:s:v comment="Cover (front)"
        // -metadata artist="artist"
        // -metadata title="title" song.mp3
        public string AddTrackMetadata(string artist, string title)
        {
            var name = $"{Path.GetDirectoryName(_a)}/{(artist is null ? "" : $"{artist} - ")}{title}.mp3";
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
            o = o.WithCustomArgument(sb.ToString());
        }

        private string Overlay(string output, Action<FFMpegArgumentOptions> action)
        {
            var fap = FFMpegArguments.FromFileInput(_a).AddFileInput(_b).OutputToFile(output, addArguments: action);
            Run(fap);
            
            return output;
        }
    }
}