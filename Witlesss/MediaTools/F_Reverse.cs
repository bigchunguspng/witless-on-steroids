using FFMpegCore;

namespace Witlesss.MediaTools
{
    // -i input [-vf reverse] [-af areverse] output
    public class F_Reverse : F_SingleInput_Base
    {
        public F_Reverse(string input) : base(input) { }

        public string Reverse() => Cook(SetOutName_WEBM_safe(_input, "-RVR"), Args);

        private void Args(FFMpegArgumentOptions o)
        {
            var i = MediaInfo();
            if (i.video) o = o.WithVideoFilters(v => v.ReverseVideo()).FixWebmSize(i.v);
            if (i.audio) o = o.WithAudioFilters(a => a.ReverseAudio()).FixSongArt(i.info);
        }
    }
}