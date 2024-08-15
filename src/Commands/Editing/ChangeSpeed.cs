using static Witlesss.Commands.Editing.ChangeSpeed.Mode;

namespace Witlesss.Commands.Editing
{
    public class ChangeSpeed : AudioVideoCommand
    {
        private double _speed;
        private Mode _mode;

        public ChangeSpeed SetMode(Mode mode)
        {
            _mode = mode;
            return this;
        }

        protected override async Task Execute()
        {
            _speed = Context.HasDoubleArgument(out var x) ? _mode == Fast ? ClampFast(x) : ClampSlow(x) : 2D;

            if (_mode == Slow) _speed = 1 / _speed;

            var (path, type) = await Bot.Download(FileID, Chat);

            var result = await FFMpegXD.ChangeSpeed(path, _speed);
            SendResult(result, type);
            Log($"{Title} >> {(_mode == Fast ? "FAST" : "SLOW" )} [>>]");

            double ClampFast(double v) => Math.Clamp(v, 0.5,   94);
            double ClampSlow(double v) => Math.Clamp(v, 0.0107, 2);
        }

        protected override string AudioFileName => SongNameOr($"Are you {Sender.Split()[0]} or something.mp3");
        protected override string VideoFileName => $"piece_fap_club-{_speed}.mp4";

        public enum Mode
        {
            Fast, Slow
        }
    }
}