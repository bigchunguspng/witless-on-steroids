using System;
using System.Drawing;
using FFMpegCore;
using Witlesss.MediaTools;
using static Witlesss.MediaTools.FF_Extensions;

namespace Witlesss
{
    public static class Memes
    {
        private static readonly DemotivatorDrawer [] _drawers = { new(), new(1280) };
        private static readonly MemeGenerator        _imgflip = new();
        private static readonly IFunnyApp            _ifunny  = new();
        private static readonly DynamicDemotivatorDrawer  _dp = new();

        private static DemotivatorDrawer Drawer => _drawers[(int) Mode];

        private static int Quality => JpegCoder.Quality > 80 ? 0 : 51 - (int)(JpegCoder.Quality * 0.42); // 0 | 17 - 51

        public static readonly Size      VideoNoteSize = new(384, 384);
        public static readonly Rectangle VideoNoteCrop = new(56, 56, 272, 272);

        public static bool Sticker;
        public static DgMode Mode;


        public static string MakeDemotivator(string path, DgText text)
        {
            return Drawer.DrawDemotivator(path, text);
        }

        public static string MakeStickerDemotivator(string path, DgText text, string extension)
        {
            return MakeDemotivator(new F_Resize(path).Transcode(extension), text);
        }

        public static string MakeVideoDemotivator(string path, DgText text)
        {
            return new F_Overlay(path, Drawer.MakeFrame(text)).Demo(Quality, Drawer);
        }


        public static string MakeDemotivatorB(string path, string text)
        {
            return _dp.DrawDemotivator(path, text);
        }

        public static string MakeStickerDemotivatorB(string path, string text, string extension)
        {
            return MakeDemotivatorB(new F_Resize(path).Transcode(extension), text);
        }

        public static string MakeVideoDemotivatorB(string path, string text)
        {
            _dp.PassTextLength(text);

            var size = GrowSize(GetSize(path));
            _dp.SetUp(size);
            _dp.SetColor();

            var frame = _dp.BakeFrame(text);
            var full_size = FitSize(GetSize(frame), 720);

            return new F_Overlay(path, frame).D300(Quality, size, _dp.Location, full_size);
        }


        public static string MakeMeme(string path, DgText text)
        {
            return _imgflip.MakeImpactMeme(path, text);
        }

        public static string MakeMemeFromSticker(string path, DgText text, string extension)
        {
            return MakeMeme(new F_Resize(path).Transcode(extension), text);
        }

        public static string MakeVideoMeme(string path, DgText text)
        {
            Sticker = false;
            var size = GrowSize(GetSize(path));
            _imgflip.SetUp(size);

            return new F_Overlay(path, _imgflip.BakeCaption(text)).Meme(Quality, size);
        }


        public static string MakeCaptionMeme(string path, string text)
        {
            return _ifunny.MakeCaptionMeme(path, text);
        }

        public static string MakeCaptionMemeFromSticker(string path, string text, string extension)
        {
            return MakeCaptionMeme(new F_Resize(path).Transcode(extension), text);
        }

        public static string MakeVideoCaptionMeme(string path, string text)
        {
            var size = GrowSize(GetSize(path));
            _ifunny.SetUp(size);

            if  (IFunnyApp.UseGivenColor) _ifunny.SetCustomColors();
            else if (IFunnyApp.PickColor) _ifunny.SetSpecialColors(new Bitmap(Image.FromFile(Snapshot(path))));
            else                          _ifunny.SetDefaultColors();

            return new F_Overlay(path, _ifunny.BakeText(text)).When(Quality, size, _ifunny.Cropping, _ifunny.Location, IFunnyApp.BlurImage);
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
        public static string Crop          (string path, string[] args) => new F_Resize(path).CropVideo (args);
        public static string Scale         (string path, string[] args) => new F_Resize(path).ScaleVideo(args);
        public static string ToVideoNote   (string path)
        {
            var s = GetSize(path);
            var d = ToEven(Math.Min(s.Width, s.Height));
            var x = (s.Width  - d) / 2;
            var y = (s.Height - d) / 2;

            return new F_Resize(path).ToVideoNote(new Rectangle(x, y, d, d));
        }

        private static Size GrowSize      (Size s, int minWH = 400)
        {
            if (s.Width + s.Height < minWH)
            {
                var ratio = s.Width / (float)s.Height;
                var wide = s.Width > s.Height;
                var lim = (int)(minWH * ratio / (ratio + 1));
                s = NormalizeSize(s, lim, reduce: wide); // lim = W
            }

            return ValidSize(s.Width, s.Height);
        }
        public  static Size FitSize       (Size s, int max = 1280)
        {
            if (s.Width > max || s.Height > max) s = NormalizeSize(s, max);
            return ValidSize(s.Width, s.Height);
        }
        public static Size NormalizeSize (Size s, int limit = 512, bool reduce = true)
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

        private static string Snapshot(string path)
        {
            var temp = JpegCoder.GetTempPicName();
            FFMpeg.Snapshot(path, temp);
            return temp;
        }
    }
    
    public record DgText(string A, string B);
    
    public record CutSpan(TimeSpan Start, TimeSpan Length);
}