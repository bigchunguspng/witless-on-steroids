using System;
using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using static Witlesss.Memes;
using FFMpAO = FFMpegCore.FFMpegArgumentOptions;

namespace Witlesss.MediaTools
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

        public string ExportThumbnail (string path, bool square) => Cook(path, o => ExportThumbnailArgs(o, square));
        public string ResizeThumbnail (string path, bool square) => Cook(path, o => ResizeThumbnailArgs(o, square));
        public string CompressJpeg    (string path, int  factor) => Cook(path, o => o.WithQscale(factor));

        
        // -s WxH -an -vcodec libx264 -crf 30
        private void ToAnimationArgs(FFMpAO o)
        {
            var v = GetVideoStream(_input);
            var size = FitSize(new Size(v.Width, v.Height));
            o.Resize(size).DisableChannel(Channel.Audio).WithCompression(30);
        }
        private void CompressAnimationArgs(FFMpAO o)
        {
            var v = GetVideoStream(_input);
            o.FixWebmSize(v).DisableChannel(Channel.Audio).WithCompression(30);
        }

        // -filter:v "crop=W:H:X:Y" -s 384x384
        private static void ToVideoNoteArgs(FFMpAO o, Rectangle crop) => o.WithVideoFilters(v => v.Crop(crop)).Resize(VideoNoteSize);

        // -filter:v "crop=272:272:56:56"
        private static void CropVideoNoteArgs(FFMpAO o) => o.WithVideoFilters(v => v.Crop(VideoNoteCrop));

        // -ss 1 -frames:v 1 -vf
        private static void ExportThumbnailArgs(FFMpAO o, bool square)
        {
            ResizeThumbnailArgs(o.Seek(TimeSpan.FromSeconds(1)).WithFrameOutputCount(1), square);
        }

        private static void ResizeThumbnailArgs(FFMpAO o, bool square)
        {
            o.WithVideoFilters(v => SelectResizing(v, square)).WithQscale(2);
        }

        // -vf "crop=w='min(iw,ih)':h='min(iw,ih)',scale=640:640" / "scale=640:-1"
        private static void SelectResizing(VideoFilterOptions v, bool square)
        {
            if (square) v.MakeSquare(640);
            else        v.Scale(640,  -1);
        }
    }
}