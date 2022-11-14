using System;
using System.Collections.Generic;
using static Witlesss.MediaType;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp4" -filter_complex "[0:v]setpts=0.5*PTS,fps=60;[0:a]atempo=2.0" output.mp4
    // ffmpeg -i "input.mp4" -filter:v       "setpts=0.5*PTS,fps=60"                      output.mp4
    // ffmpeg -i "input.mp3" -filter:a       "atempo=2.0"                             -vn output.mp3
    public class F_Speed : F_Base
    {
        private readonly double _speed, _fps;

        public F_Speed(string input, double speed, MediaType type, double fps = 0) : base(SetOutName(input, "-S"))
        {
            _speed = speed;
            _fps = fps;

            AddInput(input);
            AddOptions(FiltersNames[type], Filter(type));
            AddSizeFix(type, input);
            AddSongFix(type);
        }

        private static readonly Dictionary<MediaType, string> FiltersNames = new()
        {
            { Audio, "-filter:a"       },
            { Video, "-filter:v"       },
            { Movie, "-filter_complex" }
        };

        private string FilterAudio() => $"atempo={FormatDouble(_speed)}";
        private string FilterVideo() => $"setpts={FormatDouble(1 / _speed)}*PTS,fps={FormatDouble(_fps)}";
        private string FilterMovie() => $"[0:v]{FilterVideo()};[0:a]{FilterAudio()}";
        private string Filter(MediaType type) => type switch
        {
            Audio => FilterAudio(),
            Video => FilterVideo(),
            Movie => FilterMovie(),
            _     => throw new ArgumentOutOfRangeException()
        };
    }
}