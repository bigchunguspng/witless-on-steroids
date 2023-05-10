using System;
using System.Drawing;
using System.IO;
using FFMpegCore.Enums;
using static Witlesss.Memes;
using FFMpAO = FFMpegCore.FFMpegArgumentOptions;

namespace Witlesss.MediaTools // ReSharper disable RedundantAssignment
{
    public class F_Resize : F_SingleInput_Base
    {
        public F_Resize(string input) : base(input) { }

        
        public string Transcode (string extension) => Cook(SetOutName(_input, "-W", extension), null);

        public string ToAnimation               () => Cook(SetOutName(_input, "-silent", ".mp4"), ToAnimationArgs);
        public string ToVideoNote (Rectangle crop) => Cook(SetOutName(_input, "-vnote",  ".mp4"), o => ToVideoNoteArgs(o, crop));
        public string ToSticker           (Size s) => Cook(SetOutName(_input, "-stick", ".webp"), o => o.Resize(s));

        public string CompressImage (Size s) => Cook(SetOutName(_input, "-small", ".jpg"), o => o.Resize(s).WithQscale(5)); // -qscale:v 5
        public string CompressAnimation   () => Cook(SetOutName(_input, "-small", ".mp4"), CompressAnimationArgs);

        public string CropVideoNote       () => Cook(SetOutName(_input, "-crop",  ".mp4"), CropVideoNoteArgs);

        public string ExportThumbnail () => Cook($"{Path.GetDirectoryName(_input)}/art.png", ExportThumbnailArgs);

        
        // -s WxH -an -vcodec libx264 -crf 30
        private void ToAnimationArgs(FFMpAO o)
        {
            var v = GetVideoStream(_input);
            var size = FitSize(new Size(v.Width, v.Height));
            o = o.Resize(size).DisableChannel(Channel.Audio).WithVideoCodec("libx264").WithConstantRateFactor(30);
        }
        private void CompressAnimationArgs(FFMpAO o)
        {
            var v = GetVideoStream(_input);
            o = o.FixWebmSize(v).DisableChannel(Channel.Audio).WithVideoCodec("libx264").WithConstantRateFactor(30);
        }

        // -filter:v "crop=W:H:X:Y" -s 384x384 output.mp4
        private static void ToVideoNoteArgs(FFMpAO o, Rectangle crop) => o = o.WithVideoFilters(v => v.Crop(crop)).Resize(VideoNoteSize);

        // -filter:v "crop=272:272:56:56" output.mp4
        private static void CropVideoNoteArgs(FFMpAO o) => o = o.WithVideoFilters(v => v.Crop(VideoNoteCrop));

        // -ss 1 -frames:v 1 -s 640x640 art.png
        private static void ExportThumbnailArgs(FFMpAO o) => o.Seek(TimeSpan.FromSeconds(1)).WithFrameOutputCount(1).WithVideoFilters(v => v.Scale(-1, 640));
    }
}