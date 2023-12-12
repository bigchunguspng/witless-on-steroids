using System;
using FFMpegCore;

namespace Witlesss.MediaTools // ReSharper disable InconsistentNaming
{
    public abstract class F_Action_SingleInput : F_Action
    {
        protected readonly string _input;

        protected F_Action_SingleInput(string input) => _input = input;


        /// <summary> Gets media info + adds fixes to the options </summary>
        protected MediaInfo MediaInfoWithFixing(FFMpegArgumentOptions o)
        {
            var info = MediaInfo();
            AddFixes(o, info);
            return info;
        }
        protected MediaInfo MediaInfo()
        {
            var info = FFProbe.Analyse(_input);
            var v = info.PrimaryVideoStream;
            var a = info.PrimaryAudioStream;
            var audio = a is not null;
            var video = v is { AvgFrameRate: not double.NaN };

            return new MediaInfo(info, audio, video, v);
        }
        protected static void AddFixes(FFMpegArgumentOptions o, MediaInfo i)
        {
            if (i.video) o.FixWebmSize(i.v);
            if (i.audio) o.FixSongArt(i.info);
        }

        public double GetFramerate() => Math.Max(MediaInfo().v.AvgFrameRate, 13); // magic moment

        
        protected override string NameSource => _input;

        protected override string Cook(string output)
        {
            Run(FFMpegArguments.FromFileInput(_input).OutputToFile(output, addArguments: Action));
            return output;
        }
    }

    public record MediaInfo(IMediaAnalysis info, bool audio, bool video, VideoStream v);
}