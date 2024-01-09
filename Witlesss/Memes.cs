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
            return MakeDemotivator(Convert(path, extension), text);
        }

        public static string MakeVideoDemotivator(string path, DgText text)
        {
            return new F_Combine(path, Drawer.MakeFrame(text)).Demo(Quality, Drawer).Output("-D");
        }


        public static string MakeDemotivatorB(string path, string text)
        {
            return _dp.DrawDemotivator(path, text);
        }

        public static string MakeStickerDemotivatorB(string path, string text, string extension)
        {
            return MakeDemotivatorB(Convert(path, extension), text);
        }

        public static string MakeVideoDemotivatorB(string path, string text)
        {
            _dp.PassTextLength(text);

            var size = GrowSize(GetSize(path));
            _dp.SetUp(size);
            _dp.SetColor();

            var frame = _dp.BakeFrame(text);
            var full_size = FitSize(GetSize(frame), 720);

            return new F_Combine(path, frame).D300(Quality, size, _dp.Location, full_size).Output("-Dp");
        }


        public static string MakeMeme(string path, DgText text)
        {
            return _imgflip.MakeImpactMeme(path, text);
        }

        public static string MakeMemeFromSticker(string path, DgText text, string extension)
        {
            return MakeMeme(Convert(path, extension), text);
        }

        public static string MakeVideoMeme(string path, DgText text)
        {
            Sticker = false;
            var size = GrowSize(GetSize(path));
            _imgflip.SetUp(size);

            return new F_Combine(path, _imgflip.BakeCaption(text)).Meme(Quality, size).Output("-M");
        }


        public static string MakeCaptionMeme(string path, string text)
        {
            return _ifunny.MakeCaptionMeme(path, text);
        }

        public static string MakeCaptionMemeFromSticker(string path, string text, string extension)
        {
            return MakeCaptionMeme(Convert(path, extension), text);
        }

        public static string MakeVideoCaptionMeme(string path, string text)
        {
            var size = GrowSize(GetSize(path));
            _ifunny.SetUp(size);

            if  (IFunnyApp.UseGivenColor) _ifunny.SetCustomColors();
            else if (IFunnyApp.PickColor) _ifunny.SetSpecialColors(new Bitmap(Image.FromFile(Snapshot(path))));
            else                          _ifunny.SetDefaultColors();

            return new F_Combine(path, _ifunny.BakeText(text)).When(Quality, size, _ifunny.Cropping, _ifunny.Location, IFunnyApp.BlurImage).Output("-Top");
        }


        private static string Convert(string path, string extension)
        {
            var ffmpeg = new F_Process(path);
            var name = ffmpeg.GetOutputName("-W", extension);
            try
            {
                return ffmpeg.OutputAs(name);
            }
            catch // av_interleaved_write_frame(): I/O error
            {
                return name; // file is already exist - just use it
            }
        }


        public static string ChangeSpeed(string path, double speed, SpeedMode mode)
        {
            if (mode == SpeedMode.Slow) speed = 1 / speed;
            
            Log($"SPEED >> {FormatDouble(speed)}", ConsoleColor.Blue);

            return new F_Process(path).ChangeSpeed(speed).Output_WEBM_safe("-S");
        }

        public static string RemoveBitrate(string path, int crf)
        {
            Log($"DAMN >> {crf}", ConsoleColor.Blue);

            return new F_Process(path).Compress(crf).Output_WEBM_safe("-DAMN");
        }
        
        public static string Sus(string path, CutSpan s) => new F_Cut(path, s).Sus().Output_WEBM_safe("-Sus");
        public static string Cut(string path, CutSpan s) => new F_Cut(path, s).Cut().Output_WEBM_safe("-Cut");

        public static string Reverse       (string path) => new F_Process(path).Reverse().Output_WEBM_safe("-RVR");

        public static string RemoveAudio   (string path) => new F_Process(path).ToAnimation().Output("-silent");
        public static string Stickerize    (string path) => new F_Process(path).ToSticker(NormalizeSize(GetSize(path))).Output("-stick", ".webp");
        public static string Compress      (string path) => new F_Process(path).CompressImage(FitSize(GetSize(path), 2560)).Output("-small", ".jpg");
        public static string CompressGIF   (string path) => new F_Process(path).CompressAnimation().Output("-small");
        public static string CropVideoNote (string path) => new F_Process(path).CropVideoNote().Output("-crop");
        public static string Crop          (string path, string[] args) => new F_Process(path).CropVideo (args).Output("-crop");
        public static string Scale         (string path, string[] args) => new F_Process(path).ScaleVideo(args).Output("-s");
        public static string ChangeVolume  (string path, string   arg ) => new F_Process(path).ChangeVolume(arg).Output("-vol");
        public static string EQ            (string path, double[] args) => new F_Process(path).EQ(args).Output("-EQ");
        public static string Edit          (string path, string options, string extension)
        {
            return new F_Process(path).Edit(options).Output("-Edit", "." + extension);
        }
        public static string ToVoice       (string path) => new F_Process(path).ToVoiceMessage().Output("-voice", ".ogg");
        public static string ToVideoNote   (string path)
        {
            var s = GetSize(path);
            var d = ToEven(Math.Min(s.Width, s.Height));
            var x = (s.Width  - d) / 2;
            var y = (s.Height - d) / 2;

            return new F_Process(path).ToVideoNote(new Rectangle(x, y, d, d)).Output("-vnote");
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
            var v = F_Action.GetVideoStream(path);
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