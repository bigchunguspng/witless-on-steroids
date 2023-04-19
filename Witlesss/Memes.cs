using System;
using System.Drawing;
using Witlesss.MediaTools;
using TS = System.TimeSpan;
using static Witlesss.MediaTools.FF_Extensions;

namespace Witlesss
{
    public class Memes
    {
        private readonly DemotivatorDrawer [] _drawers = { new(), new(1280) };
        private readonly MemeGenerator        _imgflip = new();
        private readonly TopTextAttacher      _kapwing = new();

        private static int Quality => JpegCoder.Quality > 80 ? 0 : 51 - (int)(JpegCoder.Quality * 0.42); // 0 | 17 - 51

        public static readonly Size      VideoNoteSize = new(384, 384);
        public static readonly Rectangle VideoNoteCrop = new(56, 56, 272, 272);

        public DgMode Mode;

        private DemotivatorDrawer Drawer => _drawers[(int) Mode];

        public string MakeDemotivator(string path, DgText text)
        {
            return Drawer.DrawDemotivator(path, text);
        }

        public string MakeStickerDemotivator(string path, DgText text, string extension)
        {
            return MakeDemotivator(new F_Resize(path).Transcode(extension), text);
        }

        public string MakeVideoDemotivator(string path, DgText text)
        {
            return new F_Overlay(Drawer.MakeFrame(text), path).Demo(Quality, Drawer);
        }

        public string MakeMeme(string path, DgText text)
        {
            return _imgflip.MakeImpactMeme(path, text);
        }

        public string MakeMemeFromSticker(string path, DgText text, string extension)
        {
            return MakeMeme(new F_Resize(path).Transcode(extension), text);
        }

        public string MakeVideoMeme(string path, DgText text)
        {
            var size = GrowSize(GetSize(path));
            _imgflip.SetUp(size);

            return new F_Overlay(path, _imgflip.BakeCaption(text)).Meme(Quality, size);
        }
        
        public string MakeWhenTheMeme(string path, string text)
        {
            return _kapwing.MakeTopTextMeme(path, text);
        }

        public string MakeWhenTheMemeFromSticker(string path, string text, string extension)
        {
            return MakeWhenTheMeme(new F_Resize(path).Transcode(extension), text);
        }

        public string MakeVideoWhenTheMeme(string path, string text)
        {
            var size = GrowSize(GetSize(path));
            _kapwing.SetUp(size);

            return new F_Overlay(_kapwing.BakeText(text), path).When(Quality, _kapwing.Cropping);
        }

        public static string ChangeSpeed(string path, double speed, SpeedMode mode)
        {
            if (mode == SpeedMode.Slow) speed = 1 / speed;
            
            Log($"SPEED >> {FormatDouble(speed)}", ConsoleColor.Blue);

            return new F_Speed(path, speed).ChangeSpeed();
        }

        public static string RemoveBitrate(string path, int crf)
        {
            Log($"DAMN >> {crf}", ConsoleColor.Blue);

            return new F_Bitrate(path, crf).Compress();
        }
        
        public static string Sus(string path, CutSpan s) => new F_Cut(path, s).Sus();
        public static string Cut(string path, CutSpan s) => new F_Cut(path, s).Cut();

        public static string Reverse       (string path) => new F_Reverse(path).Reverse();

        public static string RemoveAudio   (string path) => new F_Resize(path).ToAnimation();
        public static string Stickerize    (string path) => new F_Resize(path).ToSticker(NormalizeSize(GetSize(path)));
        public static string Compress      (string path) => new F_Resize(path).CompressImage(FitSize(GetSize(path), 2560));
        public static string CompressGIF   (string path) => new F_Resize(path).CompressAnimation();
        public static string CropVideoNote (string path) => new F_Resize(path).CropVideoNote();
        public static string ToVideoNote   (string path)
        {
            var s = GetSize(path);
            var d = ToEven(Math.Min(s.Width, s.Height));
            var x = (s.Width  - d) / 2;
            var y = (s.Height - d) / 2;

            return new F_Resize(path).ToVideoNote(new Rectangle(x, y, d, d));
        }

        private static Size GrowSize     (Size s, int min =  256)
        {
            if (s.Width < min || s.Height < min) s = NormalizeSize(s, min, reduce: false);
            return ValidSize(s.Width, s.Height);
        }
        public  static Size FitSize      (Size s, int max =  1280)
        {
            if (s.Width > max || s.Height > max) s = NormalizeSize(s, max);
            return ValidSize(s.Width, s.Height);
        }
        private static Size NormalizeSize(Size s, int limit = 512, bool reduce = true)
        {
            double lim = limit;
            var wide = s.Width > s.Height;
            return reduce == wide
                ? new Size(limit, (int)(s.Height / (s.Width / lim)))
                : new Size((int)(s.Width / (s.Height / lim)), limit);
        }
        private static Size GetSize(string path)
        {
            var v = F_SingleInput_Base.GetVideoStream(path);
            return new Size(v.Width, v.Height);
        }
    }
}