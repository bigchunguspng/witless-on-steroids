using System;
using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using static Witlesss.Memes;
using FFMpAO = FFMpegCore.FFMpegArgumentOptions;

namespace Witlesss.MediaTools
{
    public class F_Process : F_Action_SingleInput
    {
        public F_Process(string input) : base(input) { }


        #region MEDIA CONVERSION

        // -s WxH -an -vcodec libx264 -crf 30
        public F_Action ToAnimation() => ApplyEffects(o =>
        {
            var v = GetVideoStream(_input);
            var size = FitSize(new Size(v.Width, v.Height));
            o.Resize(size).DisableChannel(Channel.Audio).WithCompression(30);
        });

        // -c:a libopus -b:a 48k -vn
        public F_Action ToVoiceMessage() => ApplyEffects(o =>
        {
            o.WithAudioCodec("libopus").WithAudioBitrate(48).DisableChannel(Channel.Video);
        });
        
        // -filter:v "crop=W:H:X:Y" -s 384x384
        public F_Action ToVideoNote (Rectangle crop) => ApplyEffects(o =>
        {
            o.WithVideoFilters(v => v.Crop(crop)).Resize(VideoNoteSize);
        });
        public F_Action ToSticker        (Size size) => ApplyEffects(o => o.Resize(size));

        #endregion


        #region COMPRESSION

        // [-s WxH] [-vn] [-vcodec libx264 -crf 45] [-b:a 1k] [-f mp3]
        public F_Action Compress(int factor) => ApplyEffects(o =>
        {
            // factor: 0 lossless - 51 lowest quality
            var i = MediaInfoWithFixing(o);
            if (i.video) o.WithCompression(factor);
            if (i.audio) o.WithAudioBitrate(154 - 3 * factor);
            if (i.audio && !i.video) o.ForceFormat("mp3");
        });

        public F_Action CompressImage (Size s) => ApplyEffects(o => o.Resize(s).WithQscale(5)); // -qscale:v 5
        public F_Action CompressAnimation   () => ApplyEffects(o =>
        {
            var v = GetVideoStream(_input);
            o.FixWebmSize(v).DisableChannel(Channel.Audio).WithCompression(30);
        });

        #endregion


        #region SPEED

        // [-s WxH] [-vn] [-vf reverse] [-af areverse]
        public F_Action Reverse() => ApplyEffects(o =>
        {
            var i = MediaInfoWithFixing(o);
            if (i.video) o.WithVideoFilters(v => v.ReverseVideo());
            if (i.audio) o.WithAudioFilters(a => a.ReverseAudio());
        });
        
        // [-vf "setpts=0.5*PTS,fps=60"][-s WxH] [-af "atempo=2.0"][-vn]
        public F_Action ChangeSpeed(double speed) => ApplyEffects(o =>
        {
            var i = MediaInfoWithFixing(o);
            if (i.video) o.WithVideoFilters(v => v.ChangeVideoSpeed(speed).SetFPS(GetFPS()));
            if (i.audio) o.WithAudioFilters(a => a.ChangeAudioSpeed(speed));

            double GetFPS() => Math.Min(i.v.AvgFrameRate * speed, 90D);
        });

        #endregion


        #region AUDIO

        public F_Action ChangeVolume(string arg)
        {
            return ApplyEffects(o => o.WithAudioFilters(v => v.Volume(arg)).WithVideoCodec("copy"));
        }
        
        public F_Action EQ(double[] args)
        {
            return ApplyEffects(o => o.WithAudioFilters(v => v.Equalize(args)).WithVideoCodec("copy"));
        }

        #endregion


        #region CROP / SCALE

        // -filter:v "crop=272:272:56:56"
        public F_Action CropVideoNote       () => ApplyEffects(o => o.WithVideoFilters(v => v.Crop(VideoNoteCrop)));
        public F_Action CropVideo (string[] c) => ApplyEffects(o => o.WithVideoFilters(v => v.Crop(c)));

        // -vf "scale=W:H,setsar=1"
        public F_Action ScaleVideo(string[] s) => ApplyEffects(o => o.WithVideoFilters(v => v.Scale(s).SampleRatio(1)));

        #endregion


        #region YT MUSIC

        public F_Action ExportThumbnail (bool square) => ApplyEffects(o => ExportThumbnail(o, square));
        public F_Action ResizeThumbnail (bool square) => ApplyEffects(o => ResizeThumbnail(o, square));
        public F_Action CompressJpeg    (int  factor) => ApplyEffects(o => o.WithQscale(factor));

        // -ss 1 -frames:v 1 -vf
        private static void ExportThumbnail(FFMpAO o, bool square)
        {
            ResizeThumbnail(o.Seek(TimeSpan.FromSeconds(1)).WithFrameOutputCount(1), square);
        }

        // -vf [ "crop=w='min(iw,ih)':h='min(iw,ih)',scale=640:640" | "scale=640:-1" ] -qscale:v 2
        private static void ResizeThumbnail(FFMpAO o, bool square)
        {
            o.WithVideoFilters(v => ApplyResizing(v, square)).WithQscale(2);
        }

        private static void ApplyResizing(VideoFilterOptions v, bool square)
        {
            if (square) v.MakeSquare(640);
            else        v.Scale(640,  -1);
        }

        #endregion
    }
}