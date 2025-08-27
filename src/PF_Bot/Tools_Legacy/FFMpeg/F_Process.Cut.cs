using System.Globalization;
using System.Text;
using FFMpegCore;

namespace PF_Bot.Tools_Legacy.FFMpeg
{
    public partial class F_Process
    {
        // -i input [-s WxH] [-vn] -filter_complex
        // "
        // [0:v] trim=start=A:duration=B, setpts=PTS-STARTPTS, split=2[v0][v1];[v1] reverse, setpts=PTS-STARTPTS[vr];[v0][vr]concat=n=2:v=1;
        // [0:a]atrim=start=A:duration=B,asetpts=PTS-STARTPTS,asplit=2[a0][a1];[a1]areverse,asetpts=PTS-STARTPTS[ar];[a0][ar]concat=n=2:v=0:a=1
        // "
        // output
        public F_Process Sus(CutSpan span) => ApplyEffects(o =>
        {
            var i = MediaInfoWithFixing(o);
            span = Span(i.Info, span);

            var sb = new StringBuilder("-filter_complex \"");
            if (i.HasVideo) sb.Append(GetSusFilter(span, "v", "", "v=1"));
            if (i is { HasAudio: true, HasVideo: true }) sb.Append(';');
            if (i.HasAudio) sb.Append(GetSusFilter(span, "a", "a", "v=0:a=1"));
            o.WithCustomArgument(sb.Append('\"').ToString());

            if (i.HasVideo) o.FixPlayback();
        });

        private string GetSusFilter(CutSpan span, string av, string a0, string concat)
        {
            return $"[0:{av}]{a0}trim=start='{TsFormat(span.Start)}':duration='{TsFormat(span.Length)}'"
                 + $",{a0}setpts=PTS-STARTPTS,{a0}split=2[{av}0][{av}1];[{av}1]{a0}reverse"
                 + $",{a0}setpts=PTS-STARTPTS[{av}r];[{av}0][{av}r]concat=n=2:{concat}";

            string TsFormat(TimeSpan s) => s.ToString("g", CultureInfo.InvariantCulture);
        }

        // -i input [-s WxH] [-vn] -ss 00:00:05 [-t 00:00:15] output
        public F_Process Cut(CutSpan span) => ApplyEffects(o =>
        {
            var i = GetMediaInfo();
            if (i.Info.Duration < span.Length && span.Start == TimeSpan.Zero)
            {
                o.WithCustomArgument("-c copy");
                return;
            }

            AddFixes(o, i);

            o.Seek(span.Start);
            if (span.Length != TimeSpan.Zero) o.WithDuration(span.Length);
            if (i.HasVideo) o.FixPlayback();
        });


        private CutSpan Span(IMediaAnalysis i, CutSpan span)
        {
            if     (span.Length < TimeSpan.Zero)          return span with { Length = i.Duration / 2D };
            return (span.Length + span.Start).Ticks > 0 ? span : span with { Length = i.Duration };
        }
    }

    public record CutSpan(TimeSpan Start, TimeSpan Length);
}