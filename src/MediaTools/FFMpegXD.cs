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
            var ffmpeg = path.UseFFMpeg();
            var name = ffmpeg.GetOutputName("-W", extension);
            try
            {
                return await ffmpeg.OutAs(name);
            }
            catch // av_interleaved_write_frame(): I/O error
            {
                return name; // file is already exist - just use it
            }
        }

        public static async Task<string> Slice(string path, MediaType type, string extension)
        {
            var video = type != MediaType.Audio;
            if (video)
            {
                var size = GetPictureSize(path);
                var fits = size.FitSize(720).ValidMp4Size();
                if (size != fits)
                {
                    string[] args = [fits.Width.ToString(), fits.Height.ToString()];
                    path = await path.UseFFMpeg().Scale(args).Out("-scale");
                }
            }
            
            return await path.UseFFMpeg().SliceRandom().Out("-slices", extension);
        }

        public static Task<string> Compress
            (string path)
            => path.UseFFMpeg().CompressImage(GetPictureSize(path).FitSize(2560).Ok()).Out("-small", ".jpg");

        public static Task<string> CompressGIF
            (string path) => path.UseFFMpeg().CompressAnimation().Out("-small");

        public static Task<string> CropVideoNote
            (string path) => path.UseFFMpeg().CropVideoNote().Out("-crop");

        public static Task<string> ToVideoNote(string path)
        {
            var s = GetPictureSize(path);
            var d = ToEven(Math.Min(s.Width, s.Height));
            var x = (s.Width  - d) / 2;
            var y = (s.Height - d) / 2;

            return path.UseFFMpeg().ToVideoNote(new Rectangle(x, y, d, d)).Out("-vnote");
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
}