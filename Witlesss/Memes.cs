using System;
using System.Drawing;
using Telegram.Bot.Types;
using Witlesss.MediaTools;
using static Witlesss.MediaTools.FF_Extensions;
using TS = System.TimeSpan;

namespace Witlesss
{
    public class Memes
    {
        private readonly DemotivatorDrawer [] _drawers = { new(), new(1280) };
        private readonly MemeGenerator        _imgflip = new();
        private static   Size SourceSize  = Size.Empty;

        private static int Quality => JpegCoder.Quality > 80 ? 0 : 51 - (int)(JpegCoder.Quality * 0.42); // 0 | 17 - 51

        public static void PassSize(Video     v) => SourceSize = new Size(v.Width, v.Height);
        public static void PassSize(Sticker   s) => SourceSize = new Size(s.Width, s.Height);
        public static void PassSize(Animation a) => SourceSize = new Size(a.Width, a.Height);
        public static void PassSize(PhotoSize p) => SourceSize = new Size(p.Width, p.Height);
        public static void PassSize(int       i) => SourceSize = new Size(i, i);
        
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
            _imgflip.SetUp(SourceSize);

            return new F_Overlay(path, _imgflip.BakeCaption(text)).Meme(Quality);
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
        public static string Stickerize    (string path) => new F_Resize(path).ToSticker(NormalizeSize(SourceSize));
        public static string Compress      (string path) => new F_Resize(path).CompressImage();
        public static string CompressGIF   (string path) => new F_Resize(path).CompressAnimation();
        public static string CropVideoNote (string path) => new F_Resize(path).CropVideoNote();
        public static string ToVideoNote   (string path)
        {
            var d = ToEven(Math.Min(SourceSize.Width, SourceSize.Height));
            var x = (SourceSize.Width  - d) / 2;
            var y = (SourceSize.Height - d) / 2;

            return new F_Resize(path).ToVideoNote(new Rectangle(x, y, d, d));
        }

        public  static Size FitSize      (Size s, int max =  1280)
        {
            if (s.Width > max || s.Height > max) s = NormalizeSize(s, max);
            return ValidSize(s.Width, s.Height);
        }
        private static Size NormalizeSize(Size s, int limit = 512)
        {
            double lim = limit;
            return s.Width > s.Height
                ? new Size(limit, (int)(s.Height / (s.Width / lim)))
                : new Size((int)(s.Width / (s.Height / lim)), limit);
        }
    }
}