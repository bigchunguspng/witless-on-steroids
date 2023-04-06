using FFMpegCore;

namespace Witlesss.MediaTools // ReSharper disable RedundantAssignment
{
    // -i input [-s WxH] [-vn] [-vf reverse] [-af areverse] output
    public class F_Reverse : F_SingleInput_Base
    {
        public F_Reverse(string input) : base(input) { }

        public string Reverse() => Cook(SetOutName_WEBM_safe(_input, "-RVR"), Args);

        private void Args(FFMpegArgumentOptions o)
        {
            var i = MediaInfoWithFixing(ref o);
            if (i.video) o = o.WithVideoFilters(v => v.ReverseVideo());
            if (i.audio) o = o.WithAudioFilters(a => a.ReverseAudio());
        }
    }
}