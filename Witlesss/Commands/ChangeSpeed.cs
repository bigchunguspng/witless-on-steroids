using System;

namespace Witlesss.Commands
{
    public class ChangeSpeed : RemoveBitrate
    {
        private SpeedMode Mode;

        public ChangeSpeed SetMode(SpeedMode mode)
        {
            Mode = mode;
            return this;
        }

        public override void Run()
        {
            if (NothingToProcess()) return;

            var speed = 2D;
            if (HasDoubleArgument(Text, out double value))
                speed = Mode == SpeedMode.Fast ? Math.Clamp(value, 0.5, 94) : Math.Clamp(value, 0.0107, 2);

            Bot.Download(FileID, Chat, out string path, out var type);
                    
            string result = Memes.ChangeSpeed(path, speed, Mode);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> {(Mode == SpeedMode.Fast ? "FAST" : "SLOW" )} [>>]");

            string AudioFilename() => MediaFileName($"Are you {Sender.Split()[0]} or something.mp3");
            string VideoFilename() => $"piece_fap_club-{speed}.mp4";
        }
    }
}