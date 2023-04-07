using System;
using static Witlesss.XD.SpeedMode;

namespace Witlesss.Commands
{
    public class ChangeSpeed : AudioVideoCommand
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
                speed = Mode == Fast ? ClampFast(value) : ClampSlow(value);

            Bot.Download(FileID, Chat, out string path, out var type);
                    
            string result = Memes.ChangeSpeed(path, speed, Mode);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> {(Mode == Fast ? "FAST" : "SLOW" )} [>>]");

            string AudioFilename() => SongNameOr($"Are you {Sender.Split()[0]} or something.mp3");
            string VideoFilename() => $"piece_fap_club-{speed}.mp4";

            double ClampFast(double v) => Math.Clamp(v, 0.5,   94);
            double ClampSlow(double v) => Math.Clamp(v, 0.0107, 2);
        }
    }
}