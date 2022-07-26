using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaToolkit.Model;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using Witlesss.Also;
using static Witlesss.Logger;
using static System.Environment;
using static Witlesss.Extension;
using static Witlesss.Also.Strings;

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
            var task = new FfTaskWebpToJpg(path, out string outputPath, extension);
            _service.ExecuteAsync(task).Wait();
            
            return MakeDemotivator(outputPath, textA, textB);
        }

        public string MakeVideoDemotivator(string path, string textA, string textB)
        {
            Drawer().SetRandomLogo();
            AnimateDemotivator(path, textA, textB).Wait();
            return $@"{_animationPath}\{_animationName}";
        }

        public string MakeVideoStickerDemotivator(string path, string textA, string textB)
        {
            var task = new FfTaskWebmToMp4(path, out path, ".mp4", GetValidSize(path));
            _service.ExecuteAsync(task).Wait();

            return MakeVideoDemotivator(path, textA, textB);
        }
        
        private async Task AnimateDemotivator(string path, string textA, string textB)
        {
            string inputFilePath = path;
            string inputFileName = path.Split('\\', '.')[^2];
            _animationPath = UniquePath($@"{CurrentDirectory}\{PICTURES_FOLDER}\{inputFileName}");
            
            Directory.CreateDirectory(_animationPath);

            var metadata = await _service.ExecuteAsync(new FfTaskGetMetadata(inputFilePath));
            
            string inFrameRate = metadata.Metadata.Streams.First().AvgFrameRate;
            string inFrames = metadata.Metadata.Streams.First().NbFrames;
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
                var task = new FfTaskSaveFrame(inputFilePath, output, TimeSpan.FromMilliseconds(k * frameDelay * frame));
                _service.ExecuteAsync(task).Wait();
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
                var task = new FfTaskRenderAnimation(outFrameRate, size, framesPath, outputPath);
                _service.ExecuteAsync(task).Wait();
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

            FfMpegTaskBase<int> task = null;
            switch (type)
            {
                case MediaType.Audio:
                    task = new FfTaskSpeedA(path, output, speed);
                    break;
                case MediaType.Video:
                    task = new FfTaskSpeedV(path, output, speed);
                    break;
                case MediaType.AudioVideo:
                    task = new FfTaskSpeedAV(path, output, speed);
                    break;
            }
            _service.ExecuteAsync(task).Wait();
            return output;
        }

        public string RemoveBitrate(string path, int bitrate, out int value)
        {
            bool noArgs = bitrate == 0;
            string outputPath;
            FfTaskRemoveBitrate task;
            
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
                
                task = new FfTaskRemoveBitrate(path, out outputPath, bitrate, size);
            }
            else
                task = new FfTaskRemoveBitrate(path, out outputPath, bitrate);

            _service.ExecuteAsync(task).Wait();
            value = bitrate;
            return outputPath;
        }

        private Size GetValidSize(string path, out MediaStream stream)
        {
            var metadata = GetMetadata(path).Result;
            stream = metadata.Metadata.Streams.First();
            int height = FallbackIfZero(stream.Height, 720);
            int width = FallbackIfZero(stream.Width, 720);

            if (width % 2 == 1 || height % 2 == 1) // РжакаБот / видеостикеры момент((9
                return new Size(NearestEven(width), NearestEven(height));
            return new Size(width, height);
        }
        private Size GetValidSize(string path) => GetValidSize(path, out _);
        private async Task<GetMetadataResult> GetMetadata(string path) => await _service.ExecuteAsync(new FfTaskGetMetadata(path));
        
        private int FallbackIfZero(int x, int alt) => x == 0 ? alt : x;
        private int NearestEven(int x) => x + x % 2;

        private double RetrieveFPS(string framerate, int alt = 16)
        {
            string[] fps = framerate.Split('/');
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
                _service.ExecuteAsync(new FfTaskWebmToMp4(path, out path, ".mp4", GetValidSize(path))).Wait();
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