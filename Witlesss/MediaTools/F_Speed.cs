using System;
using FFMpegCore;

namespace Witlesss.MediaTools
{
    // -i input [-vf "setpts=0.5*PTS,fps=60"][-s WxH] [-af "atempo=2.0"][-vn] output
    public class F_Speed : F_SingleInput_Base
    {
        private readonly double _speed;

        public F_Speed(string input, double speed) : base(input) => _speed = speed;

        public string ChangeSpeed() => Cook(SetOutName_WEBM_safe(_input, "-S"), Args);

        private void Args(FFMpegArgumentOptions o)
        {
            var i = MediaInfo();
            if (i.video) o = o.WithVideoFilters(v => v.ChangeVideoSpeed(_speed).SetFPS(GetFPS())).FixWebmSize(i.v);
            if (i.audio) o = o.WithAudioFilters(a => a.ChangeAudioSpeed(_speed)).FixSongArt(i.info);

            double GetFPS() => Math.Min(i.v.AvgFrameRate * _speed, 90D);
        }
    }
}