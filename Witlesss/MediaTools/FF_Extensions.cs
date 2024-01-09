using System.Drawing;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using VFO = FFMpegCore.Arguments.VideoFilterOptions;
using AFO = FFMpegCore.Arguments.AudioFilterOptions;
using FFO = FFMpegCore.FFMpegArgumentOptions;

namespace Witlesss.MediaTools
{
    public static class FF_Extensions
    {
        public static FFO WithComplexFilter (this FFO o,  Filter node) => o.WithArgument(new ComplexFilterArgument(node));
        public static FFO WithMapping       (this FFO o, string label) => o.WithArgument(new MapArgument         (label));

        public static FFO WithQscale       (this FFO o, int qscale) => o.WithArgument(new QscaleArgument(qscale));
        public static FFO WithCompression  (this FFO o, int factor) => o.WithVideoCodec("libx264").WithConstantRateFactor(factor);

        public static FFO FixWebmSize (this FFO o, VideoStream v) => SizeIsInvalid(v.Width, v.Height) ? o.Resize(ValidSize(v.Width, v.Height)) : o;
        public static FFO FixSongArt  (this FFO o, IMediaAnalysis info) => info.ErrorData.Count > 0   ? o.DisableChannel(Channel.Video)        : o;

        public static VFO ChangeVideoSpeed (this VFO o, double speed) => o.With(new SpeedArgument (speed));
        public static AFO ChangeAudioSpeed (this AFO o, double speed) => o.With(new AtempoArgument(speed));
        public static VFO SetFPS           (this VFO o, double fps  ) => o.With(new FpsArgument   (fps  ));
        public static VFO SampleRatio      (this VFO o, double ratio) => o.With(new SampleRatioArgument(ratio));
        
        public static VFO Scale      (this VFO o, string[]   scaling) => o.With(new ScaleArgumentXD(scaling));
        public static VFO Crop       (this VFO o, Rectangle cropping) => o.With(new CropArgument  (cropping));
        public static VFO Crop       (this VFO o, string[]  cropping) => o.With(new CropArgumentXD(cropping));
        public static VFO CropSquare (this VFO o)                     => o.With(new CropArgumentXD(_squareCropping));
        
        public static AFO Volume     (this AFO o, string volume) => o.With(new VolumeArgument(volume));
        public static AFO Equalize     (this AFO o, double[] args) => o.With(new EqualizeArgument(args));

        public static VFO MakeSquare (this VFO o, int size) => o.CropSquare().Scale(size, size);

        public static VFO ReverseVideo (this VFO o) => o.With(new  ReverseArgument());
        public static AFO ReverseAudio (this AFO o) => o.With(new AreverseArgument());


        private static VFO With(this VFO o, IVideoFilterArgument argument)
        {
            o.Arguments.Add(argument);
            return o;
        }
        private static AFO With(this AFO o, IAudioFilterArgument argument)
        {
            o.Arguments.Add(argument);
            return o;
        }

        private static bool SizeIsInvalid(int w, int h) => (w | h) % 2 > 0;
        public  static Size ValidSize    (int w, int h) => new(ToEven(w), ToEven(h));

        public  static int ToEven(int x) => x - x % 2;

        private static readonly string[] _squareCropping = new[] { "'min(iw,ih)'", "'min(iw,ih)'" };
    }
}