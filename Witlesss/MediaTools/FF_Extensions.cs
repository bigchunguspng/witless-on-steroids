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
        public static FFO WithComplexFilter (this FFO o, CoFi node) => o.WithArgument(new ComplexFilterArgument(node));
        public static FFO WithMapping    (this FFO o, string label) => o.WithArgument(new MapArgument         (label));
        public static FFO WithQscale       (this FFO o, int qscale) => o.WithArgument(new QscaleArgument     (qscale));

        public static FFO FixWebmSize (this FFO o, VideoStream v) => SizeIsInvalid(v.Width, v.Height) ? o.Resize(ValidSize(v.Width, v.Height)) : o;
        public static FFO FixSongArt  (this FFO o, IMediaAnalysis info) => info.ErrorData.Count > 0   ? o.DisableChannel(Channel.Video)        : o;

        public static VFO ChangeVideoSpeed (this VFO o, double speed) => o.WithArgument(new SpeedArgument (speed));
        public static VFO SetFPS           (this VFO o, double fps  ) => o.WithArgument(new FpsArgument   (fps  ));
        public static AFO ChangeAudioSpeed (this AFO o, double speed) => o.WithArgument(new AtempoArgument(speed));
        
        public static VFO Crop       (this VFO o, Rectangle cropping) => o.WithArgument(new CropArgument(cropping));
        public static VFO CropSquare (this VFO o)                     => o.WithArgument(new CropSquareArgument  ());

        public static VFO MakeSquare (this VFO o, int size) => o.CropSquare().Scale(size, size);

        public static VFO ReverseVideo (this VFO o) => o.WithArgument(new  ReverseArgument());
        public static AFO ReverseAudio (this AFO o) => o.WithArgument(new AreverseArgument());


        private static VFO WithArgument(this VFO o, IVideoFilterArgument argument)
        {
            o.Arguments.Add(argument);
            return o;
        }
        private static AFO WithArgument(this AFO o, IAudioFilterArgument argument)
        {
            o.Arguments.Add(argument);
            return o;
        }

        private static bool SizeIsInvalid(int w, int h) => (w | h) % 2 > 0;
        public  static Size ValidSize    (int w, int h) => new(ToEven(w), ToEven(h));

        public  static int ToEven(int x) => x - x % 2;
    }
}