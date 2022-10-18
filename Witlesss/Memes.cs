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
using static Witlesss.Logger;
using static System.Environment;
using static Witlesss.Extension;
using static Witlesss.Strings;

namespace Witlesss
{
    public class Memes
    {
        private readonly DemotivatorDrawer[] _drawers;
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

        private DemotivatorDrawer Drawer => Mode switch
        {
            DgMode.Square => _drawers[0],
            DgMode.Wide =>   _drawers[1],
            _ =>             _drawers[0]
        };

        public string MakeDemotivator(string path, string textA, string textB)
        {
            return Drawer.DrawDemotivator(path, textA, textB);
        }

        public string MakeStickerDemotivator(string path, string textA, string textB, string extension)
        {
            Execute(new F_WebpToJpg(path, out path, extension));
            
            return MakeDemotivator(path, textA, textB);
        }

        public string MakeVideoStickerDemotivator(string path, string textA, string textB)
        {
            Execute(new F_WebmToMp4(path, out path, ".mp4", GetValidSize(path)));

            return MakeVideoDemotivator(path, textA, textB);
        }
        
        public string MakeVideoDemotivator(string path, string textA, string textB)
        {
            string inputFilePath = path;
            string inputFileName = path.Split('\\', '.')[^2];
            string animationPath = UniquePath($@"{CurrentDirectory}\{PICTURES_FOLDER}\{inputFileName}");
            string animationName = $"{inputFileName}-D.mp4";
            
            Directory.CreateDirectory(animationPath);

            var stream = GetMediaStream(path);
            
            string inFrameRate = stream.AvgFrameRate;
            string inFrames = stream.NbFrames;
            Log($"IN >>> FPS: {inFrameRate} Length: {inFrames}", ConsoleColor.Blue);

            double outFrameRate = RetrieveFPS(inFrameRate);
            int outFrames = int.Parse(inFrames);
            double k = 1;

            NormalizeLength(50);
            NormalizeFrameRate(50);
            double frameDelay = 1000 / outFrameRate;
            Log($"OUT >> FPS: {FormatDouble(Math.Round(outFrameRate, 1)).PadRight(inFrameRate.Length)} Length: {outFrames}", ConsoleColor.Blue);
            
            // Extract all frames
            for (var frame = 0; frame < outFrames; frame++)
            {
                var output = @$"{animationPath}\F-{frame:0000}.jpg";
                Execute(new F_SaveFrame(inputFilePath, output, TimeSpan.FromMilliseconds(k * frameDelay * frame)));
            }

            // Demotivate each frame
            var demotivator = Drawer.MakeFrame(textA, textB);
            var frames = GetAllFrames();
            foreach (string file in frames) Drawer.PasteImage(demotivator, file);

            var framesPath = @$"{animationPath}\F-%04d-D.jpg";
            var outputPath = $@"{animationPath}\{animationName}";

            Execute(new F_RenderAnimation(outFrameRate, new Size(360, 360), framesPath, outputPath));

            return outputPath;

            void NormalizeLength(int max)
            {
                if (outFrames > max)
                {
                    k = outFrames / (double) max;
                    outFrames = max;
                    //outFrameRate = (int)(outFrameRate * k); // <- this will ++ framerate after --ing length
                }
            }
            void NormalizeFrameRate(int max) => outFrameRate = Math.Min(outFrameRate, max);
            string[] GetAllFrames() => Directory.GetFiles(animationPath);
        }

        public string ChangeSpeed(string path, double speed, SpeedMode mode, MediaType type)
        {
            if (mode == SpeedMode.Slow) speed = 1 / speed;
            
            Log($"SPEED >> {FormatDouble(speed)}", ConsoleColor.Blue);

            WebmToMp4(ref path, out _);
            SetOutName(path, out string output, "-S");

            if (type != MediaType.Audio)
            {
                double fps = RetrieveFPS(GetMediaStream(path).AvgFrameRate, 30) * speed;
                Execute(new F_Speed(path, output, speed, type, Math.Min(fps, 90)));
            }
            else
                Execute(new F_Speed(path, output, speed, type));

            return output;
        }
        
        public string Sus(string path, TimeSpan start, TimeSpan length, MediaType type)
        {
            WebmToMp4(ref path, out _);
            
            if (length < TimeSpan.Zero) length = TimeSpan.FromSeconds(GetDurationInSeconds(path) / 2D);
            
            if (start != TimeSpan.Zero || length != TimeSpan.Zero)
            {
                Execute(new F_Cut(path, out path, start, length));
            }
            
            Execute(new F_Reverse(path, out string reversed));
            Execute(new F_Concat(path, reversed, out string output, type));

            return output;
        }
        
        public string Reverse(string path)
        {
            WebmToMp4(ref path, out _);
            
            Execute(new F_Reverse(path, out string output));
            return output;
        }
        
        public string Cut(string path, TimeSpan start, TimeSpan length)
        {
            WebmToMp4(ref path, out _);
            
            Execute(new F_Cut(path, out string output, start, length));
            return output;
        }

        public string RemoveBitrate(string path, int bitrate, out int value)
        {
            bool noArgs = bitrate == 0;
            string outputPath;
            F_RemoveBitrate task;
            
            WebmToMp4(ref path, out string extension);
            if (extension == ".mp4")
            {
                var size = GetValidSize(path, out var stream);
                double fps = RetrieveFPS(stream.AvgFrameRate, 30);
                if (noArgs)
                {
                    var pixelsPerSecond = (int) ((size.Height + size.Width) * fps);
                    bitrate = pixelsPerSecond / 620;
                }
                bitrate = Math.Clamp(bitrate, 1, noArgs ? (int) (40d * (fps / 30d)) : 120);
                Log($"DAMN >> -b:v {bitrate}k", ConsoleColor.Blue);
                
                task = new F_RemoveBitrate(path, out outputPath, bitrate, size);
            }
            else
                task = new F_RemoveBitrate(path, out outputPath, bitrate);

            Execute(task);
            value = bitrate;
            return outputPath;
        }

        private Size GetValidSize(string path, out MediaStream stream)
        {
            stream = GetMediaStream(path);
            int height = FallbackIfZero(stream.Height, 720);
            int width = FallbackIfZero(stream.Width, 720);

            if (width % 2 == 1 || height % 2 == 1) // РжакаБот / видеостикеры момент((9
                return new Size(NearestEven(width), NearestEven(height));
            return new Size(width, height);
        }
        private Size GetValidSize(string path) => GetValidSize(path, out _);
        private double GetDurationInSeconds(string path) => double.Parse(GetMediaStream(path).Duration, CultureInfo.InvariantCulture);
        private MediaStream GetMediaStream(string path) => GetMetadata(path).Result.Metadata.Streams.First();
        private async Task<GetMetadataResult> GetMetadata(string path) => await _service.ExecuteAsync(new FfTaskGetMetadata(path));

        private void Execute(F_Base task) => _service.ExecuteAsync(task).Wait();
        
        private int FallbackIfZero(int x, int alt) => x == 0 ? alt : x;
        private int NearestEven(int x) => x + x % 2;

        private double RetrieveFPS(string framerate, int alt = 16)
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

        private void WebmToMp4(ref string path, out string extension)
        {
            extension = GetFileExtension(path);
            if (extension == ".webm")
            {
                Execute(new F_WebmToMp4(path, out path, ".mp4", GetValidSize(path)));
                extension = ".mp4";
            }
        }
    }

    public enum DgMode
    {
        Square,
        Wide
    }
}