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
        private readonly DemotivatorDrawer _drawer720, _drawer1280;
        private readonly IMediaToolkitService _service;
        
        private string _animationPath;
        private string _animationName;

        public Memes()
        {
            _drawer720 = new DemotivatorDrawer();
            _drawer1280 = new DemotivatorDrawer(1280);

            if (!File.Exists(FFMPEG_PATH))
            {
                Log(@$"""{FFMPEG_PATH}"" не найден. Поместите его туда или оформите вылет", ConsoleColor.Yellow);
                Log("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
            _service = MediaToolkitService.CreateInstance(FFMPEG_PATH);
        }
        
        public DgMode Mode { get; set; }

        private DemotivatorDrawer Drawer()
        {
            switch (Mode)
            {
                case DgMode.Square:
                    return _drawer720;
                case DgMode.Wide:
                    return _drawer1280;
                default:
                    return _drawer720;
            }
        }

        public string MakeDemotivator(string path, string textA, string textB)
        {
            Drawer().SetRandomLogo();
            return Drawer().DrawDemotivator(path, textA, textB);
        }

        public string MakeStickerDemotivator(string path, string textA, string textB, string extension)
        {
            Execute(new F_WebpToJpg(path, out path, extension));
            
            return MakeDemotivator(path, textA, textB);
        }

        public string MakeVideoDemotivator(string path, string textA, string textB)
        {
            Drawer().SetRandomLogo();
            AnimateDemotivator(path, textA, textB);
            return $@"{_animationPath}\{_animationName}";
        }

        public string MakeVideoStickerDemotivator(string path, string textA, string textB)
        {
            Execute(new F_WebmToMp4(path, out path, ".mp4", GetValidSize(path)));

            return MakeVideoDemotivator(path, textA, textB);
        }
        
        private void AnimateDemotivator(string path, string textA, string textB)
        {
            string inputFilePath = path;
            string inputFileName = path.Split('\\', '.')[^2];
            _animationPath = UniquePath($@"{CurrentDirectory}\{PICTURES_FOLDER}\{inputFileName}");
            
            Directory.CreateDirectory(_animationPath);

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
            Log($"OUT >> FPS: {Math.Round(outFrameRate, 1).ToString(CultureInfo.InvariantCulture).PadRight(inFrameRate.Length)} Length: {outFrames}", ConsoleColor.Blue);
            
            // Extract all frames
            for (var frame = 0; frame < outFrames; frame++)
            {
                var output = @$"{_animationPath}\F-{frame:0000}.jpg";
                Execute(new F_SaveFrame(inputFilePath, output, TimeSpan.FromMilliseconds(k * frameDelay * frame)));
            }

            // Demotivate each frame
            string[] frames = GetAllFrames();
            foreach (string file in frames) Drawer().DrawDemotivator(file, textA, textB);

            try
            {
                // Render GIF
                _animationName = $"{inputFileName}-D.mp4";
                var framesPath = @$"{_animationPath}\F-%04d-D.jpg";
                var outputPath = $@"{_animationPath}\{_animationName}";
                
                var size = new Size(360, 360);
                Execute(new F_RenderAnimation(outFrameRate, size, framesPath, outputPath));
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }

            void NormalizeLength(int max)
            {
                if (outFrames > max)
                {
                    k = outFrames / (double) max;
                    outFrames = max;
                    //outFrameRate = (int)(outFrameRate * k); // <- this will ++ framerate after --ing length
                }
            }
            void NormalizeFrameRate(int max)
            {
                if (outFrameRate > max)
                    outFrameRate = max;
            }
            string[] GetAllFrames() => Directory.GetFiles(_animationPath);
        }

        public string ChangeSpeed(string path, double speed, SpeedMode mode, MediaType type)
        {
            if (mode == SpeedMode.Slow) speed = 1 / speed;
            
            Log($"SPEED >> {speed.ToString(CultureInfo.InvariantCulture)}", ConsoleColor.Blue);

            string extension = GetFileExtension(path);
            WebmToMp4(ref extension, ref path);
            string output = path.Remove(path.LastIndexOf('.')) + "-S" + extension;

            F_Base task = null;
            switch (type)
            {
                case MediaType.Audio:
                    task = new F_SpeedA(path, output, speed);
                    break;
                case MediaType.Video:
                    task = new F_SpeedV(path, output, speed);
                    break;
                case MediaType.AudioVideo:
                    task = new F_SpeedAV(path, output, speed);
                    break;
            }
            Execute(task);
            return output;
        }
        
        public string Sus(string path, TimeSpan start, TimeSpan length, MediaType type)
        {
            string extension = GetFileExtension(path);
            WebmToMp4(ref extension, ref path);
            
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
            string extension = GetFileExtension(path);
            WebmToMp4(ref extension, ref path);
            
            Execute(new F_Reverse(path, out string output));
            return output;
        }
        
        public string Cut(string path, TimeSpan start, TimeSpan length)
        {
            string extension = GetFileExtension(path);
            WebmToMp4(ref extension, ref path);
            
            Execute(new F_Cut(path, out string output, start, length));
            return output;
        }

        public string RemoveBitrate(string path, int bitrate, out int value)
        {
            bool noArgs = bitrate == 0;
            string outputPath;
            F_RemoveBitrate task;
            
            string extension = GetFileExtension(path);
            WebmToMp4(ref extension, ref path);
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

        private void WebmToMp4(ref string extension, ref string path)
        {
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