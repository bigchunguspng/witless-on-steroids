using System;
using System.Drawing;
using System.IO;
using System.Linq;
using MediaToolkit.Model;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using Witlesss.MediaTools;
using static System.Globalization.CultureInfo;
using static Witlesss.DemotivatorDrawer;
using MD = System.Threading.Tasks.Task<MediaToolkit.Tasks.GetMetadataResult>;

namespace Witlesss
{
    public class Memes
    {
        private readonly DemotivatorDrawer[]  _drawers;
        private static   IMediaToolkitService _service;

        public Memes()
        {
            _drawers = new[] {new DemotivatorDrawer(), new DemotivatorDrawer(1280)};

            while (!File.Exists(FFMPEG_PATH))
            {
                Log(@$"""{FFMPEG_PATH}"" not found. Put it here or close the window", ConsoleColor.Yellow);
                Log("Press any key to continue...");
                Console.ReadKey();
            }
            _service = MediaToolkitService.CreateInstance(FFMPEG_PATH);
        }

        public DgMode Mode;

        private DemotivatorDrawer Drawer => _drawers[(int) Mode];

        public string MakeDemotivator(string path, DgText text)
        {
            return Drawer.DrawDemotivator(path, text);
        }

        public string MakeStickerDemotivator(string path, DgText text, string extension)
        {
            return MakeDemotivator(Execute(new F_ToJPG(path, extension)), text);
        }

        public string MakeVideoDemotivator(string path, DgText text)
        {
            path = Execute(new F_Overlay(Drawer.MakeFrame(text), path, Drawer));

            if (JpegQuality > 50) return path;

            return Execute(new F_RemoveBitrate(path, 25 + (int)(JpegQuality * 2.5)));
        }

        public string ChangeSpeed(string path, double speed, SpeedMode mode, MediaType type)
        {
            if (mode == SpeedMode.Slow) speed = 1 / speed;
            
            Log($"SPEED >> {FormatDouble(speed)}", ConsoleColor.Blue);

            if (type > MediaType.Audio)
            {
                double fps = RetrieveFPS(GetMedia(path).AvgFrameRate) * speed;
                return Execute(new F_Speed(path, speed, type, Math.Min(fps, 90)));
            }
            else
                return Execute(new F_Speed(path, speed, type));
        }
        
        public string Sus(string path, TimeSpan start, TimeSpan length, MediaType type)
        {
            if (Path.GetExtension(path) == ".webm") path = Execute(new F_ToMP4(path, GetValidSize(path)));

            if (length < TimeSpan.Zero) length = TimeSpan.FromSeconds(GetDurationInSeconds(path) / 2D);

            if ((start + length).Ticks > 0) path = Cut(path, start, length);

            return Execute(new F_Concat(path, Reverse(path, type), type));
        }
        
        public string Reverse(string path, MediaType type)              => Execute(new F_Reverse(path, type));

        public string Cut(string path, TimeSpan start, TimeSpan length) => Execute(new F_Cut(path, start, length));

        public string RemoveBitrate(string path, ref int bitrate, MediaType type)
        {
            if (type > MediaType.Audio)
            {
                bool empty = bitrate == 0;

                var size = GetValidSize(path, out var stream);
                if (empty) bitrate = GetBitrate(stream, size);

                Log($"DAMN >> {B(stream)}k --> {bitrate}k", ConsoleColor.Blue);

                return Execute(new F_RemoveBitrate(path, bitrate, type));
            }
            else
                return Execute(new F_RemoveBitrate(path));

            string B(MediaStream stream) => int.TryParse(stream.BitRate, out int x) ? (x / 1000).ToString() : "~ ";
        }

        private int GetBitrate(MediaStream stream, Size size)
        {
            if (int.TryParse(stream.BitRate, out int x))
            {
                return Math.Clamp((int)(100 * Math.Log10(0.00001 * x + 1)), 1, 150);
            }
            else
            {
                return (size.Height + size.Width) / 20;
            }
        }
        private static Size    GetValidSize(string path, out MediaStream stream)
        {
            stream = GetMedia(path);
            int height = NotZero(stream.Height);
            int width  = NotZero(stream.Width);

            return (width | height) % 2 == 1 ? new Size(ToEven(width), ToEven(height)) : new Size(width, height);
        }
        private static Size    GetValidSize(string path) => GetValidSize(path, out _);
        private double GetDurationInSeconds(string path) => double.Parse(GetMedia(path).Duration, InvariantCulture);
        private static MediaStream GetMedia(string path) => GetMetadata(path).Result.Metadata.Streams.First();
        private static async MD GetMetadata(string path) => await _service.ExecuteAsync(new FfTaskGetMetadata(path));

        private string Execute(F_Base task) => _service.ExecuteAsync(task).Result;
        
        private static int NotZero(int x, int alt = 720) => x == 0 ? alt : x;
        private static int ToEven (int x) => x + x % 2;

        private double RetrieveFPS(string framerate, int alt = 30)
        {
            var fps = framerate.Split('/');
            try
            {
                double result = int.Parse(fps[0]) / double.Parse(fps[1]);
                return double.IsNaN(result) ? alt : result;
            }
            catch
            {
                return alt;
            }
        }

        public static bool ToMP4(string input, ref string output, out Size size)
        {
            var w = Path.GetExtension(input) == ".webm";
            if (w) output = Path.ChangeExtension(output, ".mp4");
            size = w ? GetValidSize(input) : Size.Empty;
            return w;
        }
    }

    public enum DgMode { Square, Wide }
}