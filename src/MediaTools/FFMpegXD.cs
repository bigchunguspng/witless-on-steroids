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


        public static async Task<string> Convert(this MessageOrigin origin, string path, string extension)
        {
            var ffmpeg = path.UseFFMpeg(origin);
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

        public static async Task<string> ReduceSize(MessageOrigin origin, string path)
        {
            var size = GetPictureSize(path);
            var fits = size.FitSize(720).ValidMp4Size();
            if (size != fits)
            {
                string[] args = [fits.Width.ToString(), fits.Height.ToString()];
                return await path.UseFFMpeg(origin).Scale(args).Out("-scale");
            }

            return path;
        }

        public static Task<string> Compress
            (this F_Process process)
            => process.CompressImage(GetPictureSize(process.Input).FitSize(2560).Ok()).Out("-small", ".jpg");

        public static Task<string> CompressGIF
            (this F_Process process)
            => process.CompressAnimation().Out("-small");

        public static Task<string> CropVideoNoteXD
            (this F_Process process)
            => process.CropVideoNote().Out("-crop");

        public static Task<string> ToVideoNote(this F_Process process)
        {
            var s = GetPictureSize(process.Input);
            var d = ToEven(Math.Min(s.Width, s.Height));
            var x = (s.Width  - d) / 2;
            var y = (s.Height - d) / 2;

            return process.ToVideoNote(new Rectangle(x, y, d, d)).Out("-vnote");
        }

        public static string Snapshot(string path)
        {
            var temp = ImageSaver.GetTempPicName();
            FFMpeg.Snapshot(path, temp);
            return temp;
        }

        public static SixLabors.ImageSharp.Size GetPictureSize(string path)
        {
            var v = F_Process.GetVideoStream(path)!;
            return new SixLabors.ImageSharp.Size(v.Width, v.Height);
        }
    }
}