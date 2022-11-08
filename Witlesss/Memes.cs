using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaToolkit.Model;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using Witlesss.MediaTools;
using static Witlesss.DemotivatorDrawer;

namespace Witlesss
{
    public class Memes
    {
        private readonly DemotivatorDrawer[]  _drawers;
        private readonly IMediaToolkitService _service;

        public Memes()
        {
            _drawers = new[] {new DemotivatorDrawer(), new DemotivatorDrawer(1280)};

            if (!File.Exists(FFMPEG_PATH))
            {
                Log(@$"""{FFMPEG_PATH}"" не найден. Поместите его туда или оформите вылет", ConsoleColor.Yellow);
                Log("Нажмите любую клавишу для продолжения...");
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
            Execute(new F_WebpToJpg(path, out path, extension));
            
            return MakeDemotivator(path, text);
        }

        public string MakeVideoDemotivator(string path, DgText text)
        {
            Execute(new F_Overlay(Drawer.MakeFrame(text), path, out string output, Drawer));

            if (JpegQuality > 50) return output;

            path = output;
            output = SetOutName(output, "-L");
            Execute(new F_RemoveBitrate(path, output, 25 + (int)(JpegQuality * 2.5), Drawer.Size));

            return output;
        }

        public string ChangeSpeed(string path, double speed, SpeedMode mode, MediaType type)
        {
            if (mode == SpeedMode.Slow) speed = 1 / speed;
            
            Log($"SPEED >> {FormatDouble(speed)}", ConsoleColor.Blue);

            WebmToMp4(ref path);
            var output = SetOutName(path, "-S");

            if (type == MediaType.Audio)
            {
                Execute(new F_Speed(path, output, speed, type));
            }
            else
            {
                double fps = RetrieveFPS(GetMediaStream(path).AvgFrameRate) * speed;
                Execute(new F_Speed(path, output, speed, type, Math.Min(fps, 90)));
            }

            return output;
        }
        
        public string Sus(string path, TimeSpan start, TimeSpan length, MediaType type)
        {
            WebmToMp4(ref path);
            
            if (length < TimeSpan.Zero) length = TimeSpan.FromSeconds(GetDurationInSeconds(path) / 2D);
            
            if (start != TimeSpan.Zero || length != TimeSpan.Zero)
            {
                Execute(new F_Cut(path, out path, start, length));
            }
            
            Execute(new F_Reverse(path, out string reversed, type));
            Execute(new F_Concat(path, reversed, out string output, type));

            return output;
        }
        
        public string Reverse(string path, MediaType type)
        {
            WebmToMp4(ref path);
            
            Execute(new F_Reverse(path, out string output, type));
            return output;
        }
        
        public string Cut(string path, TimeSpan start, TimeSpan length)
        {
            WebmToMp4(ref path);
            
            Execute(new F_Cut(path, out string output, start, length));
            return output;
        }

        public string RemoveBitrate(string path, int bitrate, out int value, MediaType type)
        {
            var output = SetOutName(path, "-L").Replace(".webm", ".mp4");
            bool empty = bitrate == 0;

            if (type == MediaType.Audio)
            {
                Execute(new F_RemoveBitrate(path, output, bitrate));
            }
            else
            {
                var size = GetValidSize(path, out var stream);
                if (empty) bitrate = GetBitrate(stream, size);

                Log($"DAMN >> {B(stream)}k --> {bitrate}k", ConsoleColor.Blue);

                Execute(new F_RemoveBitrate(path, output, bitrate, size));
            }

            value = bitrate;
            return output;

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
        private Size GetValidSize(string path, out MediaStream stream)
        {
            stream = GetMediaStream(path);
            int height = FallbackIfZero(stream.Height, 720);
            int width  = FallbackIfZero(stream.Width,  720);

            return (width | height) % 2 == 1 ? new Size(ToEven(width), ToEven(height)) : new Size(width, height);
        }
        private Size GetValidSize(string path) => GetValidSize(path, out _);
        private double GetDurationInSeconds(string path) => double.Parse(GetMediaStream(path).Duration, CultureInfo.InvariantCulture);
        private MediaStream GetMediaStream(string path) => GetMetadata(path).Result.Metadata.Streams.First();
        private async Task<GetMetadataResult> GetMetadata(string path) => await _service.ExecuteAsync(new FfTaskGetMetadata(path));

        private void Execute(F_Base task) => _service.ExecuteAsync(task).Wait();
        
        private int FallbackIfZero(int x, int alt) => x == 0 ? alt : x;
        private int ToEven(int x) => x + x % 2;

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

        private void WebmToMp4(ref string path)
        {
            var extension = Path.GetExtension(path);
            if (extension == ".webm") Execute(new F_WebmToMp4(path, out path, ".mp4", GetValidSize(path)));
        }
    }

    public enum DgMode
    {
        Square,
        Wide
    }
}