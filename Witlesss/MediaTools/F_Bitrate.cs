using FFMpegCore;

namespace Witlesss.MediaTools // ReSharper disable RedundantAssignment
{
    // -i input [-s WxH] [-vn] [-vcodec libx264 -crf 45] [-b:a 1k] [-f mp3] output
    public class F_Bitrate : F_SingleInput_Base
    {
        private readonly int _factor; // 0 lossless - 51 lowest quality

        public F_Bitrate(string input, int factor) : base(input) => _factor = factor;

        public string Compress() => Cook(SetOutName_WEBM_safe(_input, "-DAMN"), DamnArgs);

        private void DamnArgs(FFMpegArgumentOptions o)
        {
            var i = MediaInfoWithFixing(ref o);
            if (i.video) o = o.WithVideoCodec("libx264").WithConstantRateFactor(_factor);
            if (i.audio) o = o.WithAudioBitrate(154 - 3 * _factor);
            if (i.audio && !i.video) o = o.ForceFormat("mp3");
        }
    }
}