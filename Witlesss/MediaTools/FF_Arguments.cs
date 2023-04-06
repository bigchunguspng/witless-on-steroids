using System.Drawing;
using FFMpegCore.Arguments;

namespace Witlesss.MediaTools
{
    #region IVideoFilterArguments

    public class SpeedArgument : IVideoFilterArgument
    {
        private readonly double _speed;

        public SpeedArgument(double speed) => _speed = speed;

        public string Key   => "setpts";
        public string Value => $"{FormatDouble(1 / _speed)}*PTS";
    }
    public class FpsArgument : IVideoFilterArgument
    {
        private readonly double _fps;

        public FpsArgument(double fps) => _fps = fps;

        public string Key   => "fps";
        public string Value => FormatDouble(_fps);
    }
    public class CropArgument : IVideoFilterArgument
    {
        private readonly Rectangle _crop;

        public CropArgument(Rectangle rectangle) => _crop = rectangle;

        public string Key   => "crop";
        public string Value => $"{_crop.Width}:{_crop.Height}:{_crop.X}:{_crop.Y}";
    }
    public class ReverseArgument : IVideoFilterArgument
    {
        public string Key   => null;
        public string Value => "reverse";
    }

    #endregion

    #region IAudioFilterArguments

    public class AtempoArgument : IAudioFilterArgument
    {
        private readonly double _atempo;

        public AtempoArgument(double atempo) => _atempo = atempo;

        public string Key   => "atempo";
        public string Value => FormatDouble(_atempo);
    }
    public class AreverseArgument : IAudioFilterArgument
    {
        public string Key   => null;
        public string Value => "areverse";
    }

    #endregion

    #region IArguments

    public class MapArgument : IArgument
    {
        private readonly string _label;
        
        public MapArgument(string label) => _label = label;

        public string Text => $"-map \"[{_label}]\"";
    }
    public class QscaleArgument : IArgument
    {
        private readonly int _qscale; // 1 - 31 (best - worst quality)
        
        public QscaleArgument(int qscale) => _qscale = qscale;

        public string Text => $"-qscale:v \"[{_qscale}]\"";
    }
    public class ComplexFilterArgument : IArgument
    {
        private readonly string _xd;
        
        public ComplexFilterArgument(CoFi node) => _xd = node.Text;

        public string Text => $"-filter_complex \"{_xd}\"";
    }

    #endregion
}