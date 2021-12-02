using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using Witlesss.Also;
using static Witlesss.Logger;
using static System.Environment;
using static Witlesss.Also.Extension;
using static Witlesss.Also.Strings;

namespace Witlesss
{
    public class Memes
    {
        private readonly DemotivatorDrawer _drawer;
        private readonly IMediaToolkitService _service;
        
        private string _animationPath;
        private string _animationName;

        public Memes()
        {
            _drawer = new DemotivatorDrawer();

            if (!File.Exists(FFMPEG_PATH))
            {
                Log(@$"""{FFMPEG_PATH}"" не найден. Поместите его туда или оформите вылет", ConsoleColor.Yellow);
                Log("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
            _service = MediaToolkitService.CreateInstance(FFMPEG_PATH);
        }


        public string MakeDemotivator(string path, string textA, string textB)
        {
            _drawer.SetRandomLogo();
            return _drawer.DrawDemotivator(path, textA, textB);
        }

        public string MakeStickerDemotivator(string path, string textA, string textB, string extension)
        {
            var task = new FfTaskWebpToJpg(path, out string outputPath, extension);
            _service.ExecuteAsync(task).Wait();
            
            return MakeDemotivator(outputPath, textA, textB);
        }

        public string MakeAnimatedDemotivator(string path, string textA, string textB)
        {
            _drawer.SetRandomLogo();
            AnimateDemotivator(path, textA, textB).Wait();
            return $@"{_animationPath}\{_animationName}";
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
            Log($"In: FPS: {inFrameRate}, Length: {inFrames}", ConsoleColor.Blue);

            int outFrameRate = RetrieveFPS(inFrameRate);
            int outFrames = int.Parse(inFrames);
            double k = 1;

            NormalizeLength(50);
            NormalizeFrameRate(50);
            int frameDelay = 1000 / outFrameRate;
            Log($"Out: FPS: {outFrameRate}, Length: {outFrames}", ConsoleColor.Blue);
            
            // Extract all frames
            for (var frame = 0; frame < outFrames; frame++)
            {
                var output = @$"{_animationPath}\F-{frame:0000}.jpg";
                var task = new FfTaskSaveFrame(inputFilePath, output, TimeSpan.FromMilliseconds(k * frameDelay * frame));
                _service.ExecuteAsync(task).Wait();
            }

            // Demotivate each frame
            string[] frames = GetAllFrames();
            foreach (string file in frames) _drawer.DrawDemotivator(file, textA, textB);

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
                Log(e.Message, ConsoleColor.Red);
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

        public string RemoveBitrate(string path, int bitrate)
        {
            bool noArgs = bitrate == 0;
            string outputPath;
            FfTaskRemoveBitrate task;
            if (GetFileExtension(path) == ".mp4")
            {
                Size size = default;
                CalculateOutBitrate().Wait();
                task = new FfTaskRemoveBitrate(path, out outputPath, bitrate, size);
                
                async Task CalculateOutBitrate()
                {
                    var metadata = await _service.ExecuteAsync(new FfTaskGetMetadata(path));
                    var stream = metadata.Metadata.Streams.First();
                    int height = stream.Height;
                    int width = stream.Width;
                    if (width % 2 == 1 || height % 2 == 1) // РжакаБот момент((9
                        size = new Size(NearestEven(width), NearestEven(height));
                    int fps = RetrieveFPS(stream.AvgFrameRate, 30);
                    if (noArgs)
                    {
                        int pixelsPerSecond = (height + width) * fps;
                        bitrate = pixelsPerSecond / 620;
                    }

                    bitrate = Math.Clamp(bitrate, 1, noArgs ? (int) (40d * (fps / 30d)) : 120);
                    Log($"Damn! -b:v {bitrate}k", ConsoleColor.Blue);

                    int NearestEven(int x) => x + x % 2;
                }
            }
            else
                task = new FfTaskRemoveBitrate(path, out outputPath, bitrate);

            _service.ExecuteAsync(task).Wait();
            return outputPath;
        }

        private int RetrieveFPS(string framerate, int alt = 16)
        {
            string[] fps = framerate.Split('/');
            try
            {
                return int.Parse(fps[0]) / int.Parse(fps[1]);
            }
            catch
            {
                return alt;
            }
        }
    }
}