using System;
using System.Drawing;
using System.IO;
using System.Linq;
using MediaToolkit.Model;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using Telegram.Bot.Types;
using Witlesss.MediaTools;
using static System.Globalization.CultureInfo;
using static Witlesss.JpegCoder;
using MD = System.Threading.Tasks.Task<MediaToolkit.Tasks.GetMetadataResult>;
using TS = System.TimeSpan;

namespace Witlesss
{
    public class Memes
    {
        private readonly DemotivatorDrawer [] _drawers;
        private readonly MemeGenerator        _imgflip;
        private readonly IMediaToolkitService _service;
        public  static   Size SourceSize  = Size.Empty;

        public static void PassSize(Video     v) => SourceSize = new Size(v.Width, v.Height);
        public static void PassSize(Sticker   s) => SourceSize = new Size(s.Width, s.Height);
        public static void PassSize(Animation a) => SourceSize = new Size(a.Width, a.Height);
        public static void PassSize(PhotoSize p) => SourceSize = new Size(p.Width, p.Height);

        public Memes()
        {
            _drawers = new[] { new DemotivatorDrawer(), new DemotivatorDrawer(1280) };
            _imgflip = new MemeGenerator();

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
            var quality = JpegQuality > 50 ? 0 : 51 - (int)(JpegQuality * 0.42);

            return Execute(new F_Overlay(Drawer.MakeFrame(text), path, Drawer.Size, Drawer.Pic, quality));
        }

        public string MakeMeme(string path, DgText text)
        {
            return _imgflip.MakeImpactMeme(path, text);
        }

        public string MakeMemeFromSticker(string path, DgText text, string extension)
        {
            return MakeMeme(Execute(new F_ToJPG(path, extension)), text);
        }

        public string MakeVideoMeme(string path, DgText text)
        {
            var quality = JpegQuality > 50 ? 0 : 51 - (int)(JpegQuality * 0.42);

            _imgflip.SetUp(SourceSize);

            return Execute(new F_Overlay(path, _imgflip.BakeCaption(text), SourceSize, Point.Empty, quality));
        }

        public string Stickerize(string path) => Execute(new F_Resize(path, NormalizeSize(SourceSize), ".webp"));

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
        
        public string Sus(string path, CutSpan s, MediaType type)
        {
            var b = IsWEBM(path) && SizeIsInvalid(SourceSize);
            if (b) path = Execute(new F_Resize(path, CorrectedSize(SourceSize)));

            if (s.Length < TS.Zero) s = s with { Length = TS.FromSeconds(GetDuration(path) / 2D) };

            if ((s.Start + s.Length).Ticks > 0) path = Cut(path, s, type);

            return Execute(new F_Concat(path, Reverse(path, type), type));
        }
        
        public string Reverse(string path, MediaType type)        => Execute(new F_Reverse(path, type));

        public string Cut(string path, CutSpan s, MediaType type) => Execute(new F_Cut (path, s, type));

        public string RemoveAudio(string path) => Execute(new F_ToAnimation(path, FitSize(SourceSize)));

        public string RemoveBitrate(string path, ref int bitrate, MediaType type)
        {
            if (type > MediaType.Audio)
            {
                Log($"DAMN >> {bitrate}", ConsoleColor.Blue);

                bitrate += 30;

                return Execute(new F_Bitrate(path, bitrate, type));
            }
            else
                return Execute(new F_Bitrate(path,    type: type));
        }

        private double   GetDuration (string path) => double.Parse(GetMedia(path).Duration, InvariantCulture);
        private MediaStream GetMedia (string path) => GetMetadata(path).Result.Metadata.Streams.First();
        private async MD GetMetadata (string path) => await _service.ExecuteAsync(new FfTaskGetMetadata(path));

        private string Execute(F_Base task) => _service.ExecuteAsync(task).Result;

        private static int ToEven (int x) => x + x % 2;

        public  static bool IsWEBM  (string path) => Path.GetExtension(path) == ".webm";
        public  static bool SizeIsInvalid(Size s) => (s.Width | s.Height) % 2 > 0;
        public  static Size CorrectedSize(Size s) => new(ToEven(s.Width), ToEven(s.Height));
        private static Size NormalizeSize(Size s, int limit = 512)
        {
            double lim = limit;
            if (s.Width > s.Height)
            {
                return new Size(limit, (int)(s.Height / (s.Width / lim)));
            }
            else
            {
                return new Size((int)(s.Width / (s.Height / lim)), limit);
            }
        }
        private static Size FitSize      (Size s, int max = 1280)
        {
            if (s.Width > max || s.Height > max) s = NormalizeSize(s, max);
            return CorrectedSize(s);
        }

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
    }

    public enum DgMode { Square, Wide }
}