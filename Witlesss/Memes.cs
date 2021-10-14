using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnimatedGif;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using Witlesss.Also;
using static Witlesss.Logger;
using static System.Environment;
using static Witlesss.Also.Extension;
using static Witlesss.Also.Strings;
using Image = System.Drawing.Image;

namespace Witlesss
{
    public class Memes
    {
        private readonly DemotivatorDrawer _drawer;
        private readonly IMediaToolkitService _service;
        
        private string _animationPath;

        public Memes()
        {
            _drawer = new DemotivatorDrawer();
            _service = MediaToolkitService.CreateInstance(FFMPEG_PATH);
        }


        public string MakeDemotivator(string path, string textA, string textB)
        {
            _drawer.SetRandomLogo();
            return _drawer.DrawDemotivator(path, textA, textB);
        }

        public string MakeAnimatedDemotivator(string path, string textA, string textB)
        {
            _drawer.SetRandomLogo();
            AnimateDemotivator(path, textA, textB).Wait();
            return $@"{_animationPath}\D.gif";
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

            string[] fps = inFrameRate.Split('/');
            int outFrameRate = int.Parse(fps[0]) / int.Parse(fps[1]);
            int outFrames = int.Parse(inFrames);
            double k = 1;

            NormalizeFrameRate(50);
            NormalizeLength(50);
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
                using var gif = new AnimatedGifCreator($@"{_animationPath}\D.gif", frameDelay);
                frames = GetAllFrames();
                foreach (string file in frames)
                    if (file.EndsWith("D.jpg"))
                        gif.AddFrameAsync(Image.FromFile(file), quality: GifQuality.Bit8).Wait();
            }
            catch (Exception e)
            {
                Log(e.Message, ConsoleColor.Red);
            }
            
            
            void NormalizeFrameRate(int max)
            {
                if (outFrameRate > max)
                {
                    outFrames /= outFrameRate / max;
                    outFrameRate = max;
                }
            }
            void NormalizeLength(int max)
            {
                if (outFrames > max)
                {
                    k = outFrames / (double) max;
                    outFrames = max;
                }
            }
            string[] GetAllFrames() => Directory.GetFiles(_animationPath);
        }
    }
}