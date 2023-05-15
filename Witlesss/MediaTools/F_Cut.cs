using System;
using FFMpegCore;
using static Witlesss.MediaTools.ComplexFilterArgs;

namespace Witlesss.MediaTools
{
    public class F_Cut : F_SingleInput_Base
    {
        private readonly CutSpan _span;

        public F_Cut(string input, CutSpan span) : base(input) => _span = span;

        public string Sus() => Cook(SetOutName_WEBM_safe(_input, "-Sus"), SusArgs);
        public string Cut() => Cook(SetOutName_WEBM_safe(_input, "-Cut"), CutArgs);

        // -i input [-s WxH] [-vn] -filter_complex
        // "
        // [0:v] trim=start=A:duration=B, setpts=PTS-STARTPTS, split=2[v0][v1];[v1] reverse, setpts=PTS-STARTPTS[vr];[v0][vr]concat=n=2:v=1;
        // [0:a]atrim=start=A:duration=B,asetpts=PTS-STARTPTS,asplit=2[a0][a1];[a1]areverse,asetpts=PTS-STARTPTS[ar];[a0][ar]concat=n=2:v=0:a=1
        // "
        // output
        private void SusArgs(FFMpegArgumentOptions o)
        {
            var i = MediaInfoWithFixing(o);

            var nodes = CoFi.Null;
            var span = Span(i.info);
            if (VF(i.video)) nodes = nodes.Trim("0:v", span, "vc").Split("v0", "v1").Reverse("v1", "vr").Concat("v0", "vr");
            if (AF(i.audio)) nodes = nodes.Trim("0:a", span, "ac").Split("a0", "a1").Reverse("a1", "ar").Concat("a0", "ar");
            o.WithComplexFilter(nodes);
        }

        // -i input [-s WxH] [-vn] -ss 00:00:05 [-t 00:00:15] output
        private void CutArgs(FFMpegArgumentOptions o)
        {
            AddFixes(o, MediaInfo());

            o.Seek(_span.Start);
            if (_span.Length != TimeSpan.Zero) o.WithDuration(_span.Length);
        }

        private CutSpan Span(IMediaAnalysis i)
        {
            if     (_span.Length < TimeSpan.Zero)            return _span with { Length = i.Duration / 2D };
            return (_span.Length + _span.Start).Ticks > 0 ? _span : _span with { Length = i.Duration };
        }
    }
}