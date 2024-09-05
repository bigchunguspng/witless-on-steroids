using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using Witlesss.Backrooms;
using static Witlesss.MediaTools.FFMpegXD;
using FFMpAO = FFMpegCore.FFMpegArgumentOptions;

namespace Witlesss.MediaTools
{
    public partial class F_Process
    {
        #region MEDIA CONVERSION

        // -s WxH -an -vcodec libx264 -crf 30
        public F_Process RemoveAudio() => ApplyEffects(o =>
        {
            var v = GetVideoStream(Input)!;
            var size = new Size(v.Width, v.Height).Ok().FitSize().ValidMp4Size().Ok();
            o.Resize(size).DisableChannel(Channel.Audio).WithCompression(30).FixPlayback();
        });

        // -c:a libopus -b:a 48k -vn
        public F_Process ToVoice() => ApplyEffects(o =>
        {
            o.WithAudioCodec("libopus").WithAudioBitrate(48).DisableChannel(Channel.Video);
        });

        // -filter:v "crop=W:H:X:Y" -s 384x384
        public F_Process ToVideoNote(Rectangle crop) => ApplyEffects(o =>
        {
            o.WithVideoFilters(v => v.Crop(crop)).Resize(VideoNoteSize).FixPlayback();
        });

        public F_Process ToSticker(Size size) => ApplyEffects(o => o.Resize(size));

        public F_Process Edit(string options) => ApplyEffects(o => o.WithCustomArgument(options));

        #endregion


        #region COMPRESSION

        // [-s WxH] [-vn] [-vcodec libx264 -crf 45] [-b:a 1k] [-f mp3]
        public F_Process Compress(int factor) => ApplyEffects(o => AddCompression(o, factor));

        private void AddCompression(FFMpAO o, int factor)
        {
            // factor: 0 lossless - 51 lowest quality
            var i = MediaInfoWithFixing(o);
            if (i.HasVideo) o.WithCompression(factor).FixPlayback();
            if (i.HasAudio) o.WithAudioBitrate(154 - 3 * factor);
            if (i.HasAudio && !i.HasVideo) o.ForceFormat("mp3");
        }

        public F_Process CompressImage (Size s) => ApplyEffects(o => o.Resize(s).WithQscale(5)); // -qscale:v 5
        public F_Process CompressAnimation   () => ApplyEffects(o =>
        {
            var v = GetVideoStream(Input)!;
            o.FixWebmSize(v).DisableChannel(Channel.Audio).WithCompression(30).FixPlayback();
        });

        #endregion


        #region SPEED

        // [-s WxH] [-vn] [-vf reverse] [-af areverse]
        public F_Process Reverse() => ApplyEffects(o =>
        {
            var i = MediaInfoWithFixing(o);
            if (i.HasVideo) o.WithVideoFilters(v => v.ReverseVideo()).FixPlayback();
            if (i.HasAudio) o.WithAudioFilters(a => a.ReverseAudio());
        });
        
        // [-vf "setpts=0.5*PTS,fps=60"][-s WxH] [-af "atempo=2.0"][-vn]
        public F_Process ChangeSpeed(double speed) => ApplyEffects(o =>
        {
            var i = MediaInfoWithFixing(o);
            if (i.HasVideo) o.WithVideoFilters(v => v.ChangeVideoSpeed(speed).SetFPS(GetFPS())).FixPlayback();
            if (i.HasAudio) o.WithAudioFilters(a => a.ChangeAudioSpeed(speed));

            double GetFPS() => Math.Min(i.Video.AvgFrameRate * speed, 90D);
        });

        #endregion


        #region AUDIO

        public F_Process ChangeVolume(string arg)
        {
            return ApplyEffects(o => o.WithAudioFilters(v => v.Volume(arg)).WithVideoCodec("copy"));
        }
        
        public F_Process EQ(double[] args)
        {
            return ApplyEffects(o => o.WithAudioFilters(v => v.Equalize(args)).WithVideoCodec("copy"));
        }

        #endregion


        #region CROP / SCALE

        // -filter:v "crop=272:272:56:56"
        public F_Process CropVideoNote
            ()           => ApplyEffects(o => o.FixPlayback().WithVideoFilters(v => v.Crop(VideoNoteCrop)));
        public F_Process CropVideo
            (string[] c) => ApplyEffects(o => o.FixPlayback().WithVideoFilters(v => v.Crop(c)));
        public F_Process CropJpeg
            (string[] c) => ApplyEffects(o => o              .WithVideoFilters(v => v.Crop(c)));

        // -vf "scale=W:H,setsar=1"
        public F_Process Scale
            (string[] s) => ApplyEffects(o => o.FixPlayback().WithVideoFilters(v => v.Scale(s).SampleRatio(1)));
        public F_Process ScaleJpeg
            (string[] s) => ApplyEffects(o => o              .WithVideoFilters(v => v.Scale(s).SampleRatio(1)));

        #endregion


        #region YT MUSIC

        public F_Process ExportThumbnail (bool square) => ApplyEffects(o => ExportThumbnail(o, square));
        public F_Process ResizeThumbnail (bool square) => ApplyEffects(o => ResizeThumbnail(o, square));
        public F_Process CompressJpeg    (int  factor) => ApplyEffects(o => o.WithQscale(factor));
        public F_Process MakeThumb       (int  factor)
        {
            return ApplyEffects(o => o.WithQscale(factor).Resize(GetPictureSize(Input).FitSize(320).Ok()));
        }

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