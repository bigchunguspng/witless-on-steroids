using System;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class ChangeSpeed : RemoveBitrate
    {
        public SpeedMode Mode;

        public override void Run()
        {
            string fileID = GetVideoOrAudioID();
            if (fileID == null) return;

            var speed = 2D;
            if (HasDoubleArgument(Text, out double value))
                speed = Mode == SpeedMode.Fast ? Math.Clamp(value, 0.5, 94) : Math.Clamp(value, 0.0107, 2);

            Download(fileID, out string path, out var type);
                    
            string result = Bot.MemeService.ChangeSpeed(path, speed, Mode, type);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> {(Mode == SpeedMode.Fast ? "FAST" : "SLOW" )} [>>]");

            string AudioFilename() => MediaFileName($"Are you {Sender().Split()[0]} or something.mp3");
            string VideoFilename() => $"piece_fap_club-{speed}.mp4";
        }
    }
}