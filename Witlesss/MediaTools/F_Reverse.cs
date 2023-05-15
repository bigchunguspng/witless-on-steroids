using FFMpegCore;

namespace Witlesss.MediaTools
{
    // -i input [-s WxH] [-vn] [-vf reverse] [-af areverse] output
    public class F_Reverse : F_SingleInput_Base
    {
        public F_Reverse(string input) : base(input) { }

        public string Reverse() => Cook(SetOutName_WEBM_safe(_input, "-RVR"), Args);

        private void Args(FFMpegArgumentOptions o)
        {
            var i = MediaInfoWithFixing(o);
            if (i.video) o.WithVideoFilters(v => v.ReverseVideo());
            if (i.audio) o.WithAudioFilters(a => a.ReverseAudio());
        }
    }
}