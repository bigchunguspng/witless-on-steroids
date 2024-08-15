using FFMpegCore;
using static Witlesss.MediaTools.FF_Extensions;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Witlesss.MediaTools
{
    public static class FFMpegXD
    {
        public static readonly Size      VideoNoteSize = new(384, 384);
        public static readonly Rectangle VideoNoteCrop = new(56, 56, 272, 272);


        public static async Task<string> Convert(string path, string extension)
        {
            var ffmpeg = new F_Process(path);
            var name = ffmpeg.GetOutputName("-W", extension);
            try
            {
                return await ffmpeg.OutputAs(name);
            }
            catch // av_interleaved_write_frame(): I/O error
            {
                return name; // file is already exist - just use it
            }
        }

        public static async Task<string> Slice(string path)
        {
            var extension = Path.GetExtension(path);
            var video = extension is ".mp4" or ".webm";
            if (video)
            {
                var size = GetPictureSize(path);
                var fits = size.FitSize(720).ValidMp4Size();
                if (size != fits)
                {
                    path = await Scale(path, [fits.Width.ToString(), fits.Height.ToString()]);
                }
            }
            
            return await new F_Process(path).SliceRandom().Output("-slices", video ? ".mp4" : ".mp3");
        }

        public static Task<string> ChangeSpeed  (string path, double speed) => new F_Process(path).ChangeSpeed(speed).Output_WEBM_safe("-Speed");
        public static Task<string> RemoveBitrate(string path, int      crf) => new F_Process(path).Compress(crf).Output_WEBM_safe("-DAMN");

        public static Task<string> Sus(string path, CutSpan s) => new F_Cut(path, s).Sus().Output_WEBM_safe("-Sus");
        public static Task<string> Cut(string path, CutSpan s) => new F_Cut(path, s).Cut().Output_WEBM_safe("-Cut");

        public static Task<string> Reverse       (string path) => new F_Process(path).Reverse().Output_WEBM_safe("-Reverse");

        public static Task<string> RemoveAudio   (string path) => new F_Process(path).ToAnimation().Output("-silent");
        public static Task<string> Stickerize    (string path) => new F_Process(path).ToSticker(GetPictureSize(path).Normalize().Ok()).Output("-stick", ".webp");
        public static Task<string> Compress      (string path) => new F_Process(path).CompressImage(GetPictureSize(path).FitSize(2560).Ok()).Output("-small", ".jpg");
        public static Task<string> CompressGIF   (string path) => new F_Process(path).CompressAnimation().Output("-small");
        public static Task<string> CropVideoNote (string path) => new F_Process(path).CropVideoNote().Output("-crop");
        public static Task<string> Crop          (string path, string[] args) => new F_Process(path).CropVideo (args).Output("-crop");
        public static Task<string> Scale         (string path, string[] args) => new F_Process(path).ScaleVideo(args).Output("-scale");
        public static Task<string> ChangeVolume  (string path, string   arg ) => new F_Process(path).ChangeVolume(arg).Output("-vol");
        public static Task<string> EQ            (string path, double[] args) => new F_Process(path).EQ(args).Output("-EQ");
        public static Task<string> Edit          (string path, string options, string extension)
        {
            return new F_Process(path).Edit(options).Output("-Edit", "." + extension);
        }
        public static Task<string> ToVoice       (string path) => new F_Process(path).ToVoiceMessage().Output("-voice", ".ogg");
        public static Task<string> ToVideoNote   (string path)
        {
            var s = GetPictureSize(path);
            var d = ToEven(Math.Min(s.Width, s.Height));
            var x = (s.Width  - d) / 2;
            var y = (s.Height - d) / 2;

            return new F_Process(path).ToVideoNote(new Rectangle(x, y, d, d)).Output("-vnote");
        }

        public static string Snapshot(string path)
        {
            var temp = ImageSaver.GetTempPicName();
            FFMpeg.Snapshot(path, temp);
            return temp;
        }

        public static SixLabors.ImageSharp.Size GetPictureSize(string path)
        {
            var v = F_Action.GetVideoStream(path)!;
            return new SixLabors.ImageSharp.Size(v.Width, v.Height);
        }
    }

    public record TextPair(string A, string B);

    public record CutSpan(TimeSpan Start, TimeSpan Length);
}