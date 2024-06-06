using System;
using System.IO;
using FFMpegCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Witlesss.Backrooms;
using Witlesss.MediaTools;
using static Witlesss.Backrooms.SizeHelpers;
using static Witlesss.MediaTools.FF_Extensions;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Witlesss
{
    public static class Memes
    {
        private static readonly DemotivatorDrawer [] _drawers = { new(), new(1280) };
        private static readonly MemeGenerator        _imgflip = new();
        private static readonly IFunnyApp            _ifunny  = new();
        private static readonly DynamicDemotivatorDrawer  _dp = new();

        private static DemotivatorDrawer Drawer => _drawers[(int) Mode];

        private static int Quality => ImageSaver.Quality > 80 ? 0 : 51 - (int)(ImageSaver.Quality * 0.42); // 0 | 17 - 51
        private static int Qscale  => 31 + (int)(-0.29 * (int)ImageSaver.Quality); // 2 - 31

        public static readonly Size      VideoNoteSize = new(384, 384);
        public static readonly Rectangle VideoNoteCrop = new(56, 56, 272, 272);

        public static bool Sticker;
        public static DgMode Mode;


        public static string MakeDemotivator(string path, DgText text)
        {
            return Drawer.MakeDemotivator(path, text);
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
            return _imgflip.MakeMeme(path, text);
        }

        public static string MakeMemeFromSticker(string path, DgText text, string extension)
        {
            return MakeMeme(Convert(path, extension), text);
        }

        public static string MakeVideoMeme(string path, DgText text)
        {
            Sticker = false;
            var size = GetImageSize_FFmpeg(path).GrowSize().ValidMp4Size();
            _imgflip.SetUp(size);

            return new F_Combine(path, _imgflip.MakeCaption(text)).Meme(Quality, size).Output("-M");
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
            var size = GetImageSize_FFmpeg(path).GrowSize();
            _ifunny.SetUp(size);

            if  (IFunnyApp.UseGivenColor) _ifunny.SetCustomColors();
            else if (IFunnyApp.PickColor) _ifunny.SetSpecialColors(Image.Load<Rgba32>(Snapshot(path)));
            else                          _ifunny.SetDefaultColors();

            return new F_Combine(path, _ifunny.BakeText(text)).When(Quality, size, _ifunny.Cropping, _ifunny.Location, IFunnyApp.BlurImage).Output("-Top");
        }


        public static string DeepFryImage(string path, int _ = 0)
        {
            var extension = Sticker ? ".webp" : Path.GetExtension(path);
            return new F_Process(path).DeepFry(Qscale).Output("-Nuked", extension);
        }

        public static string DeepFryStick(string path, int _ = 0, string extension = null) => DeepFryImage(path);

        public static string DeepFryVideo(string path, int _ = 0)
        {
            var size = GetImageSize_FFmpeg(path).GrowSize().ValidMp4Size();
            return new F_Process(path).DeepFryVideo(size.Ok(), Quality).Output_WEBM_safe("-Nuked");
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

        public static string Slice(string path)
        {
            var extension = Path.GetExtension(path);
            var video = extension == ".mp4" || extension == ".webm";
            if (video)
            {
                var size = GetImageSize_FFmpeg(path);
                var fits = size.FitSize(720).ValidMp4Size();
                if (size != fits)
                {
                    path = Scale(path, new[] { fits.Width.ToString(), fits.Height.ToString() });
                }
            }
            return new F_Process(path).SliceRandom().Output("-slices", video ? ".mp4" : ".mp3");
        }

        public static string ChangeSpeed  (string path, double speed) => new F_Process(path).ChangeSpeed(speed).Output_WEBM_safe("-Speed");
        public static string RemoveBitrate(string path, int      crf) => new F_Process(path).Compress(crf).Output_WEBM_safe("-DAMN");

        public static string Sus(string path, CutSpan s) => new F_Cut(path, s).Sus().Output_WEBM_safe("-Sus");
        public static string Cut(string path, CutSpan s) => new F_Cut(path, s).Cut().Output_WEBM_safe("-Cut");

        public static string Reverse       (string path) => new F_Process(path).Reverse().Output_WEBM_safe("-Reverse");

        public static string RemoveAudio   (string path) => new F_Process(path).ToAnimation().Output("-silent");
        public static string Stickerize    (string path) => new F_Process(path).ToSticker(GetImageSize_FFmpeg(path).Normalize().Ok()).Output("-stick", ".webp");
        public static string Compress      (string path) => new F_Process(path).CompressImage(GetImageSize_FFmpeg(path).FitSize(2560).Ok()).Output("-small", ".jpg");
        public static string CompressGIF   (string path) => new F_Process(path).CompressAnimation().Output("-small");
        public static string CropVideoNote (string path) => new F_Process(path).CropVideoNote().Output("-crop");
        public static string Crop          (string path, string[] args) => new F_Process(path).CropVideo (args).Output("-crop");
        public static string Scale         (string path, string[] args) => new F_Process(path).ScaleVideo(args).Output("-scale");
        public static string ChangeVolume  (string path, string   arg ) => new F_Process(path).ChangeVolume(arg).Output("-vol");
        public static string EQ            (string path, double[] args) => new F_Process(path).EQ(args).Output("-EQ");
        public static string Edit          (string path, string options, string extension)
        {
            return new F_Process(path).Edit(options).Output("-Edit", "." + extension);
        }
        public static string ToVoice       (string path) => new F_Process(path).ToVoiceMessage().Output("-voice", ".ogg");
        public static string ToVideoNote   (string path)
        {
            var s = GetImageSize_FFmpeg(path);
            var d = ToEven(Math.Min(s.Width, s.Height));
            var x = (s.Width  - d) / 2;
            var y = (s.Height - d) / 2;

            return new F_Process(path).ToVideoNote(new Rectangle(x, y, d, d)).Output("-vnote");
        }

        private static string Snapshot(string path)
        {
            var temp = ImageSaver.GetTempPicName();
            FFMpeg.Snapshot(path, temp);
            return temp;
        }
    }
    
    public record DgText(string A, string B);
    
    public record CutSpan(TimeSpan Start, TimeSpan Length);
}